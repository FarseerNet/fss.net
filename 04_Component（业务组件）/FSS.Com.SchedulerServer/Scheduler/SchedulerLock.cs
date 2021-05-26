using System;
using System.Threading.Tasks;
using FS.Cache.Redis;
using FSS.Abstract.Server.Scheduler;
using StackExchange.Redis;

namespace FSS.Com.SchedulerServer.Scheduler
{
    public class SchedulerLock : ISchedulerLock
    {
        private string             Key(int taskId)   => $"SchedulerLock:{taskId}";
        public  IRedisCacheManager RedisCacheManager { get; set; }

        /// <summary>
        /// 锁住任务，只允许一个节点对其调度
        /// </summary>
        public bool TryLock(int taskId, string serverNode)
        {
            var key = Key(taskId);
            return RedisCacheManager.Db.StringSet(key, serverNode, TimeSpan.FromMinutes(2), When.NotExists);
        }
        
        /// <summary>
        /// 删除锁
        /// </summary>
        public Task ClearLock(int taskId)
        {
            var key = Key(taskId);
            return RedisCacheManager.Db.KeyDeleteAsync(key);
        }
    }
}