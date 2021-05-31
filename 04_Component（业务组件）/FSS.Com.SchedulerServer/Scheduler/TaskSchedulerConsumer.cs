using System;
using System.Threading.Tasks;
using FS.DI;
using FS.Extends;
using FS.MQ.RedisStream;
using FS.MQ.RedisStream.Attr;
using FSS.Abstract.Entity.MetaInfo;
using FSS.Abstract.Server.MetaInfo;
using FSS.Abstract.Server.RegisterCenter;
using FSS.Abstract.Server.Scheduler;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace FSS.Com.SchedulerServer.Scheduler
{
    /// <summary>
    /// 新任务调度
    /// </summary>
    [Consumer(Enable = true, RedisName = "default", QueueName = "TaskScheduler", PullCount = 1, ConsumeThreadNums = 1)]
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
            var dicTaskGroup = await TaskGroupList.ToListByMemoryAsync();
            var logger       = IocManager.Logger<TaskSchedulerConsumer>();
            foreach (var message in messages)
            {
                var taskGroupId = message.Values[0].Value.ToString().ConvertType(0);
                if (taskGroupId == 0)
                {
                    await ea.Ack(message);
                    continue;
                }
                
                var taskVO      = await TaskInfo.ToInfoByGroupIdAsync(taskGroupId);
                // 如果任务在当前节点没有客户端连接，则跳过，不处理
                if (!ClientRegister.Exists(taskVO.JobName)) continue;
                if (!dicTaskGroup.ContainsKey(taskVO.TaskGroupId)) continue;
                try
                {
                    await TaskScheduler.Scheduler(dicTaskGroup[taskVO.TaskGroupId], taskVO);
                    await ea.Ack(message);
                }
                catch (Exception e)
                {
                    logger.LogError(e.Message);
                }
            }

            return true;
        }

        public Task<bool> FailureHandling(StreamEntry[] messages, ConsumeContext ea) => throw new System.NotImplementedException();
    }
}