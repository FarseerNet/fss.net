using System;
using System.Threading;
using System.Threading.Tasks;
using FS.DI;
using FSS.Abstract.Enum;
using FSS.Abstract.Server.MetaInfo;
using FSS.Abstract.Server.Scheduler;
using Microsoft.Extensions.Logging;

namespace FSS.Com.SchedulerServer.Scheduler
{
    public class WhenTaskStatusWorking : IWhenTaskStatus
    {
        public ITaskInfo          TaskInfo           { get; set; }
        public ITaskList          TaskList           { get; set; }
        public ITaskGroupList     TaskGroupList      { get; set; }
        public ILogger            Logger             { get; set; }
        public IIocManager        IocManager         { get; set; }
        public CheckClientOffline CheckClientOffline { get; set; }

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
                        var lstTask = await TaskList.ToSchedulerWorkingListAsync();
                        foreach (var task in lstTask)
                        {
                            if (task != null)
                            {
                                // 状态必须是 完成的
                                if (task.Status != EumTaskType.Scheduler && task.Status != EumTaskType.Working) continue;
                                // 加个时间，来限制并发
                                if (task.Status == EumTaskType.Scheduler && (DateTime.Now - task.StartAt).TotalSeconds < 5) continue;
                                if (task.Status == EumTaskType.Working   && (DateTime.Now - task.RunAt).TotalSeconds   < 5) continue;

                                // 检查是否离线
                                await CheckClientOffline.Check(task);
                                await Task.Delay(200);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.LogError(e, e.Message);
                    }

                    // 休眠下，防止CPU过高
                    await Task.Delay(TimeSpan.FromSeconds(30));
                }
            });
            return Task.FromResult(0);
        }
    }
}