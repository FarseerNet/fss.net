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
        /// 获取全部任务列表
        /// </summary>
        public async Task<List<TaskGroupVO>> ToListAndSaveAsync()
        {
            var taskGroupVos = await TaskGroupAgent.ToListAsync().MapAsync<TaskGroupVO, TaskGroupPO>();
            await TaskGroupCache.SaveAsync(taskGroupVos);
            return taskGroupVos;
        }

        /// <summary>
        /// 获取全部任务列表（数据库）
        /// </summary>
        public Task<List<TaskGroupVO>> ToListInDbAsync() => TaskGroupAgent.ToListAsync().MapAsync<TaskGroupVO, TaskGroupPO>();

        /// <summary>
        /// 获取全部任务列表
        /// </summary>
        public Task<List<TaskGroupVO>> ToListInCacheAsync(EumCacheStoreType cacheStoreType = EumCacheStoreType.Redis) => TaskGroupCache.ToListAsync(cacheStoreType);

        /// <summary>
        /// 本地缓存获取任务组
        /// </summary>
        public async Task<Dictionary<int, TaskGroupVO>> ToListInMemoryAsync()
        {
            var lst = await ToListInCacheAsync(EumCacheStoreType.MemoryAndRedis);
            return lst.ToDictionary(o => o.Id, o => o);
        }

        /// <summary>
        /// 获取任务组数量
        /// </summary>
        public Task<long> Count()
        {
            var key = CacheKeys.TaskGroupKey(EumCacheStoreType.Redis);
            return RedisContext.Instance.CacheManager.GetCountAsync(key);
        }

        /// <summary>
        /// 获取未执行的任务列表(FOPS)
        /// </summary>
        public async Task<int> ToUnRunCountAsync()
        {
            try
            {
                var now = DateTime.Now.AddMilliseconds(-500);
                var lst = await ToListAndSaveAsync();
                return lst.Count(o => o.NextAt < now && o.IsEnable);
            }
            catch (Exception e)
            {
                IocManager.Instance.Logger<TaskGroupList>().LogError(e, e.Message);
                return 0;
            }
        }
    }
}