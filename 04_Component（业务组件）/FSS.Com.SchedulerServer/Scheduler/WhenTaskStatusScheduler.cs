using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FS.DI;
using FSS.Abstract.Enum;
using FSS.Abstract.Server.MetaInfo;
using FSS.Abstract.Server.RegisterCenter;
using FSS.Abstract.Server.Scheduler;
using Microsoft.Extensions.Logging;

namespace FSS.Com.SchedulerServer.Scheduler
{
    public class WhenTaskStatusScheduler : IWhenTaskStatus
    {
        public static           bool            IsRun;
        public                  ITaskInfo       TaskInfo       { get; set; }
        public                  IClientRegister ClientRegister { get; set; }
        public                  ITaskGroupList  TaskGroupList  { get; set; }
        public                  ILogger         Logger         { get; set; }
        public                  IIocManager     IocManager     { get; set; }
        public                  ITaskUpdate     TaskUpdate     { get; set; }
        public                  IRunLogAdd      RunLogAdd      { get; set; }
        private static readonly object          ObjLock = new();

        /// <summary>
        /// 运行当状态为Node的任务
        /// </summary>
        public Task Run()
        {
            Logger = IocManager.Logger<WhenTaskStatusScheduler>();

            // 当前没有客户端连接时，休眠
            if (ClientRegister.Count() == 0)
            {
                Logger.LogDebug($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} 当前没有客户端连接，Scheduler休眠...");
                return Task.FromResult(0);
            }

            if (IsRun) return Task.FromResult(0);
            lock (ObjLock)
            {
                if (IsRun) return Task.FromResult(0);
                IsRun = true;
            }

            ThreadPool.QueueUserWorkItem(async _ =>
            {
                while (ClientRegister.Count() > 0)
                {
                    IsRun = true;
                    try
                    {
                        var dicTaskGroup = (await TaskGroupList.ToListAndSaveAsync()).ToDictionary(o => o.Id, o => o);
                        var lstTask      = await TaskInfo.ToGroupListAsync();

                        // 注册进来的客户端，必须是能处理的，否则退出线程
                        var lstStatusScheduler = lstTask.FindAll(o => ClientRegister.Count(dicTaskGroup[o.TaskGroupId].JobTypeName) > 0);
                        if (lstStatusScheduler == null || lstStatusScheduler.Count == 0) return;

                        // 取出状态为Scheduler的，且调度时间超过2S的
                        lstStatusScheduler = lstStatusScheduler.FindAll(o =>
                                o.Status == EumTaskType.Scheduler &&                      // 状态必须是 EumTaskType.None
                                (DateTime.Now - o.SchedulerAt).TotalMilliseconds >= 3000) // 执行时间在50ms内
                            .OrderBy(o => o.SchedulerAt).ToList();

                        // 没有任务符合条件
                        if (lstStatusScheduler == null || lstStatusScheduler.Count == 0)
                        {
                            await Task.Delay(2000);
                            continue;
                        }

                        foreach (var taskGroupId in lstStatusScheduler.Select(o => o.TaskGroupId))
                        {
                            // 重新取一遍，担心正好数据被正确处理好了
                            var task = await TaskInfo.ToGroupAsync(taskGroupId);

                            // 说明已调度成功
                            if (task.Status != EumTaskType.Scheduler) break;

                            // 处于Scheduler状态，如果时间>2S，认为客户端无法处理当前JOB，重新调度
                            var taskTimeSpan = DateTime.Now - task.SchedulerAt;
                            await RunLogAdd.AddAsync(task.TaskGroupId, task.Id, LogLevel.Warning, $"任务ID：{task.Id}，已调度，{(int) taskTimeSpan.TotalMilliseconds} ms未执行，重新调度");

                            // 标记为重新调度
                            task.Status = EumTaskType.ReScheduler;
                            await TaskUpdate.SaveAsync(task);

                            // 休眠下，防止CPU过高
                            await Task.Delay(10);
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.LogError(e, e.Message);
                    }

                    // 休眠下，防止CPU过高
                    await Task.Delay(100);
                }

                IsRun = false;
            });
            return Task.FromResult(0);
        }
    }
}