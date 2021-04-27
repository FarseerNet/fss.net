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
using FSS.Com.RegisterCenterServer.Abstract;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace FSS.Com.SchedulerServer.Scheduler
{
    public class TaskGroupScheduler : ITaskGroupScheduler
    {
        public                  IIocManager           IocManager       { get; set; }
        public                  ITaskInfo             TaskInfo         { get; set; }
        public                  ITaskUpdate           TaskUpdate       { get; set; }
        public                  IClientSlb            ClientSlb        { get; set; }
        public                  IClientNotifyGrpc     ClientNotifyGrpc { get; set; }
        public                  ITaskAdd              TaskAdd          { get; set; }
        public                  IClientEndpoint       ClientEndpoint   { get; set; }
        private static readonly Dictionary<int, bool> DicSleep = new();

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
                DicSleep[groupIdState] = false;

                IocManager.Logger<TaskGroupScheduler>().LogInformation($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} 任务组：{taskGroupId} 执行任务触发器");
                while (true)
                {
                    // 取出Task
                    var task = TaskInfo.ToGroupTask(taskGroupId);
                    if (task.Status is EumTaskType.Fail or EumTaskType.Success)
                    {
                        task                   = TaskAdd.Create(taskGroupId);
                        DicSleep[groupIdState] = false;
                    }

                    // 大于2S，说明没有线程在执行当前任务
                    if (!DicSleep[groupIdState])
                    {
                        // 开启一个线程执行
                        Scheduler(taskGroupId: taskGroupId, task: task);
                        DicSleep[groupIdState] = true;
                    }

                    while (DicSleep[groupIdState])
                    {
                        Thread.Sleep(10);
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

                var timeSpan = task.StartAt - DateTime.Now;
                while (timeSpan.TotalMilliseconds > 0)
                {
                    // 休眠500ms
                    Thread.Sleep(timeSpan.TotalMilliseconds > 500 ? 500 : (int) timeSpan.TotalMilliseconds);
                    timeSpan = task.StartAt - DateTime.Now;
                }

                // 取出空闲客户端、开始调度执行
                var clientVO = ClientSlb.Slb();
                try
                {
                    // 当前没有客户端注册进来
                    if (clientVO == null)
                    {
                        IocManager.Logger<TaskGroupScheduler>().LogWarning($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} 任务：{taskVO.TaskGroupId}-{taskVO.Id} 需要在（{taskVO.StartAt:yyyy-MM-dd HH:mm:ss}）执行，但没有找到可以调度的客户端");
                        Thread.Sleep(5000);
                        return;
                    }

                    IocManager.Logger<TaskGroupScheduler>().LogInformation($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} 任务：{taskVO.TaskGroupId}-{taskVO.Id} 调度给{clientVO.Endpoint} 执行");
                    var result = ClientNotifyGrpc.Invoke(clientVO, taskVO).Result;
                    // 不成功，则暂停3秒
                    if (result.Status != EumTaskType.Success)
                    {
                        IocManager.Logger<TaskGroupScheduler>().LogWarning($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} 任务：{taskVO.TaskGroupId}-{taskVO.Id} 执行失败");
                        Thread.Sleep(3000);
                    }
                    else
                    {
                        IocManager.Logger<TaskGroupScheduler>().LogInformation($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} 任务：{taskVO.TaskGroupId}-{taskVO.Id} 执行成功，耗时：{result.RunSpeed} ms");
                    }
                }
                catch (Exception e)
                {
                    if (e.InnerException != null) e = e.InnerException;

                    var statusCode = StatusCode.Unknown;
                    var msg        = e.Message;
                    if (e is RpcException rpcException1)
                    {
                        msg        = rpcException1.Status.Detail;
                        statusCode = rpcException1.Status.StatusCode;
                    }

                    // 客户端断开连接
                    if (statusCode == StatusCode.Unavailable)
                    {
                        ClientSlb.Remove(clientVO.Id);
                        IocManager.Logger<TaskGroupScheduler>().LogWarning(msg);
                    }
                    else
                        IocManager.Logger<TaskGroupScheduler>().LogError(msg);

                    task.Status = EumTaskType.Fail;
                    TaskUpdate.Save(task);
                    Thread.Sleep(3000);
                }
                finally
                {
                    TaskAdd.Create(taskGroupId);
                    DicSleep[task.TaskGroupId] = false;

                    // 更新客户端统计
                    if (clientVO != null)
                    {
                        clientVO.UseAt = DateTime.Now;
                        ClientEndpoint.Save(clientVO.Id, clientVO);
                    }
                }
            }, task);
        }
    }
}