using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FS.DI;
using FSS.Abstract.Entity.MetaInfo;
using FSS.Abstract.Enum;
using FSS.Abstract.Server.MetaInfo;
using FSS.Abstract.Server.RegisterCenter;
using FSS.Abstract.Server.RemoteCall;
using FSS.Abstract.Server.Scheduler;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace FSS.Com.SchedulerServer.Scheduler
{
    public class WhenTaskStatusNone : IWhenTaskStatus
    {
        public ITaskInfo       TaskInfo       { get; set; }
        public IClientRegister ClientRegister { get; set; }
        public ITaskGroupList  TaskGroupList  { get; set; }
        public ILogger         Logger         { get; set; }
        public IIocManager     IocManager     { get; set; }
        public ITaskScheduler  TaskScheduler  { get; set; }

        /// <summary>
        /// 运行当状态为Node的任务
        /// </summary>
        public Task Run()
        {
            Logger = IocManager.Logger<WhenTaskStatusNone>();

            ThreadPool.QueueUserWorkItem(async _ =>
            {
                while (true)
                {
                    try
                    {
                        var dicTaskGroup = (await TaskGroupList.ToListAsync()).ToDictionary(o => o.Id, o => o);
                        var lstTask      = await TaskInfo.ToGroupListAsync();

                        // 注册进来的客户端，必须是能处理的，否则退出线程
                        var lstStatusNone = lstTask.FindAll(o => ClientRegister.Count(dicTaskGroup[o.TaskGroupId].JobName) > 0);
                        if (lstStatusNone == null || lstStatusNone.Count == 0)
                        {
                            await Task.Delay(3000);
                            continue;
                        }

                        // 取出状态为None的，且马上到时间要处理的
                        lstStatusNone = lstStatusNone.FindAll(o =>
                                o.Status == EumTaskType.None &&                 // 状态必须是 EumTaskType.None
                                (o.StartAt - DateTime.Now).TotalMinutes <= 1 && // 执行时间在1分钟内
                                dicTaskGroup[o.TaskGroupId].IsEnable)           // 任务组必须是开启
                            .OrderBy(o => o.StartAt).ToList();

                        // 没有任务需要调度
                        if (lstStatusNone == null || lstStatusNone.Count == 0)
                        {
                            //Logger.LogDebug($"没有任务需要调度");
                            await Task.Delay(50);
                            continue;
                        }

                        //Parallel.ForEach(lstStatusNone.Select(o => o.TaskGroupId), new ParallelOptions(), async taskGroupId =>
                        //{

                        var lstSchedulerTask = new List<Task>();
                        foreach (var task in lstStatusNone)
                        {
                            // 重新取一遍，担心正好数据被正确处理好了
                            //var task = await TaskInfo.ToGroupAsync(taskGroupId);

                            //var taskGroup = dicTaskGroup[task.TaskGroupId];
                            lstSchedulerTask.Add(TaskScheduler.Scheduler(dicTaskGroup[task.TaskGroupId], task));

                            // 休眠下，防止CPU过高
                            //await Task.Delay(10);
                        }

                        await Task.WhenAll(lstSchedulerTask);
                        //});
                        continue;
                    }
                    catch (Exception e)
                    {
                        Logger.LogError(e, e.Message);
                        // 休眠下，防止CPU过高
                        await Task.Delay(100);
                    }
                }
            });
            return Task.FromResult(0);
        }
    }
}