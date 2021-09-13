using System;
using System.Threading;
using System.Threading.Tasks;
using FS.DI;
using FSS.Abstract.Enum;
using FSS.Abstract.Server.MetaInfo;
using FSS.Abstract.Server.Scheduler;
using FSS.Com.SchedulerServer.Abstract;
using Microsoft.Extensions.Logging;

namespace FSS.Com.SchedulerServer.Scheduler
{
    public class WhenTaskStatusWorking : IWhenTaskStatus
    {
        public ITaskList           TaskList           { get; set; }
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
                        var lstTask      = await TaskList.ToSchedulerWorkingListAsync();
                        lstTask.RemoveAll(o => o.Status is EumTaskType.Scheduler && (DateTime.Now - o.StartAt).TotalSeconds < 5);                                              // 调度状态、且计划时间在5秒内还没执行的，暂停认为正常
                        lstTask.RemoveAll(o => o.Status is EumTaskType.Working && (DateTime.Now - o.RunAt).TotalMilliseconds < dicTaskGroup[o.TaskGroupId].RunSpeedAvg * 1.3); // 执行时间还没有超出平均运行时间1.3倍

                        // 检查是否离线
                        foreach (var task in lstTask)
                        {
                            await CheckClientOffline.Check(task);
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