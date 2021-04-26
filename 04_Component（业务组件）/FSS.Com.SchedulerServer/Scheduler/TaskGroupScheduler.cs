using System;
using System.Collections.Generic;
using System.Threading;
using FS.DI;
using FSS.Abstract.Entity.MetaInfo;
using FSS.Abstract.Enum;
using FSS.Abstract.Server.MetaInfo;
using FSS.Abstract.Server.RegisterCenter;
using FSS.Abstract.Server.RemoteCall;
using FSS.Abstract.Server.Scheduler;
using Microsoft.Extensions.Logging;

namespace FSS.Com.SchedulerServer.Scheduler
{
    public class TaskGroupScheduler : ITaskGroupScheduler
    {
        public                  IIocManager          IocManager       { get; set; }
        public                  ITaskInfo            TaskInfo         { get; set; }
        public                  ITaskUpdate          TaskUpdate       { get; set; }
        public                  IClientSlb           ClientSlb        { get; set; }
        public                  IClientNotifyGrpc    ClientNotifyGrpc { get; set; }
        public                  ITaskAdd             TaskAdd          { get; set; }
        private static readonly Dictionary<int, int> DicSleep = new();

        /// <summary>
        /// 根据任务组ID，进行任务调度
        /// </summary>
        /// <param name="taskGroupId">任务组ID</param>
        public void Scheduler(int taskGroupId)
        {
            // 如果已有该任务组的时间器，才不需要再执行
            if (DicSleep.ContainsKey(taskGroupId)) return;

            // 开启单个任务组线程，判断是否要创建任务
            ThreadPool.QueueUserWorkItem(taskGroupIdState =>
            {
                // 默认500Ms执行一次
                var groupIdState = (int) taskGroupIdState;
                DicSleep[groupIdState] = 500;
                while (true)
                {
                    // 取出Task
                    var task = TaskInfo.ToGroupTask(taskGroupId);
                    if (task.Status is EumTaskType.Fail or EumTaskType.Success)
                    {
                        task = TaskAdd.Create(taskGroupId);
                    }

                    // 大于2S，说明没有线程在执行当前任务
                    if ((DateTime.Now - task.SchedulerActiveAt).TotalSeconds > 2)
                    {
                        // 开启一个线程执行
                        Scheduler(taskGroupId: taskGroupId, task: task);
                    }

                    while (DicSleep[groupIdState] > 0)
                    {
                        Thread.Sleep(5);
                        DicSleep[groupIdState] -= 5;
                    }
                }
            }, taskGroupId);
        }

        /// <summary>
        /// 开启一个线程执行，执行调度
        /// </summary>
        private void Scheduler(int taskGroupId, TaskVO task)
        {
            ThreadPool.QueueUserWorkItem(taskState =>
            {
                var taskVO = (TaskVO) taskState;

                // 更新当前调度活动时间
                task.SchedulerActiveAt = DateTime.Now;
                TaskUpdate.Update(task);

                var timeSpan = DateTime.Now - task.StartAt;
                while (timeSpan.TotalMilliseconds > 0)
                {
                    // 休眠500ms
                    Thread.Sleep(timeSpan.TotalMilliseconds > 500 ? 500 : (int) timeSpan.TotalMilliseconds);

                    // 更新当前调度活动时间，证明线程还活着
                    taskVO.SchedulerActiveAt = DateTime.Now;
                    TaskUpdate.Update(task);

                    timeSpan = DateTime.Now - task.StartAt;
                }

                // 取出空间客户端、开始调度执行
                try
                {
                    var clientVO = ClientSlb.Slb();
                    
                    // 当前没有客户端注册进来
                    if (clientVO == null)
                    {
                        taskVO.SchedulerActiveAt = DateTime.MinValue;
                        TaskUpdate.Update(task);
                        return;
                    }
                    ClientNotifyGrpc.Invoke(clientVO, taskVO);
                }
                catch (Exception e)
                {
                    IocManager.Logger<TaskGroupScheduler>().LogError(e, e.Message);
                    task.Status = EumTaskType.Fail;
                    TaskUpdate.Save(task);
                }
                finally
                {
                    TaskAdd.Create(taskGroupId);
                    DicSleep[task.TaskGroupId] = 0;
                }
            }, task);
        }
    }
}