using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FS.DI;
using FS.Extends;
using FS.MQ.RedisStream;
using FS.MQ.RedisStream.Attr;
using FSS.Abstract.Enum;
using FSS.Abstract.Server.MetaInfo;
using FSS.Abstract.Server.RegisterCenter;
using FSS.Abstract.Server.Scheduler;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace FSS.Com.SchedulerServer.Scheduler
{
    /// <summary>
    /// 新任务调度
    /// </summary>
    [Consumer(Enable = true, RedisName = "default", QueueName = "TaskScheduler", PullCount = 5, ConsumeThreadNums = 1)]
    public class TaskSchedulerConsumer : IListenerMessage
    {
        public IClientRegister ClientRegister { get; set; }
        public ITaskScheduler  TaskScheduler  { get; set; }
        public ITaskGroupList  TaskGroupList  { get; set; }
        public ITaskInfo       TaskInfo       { get; set; }
        public IIocManager     IocManager     { get; set; }

        /// <summary>
        /// 消费
        /// </summary>
        public async Task<bool> Consumer(StreamEntry[] messages, ConsumeContext ea)
        {
            var       logger       = IocManager.Logger<TaskSchedulerConsumer>();
            var       dicTaskGroup = await TaskGroupList.ToListByMemoryAsync();
            foreach (var message in messages)
            {
                Stopwatch sw          = Stopwatch.StartNew();
                var       taskGroupId = message.Values[0].Value.ToString().ConvertType(0);
                if (taskGroupId == 0)
                {
                    await ea.Ack(message);
                    continue;
                }

                var       taskVO = await TaskInfo.ToInfoByGroupIdAsync(taskGroupId);
                // 如果任务在当前节点没有客户端连接
                if (!ClientRegister.Exists(taskVO.JobName))
                {
                    // 取出全局客户端，如果都没有，则返回true，删除消息
                    var lst = await ClientRegister.ToListByMemoryAsync();
                    if (lst == null || lst.Count == 0)
                    {
                        await ea.Ack(message);
                        continue;
                    }

                    var result = lst.All(o => o.JobName != taskVO.JobName);
                    if (result)
                    {
                        await ea.Ack(message);
                    }
                    continue;
                }

                if (!dicTaskGroup.ContainsKey(taskVO.TaskGroupId)) continue;
                if (taskVO.Status != EumTaskType.None)
                {
                    await ea.Ack(message);
                    continue;
                }

                try
                {
                    await TaskScheduler.Scheduler(dicTaskGroup[taskVO.TaskGroupId], taskVO);
                    logger.LogInformation($"统计：调度【{taskVO.Caption} ({taskVO.JobName})】耗时：{sw.ElapsedMilliseconds} ms");
                    await ea.Ack(message);
                }
                catch (Exception e)
                {
                    logger.LogError(e.Message);
                    return false;
                }
            }

            return false;
        }

        public Task<bool> FailureHandling(StreamEntry[] messages, ConsumeContext ea) => throw new System.NotImplementedException();
    }
}