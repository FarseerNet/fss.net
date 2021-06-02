using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FS.Cache.Redis;
using FS.DI;
using FS.MQ.RedisStream;
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
        public ITaskInfo          TaskInfo          { get; set; }
        public IClientRegister    ClientRegister    { get; set; }
        public ITaskGroupList     TaskGroupList     { get; set; }
        public ITaskList          TaskList          { get; set; }
        public ITaskUpdate        TaskUpdate        { get; set; }
        public IIocManager        IocManager        { get; set; }
        public IRunLogAdd         RunLogAdd         { get; set; }
        public IRedisCacheManager RedisCacheManager { get; set; }

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
                        var dicTaskGroup = await TaskGroupList.ToListByMemoryAsync();
                        var lstTask      = await TaskInfo.ToGroupListAsync();
                        
                        // 找出未执行的任务列表
                        var lstNoneTask  = await TaskList.ToNoneListAsync();
                        foreach (var task in lstNoneTask.Where(taskVO => lstTask.All(o => o.Id != taskVO.Id)))
                        {
                            // 强制设为失败
                            await RunLogAdd.AddAsync(task.TaskGroupId, task.Id, LogLevel.Warning, $"任务ID：{task.Id}，与当前任务组正在执行的任务不一致，强制设为失败状态");
                            task.Status = EumTaskType.Fail;
                            await TaskUpdate.SaveAsync(task);
                        }
                        
                        // 注册进来的客户端，必须是能处理的，否则退出线程
                        var lstStatusNone = lstTask.FindAll(o => ClientRegister.Exists(o.JobName));
                        if (lstStatusNone == null || lstStatusNone.Count == 0)
                        {
                            await Task.Delay(5000);
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
                            await Task.Delay(5000);
                            continue;
                        }

                        var streamRange = await RedisCacheManager.Db.StreamRangeAsync("TaskScheduler");
                        foreach (var taskVO in lstStatusNone)
                        {
                            if (streamRange.Any(o=>o.Values[0].Value.ToString() == taskVO.TaskGroupId.ToString())) continue;
                            await IocManager.Resolve<IRedisStreamProduct>("TaskScheduler").SendAsync(taskVO.TaskGroupId.ToString());
                        }
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, e.Message);
                        // 休眠下，防止CPU过高
                        await Task.Delay(100);
                    }
                    await Task.Delay(5000);
                }
            });
            return Task.FromResult(0);
        }
    }
}