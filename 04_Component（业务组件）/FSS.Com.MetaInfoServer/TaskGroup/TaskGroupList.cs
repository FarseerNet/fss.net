using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FS.Cache;
using FS.Cache.Redis;
using FS.DI;
using FS.Extends;
using FSS.Abstract.Entity.MetaInfo;
using FSS.Abstract.Server.MetaInfo;
using FSS.Com.MetaInfoServer.TaskGroup.Dal;
using FSS.Infrastructure.Repository;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace FSS.Com.MetaInfoServer.TaskGroup
{
    /// <summary>
    /// 任务列表
    /// </summary>
    public class TaskGroupList : ITaskGroupList
    {
        public TaskGroupAgent TaskGroupAgent { get; set; }
        public TaskGroupCache TaskGroupCache { get; set; }

        /// <summary>
        /// 获取全部任务列表（数据库）
        /// </summary>
        public Task<List<TaskGroupVO>> ToListInDbAsync() => TaskGroupAgent.ToListAsync().MapAsync<TaskGroupVO, TaskGroupPO>();

        /// <summary>
        /// 获取全部任务列表
        /// </summary>
        public Task<List<TaskGroupVO>> ToListInCacheAsync(EumCacheStoreType cacheStoreType = EumCacheStoreType.Redis) => TaskGroupCache.ToListAsync(cacheStoreType);

        /// <summary>
        /// 获取任务组数量
        /// </summary>
        public Task<long> Count()
        {
            var key = CacheKeys.TaskGroupKey(EumCacheStoreType.Redis);
            return RedisContext.Instance.CacheManager.GetCountAsync(key);
        }
    }
}