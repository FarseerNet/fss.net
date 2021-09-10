using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FS.DI;
using FSS.Abstract.Entity.MetaInfo;
using FSS.Abstract.Enum;
using FSS.Abstract.Server.MetaInfo;
using FSS.Abstract.Server.RegisterCenter;
using FSS.Abstract.Server.Scheduler;
using FSS.Com.SchedulerServer.Abstract;
using Microsoft.Extensions.Logging;

namespace FSS.Com.SchedulerServer.Scheduler
{
    public class WhenTaskStatusWorking : IWhenTaskStatus
    {
        public ITaskInfo           TaskInfo           { get; set; }
        public ITaskGroupList      TaskGroupList      { get; set; }
        public ILogger             Logger             { get; set; }
        public IIocManager         IocManager         { get; set; }
        public ICheckClientOffline CheckClientOffline { get; set; }

        /// <summary>
        /// 运行当状态为Node的任务
        /// </summary>
        public Task Run()
        {
            Logger = IocManager.Logger<WhenTaskStatusWorking>();
            ThreadPool.QueueUserWorkItem(async _ =>
            {
                while (true)
                {
                    try
                    {
                        var dicTaskGroup = await TaskGroupList.ToListByMemoryAsync();
                        var lstTask      = await TaskInfo.ToGroupListAsync();

                        // 取出马上到时间要处理的
                        var lstStatusWorking = lstTask.FindAll(o =>
                            (o.Status is EumTaskType.Working or EumTaskType.Scheduler) &&                                        // 状态必须是 工作中或已调度
                            (DateTime.Now - o.SchedulerAt).TotalMilliseconds >= dicTaskGroup[o.TaskGroupId].RunSpeedAvg * 1.3 && // 执行时间超出平均运行时间1.3倍
                            dicTaskGroup[o.TaskGroupId].IsEnable);                                                               // 任务组必须是开启

                        // 检查是否离线
                        foreach (var taskGroupId in lstStatusWorking.Select(o => o.TaskGroupId))
                        {
                            await CheckClientOffline.Check(taskGroupId);
                            await Task.Delay(10);
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.LogError(e, e.Message);
                    }

                    // 休眠下，防止CPU过高
                    await Task.Delay(5000);
                }
            });
            return Task.FromResult(0);
        }
    }
}