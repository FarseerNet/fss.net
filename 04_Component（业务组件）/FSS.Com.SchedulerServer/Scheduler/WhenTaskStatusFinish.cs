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
    public class WhenTaskStatusFinish : IWhenTaskStatus
    {
        public static           bool            IsRun;
        public                  ITaskInfo       TaskInfo       { get; set; }
        public                  IClientRegister ClientRegister { get; set; }
        public                  ITaskGroupList  TaskGroupList  { get; set; }
        public                  ILogger         Logger         { get; set; }
        public                  IIocManager     IocManager     { get; set; }
        public                  ITaskAdd        TaskAdd        { get; set; }
        private static readonly object          ObjLock = new();

        /// <summary>
        /// 运行当状态为Node的任务
        /// </summary>
        public Task Run()
        {
            Logger = IocManager.Logger<WhenTaskStatusFinish>();

            // 当前没有客户端连接时，休眠
            if (ClientRegister.Count() == 0)
            {
                Logger.LogDebug($"当前没有客户端连接，Working休眠...");
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
                        var lstStatusFinish = lstTask.FindAll(o => ClientRegister.Count(dicTaskGroup[o.TaskGroupId].JobName) > 0);
                        if (lstStatusFinish == null || lstStatusFinish.Count == 0) return;

                        // 取出状态为None的，且马上到时间要处理的
                        lstStatusFinish = lstStatusFinish.FindAll(o =>
                                o.Status is EumTaskType.Fail or EumTaskType.Success or EumTaskType.ReScheduler && // 状态必须是 EumTaskType.None
                                dicTaskGroup[o.TaskGroupId].IsEnable)                                             // 任务组必须是开启
                            .OrderBy(o => o.StartAt).ToList();

                        // 没有任务需要调度
                        if (lstStatusFinish == null || lstStatusFinish.Count == 0)
                        {
                            await Task.Delay(5000);
                            continue;
                        }

                        foreach (var task in lstStatusFinish)
                        {
                            await TaskAdd.GetOrCreateAsync(task.TaskGroupId);
                            Logger.LogDebug($"\t1、新建任务: GroupId={task.TaskGroupId} TaskId={task.Id}");
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