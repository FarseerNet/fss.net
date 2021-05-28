using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public IIocManager     IocManager     { get; set; }
        public ITaskScheduler  TaskScheduler  { get; set; }

        /// <summary>
        /// 运行当状态为Node的任务
        /// </summary>
        public Task Run()
        {
            var logger = IocManager.Logger<WhenTaskStatusNone>();

            ThreadPool.QueueUserWorkItem(async _ =>
            {
                while (true)
                {
                    try
                    {
                        Stopwatch sw           = Stopwatch.StartNew();
                        var       dicTaskGroup = await TaskGroupList.ToListByMemoryAsync();
                        var       lstTask      = await TaskInfo.ToGroupListAsync();

                        // 注册进来的客户端，必须是能处理的，否则退出线程
                        var lstStatusNone = lstTask.FindAll(o => ClientRegister.Exists(dicTaskGroup[o.TaskGroupId].JobName));
                        if (lstStatusNone == null || lstStatusNone.Count == 0)
                        {
                            await Task.Delay(3000);
                            continue;
                        }

                        // 取出状态为None的，且马上到时间要处理的
                        lstStatusNone = lstStatusNone.FindAll(o =>
                                o.Status == EumTaskType.None &&                 // 状态必须是 EumTaskType.None
                                (o.StartAt - DateTime.Now).TotalMinutes <= 2 && // 执行时间在1分钟内
                                dicTaskGroup[o.TaskGroupId].IsEnable)           // 任务组必须是开启
                            .OrderBy(o => o.StartAt).ToList();
                        
                        // 没有任务需要调度
                        if (lstStatusNone == null || lstStatusNone.Count == 0)
                        {
                            await Task.Delay(200);
                            continue;
                        }
                        logger.LogInformation($"取任务{lstStatusNone.Count} 条任务，共耗时：{sw.ElapsedMilliseconds} ms");
                        var lstSchedulerTask = lstStatusNone.Select(task => TaskScheduler.Scheduler(dicTaskGroup[task.TaskGroupId], task)).ToList();
                        await Task.WhenAll(lstSchedulerTask);
                        
                        logger.LogInformation($"调度{lstStatusNone.Count} 条任务，共耗时：{sw.ElapsedMilliseconds} ms");
                        //logger.LogInformation("--------------------");
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, e.Message);
                        // 休眠下，防止CPU过高
                        await Task.Delay(100);
                    }
                    await Task.Delay(50);
                }
            });
            return Task.FromResult(0);
        }
    }
}