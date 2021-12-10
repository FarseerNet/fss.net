using System.Collections.Generic;
using System.Threading.Tasks;
using FS.Cache;
using FS.DI;
using FS.Extends;
using FSS.Abstract.Entity.MetaInfo;
using FSS.Domain.Tasks.TaskGroup.Entity;
using FSS.Infrastructure.Repository;

namespace FSS.Com.MetaInfoServer.TaskGroup.Dal
{
    /// <summary>
    /// 任务组缓存
    /// </summary>
    // ReSharper disable once UnusedType.Global
    public class TaskGroupCache : ISingletonDependency
    {
        public TaskGroupAgent TaskGroupAgent { get; set; }

        /// <summary>
        /// 保存任务组信息
        /// </summary>
        public Task SaveAsync(TaskGroupDO taskGroup)
        {
            var key = CacheKeys.TaskGroupKey(EumCacheStoreType.MemoryAndRedis);
            return RedisContext.Instance.CacheManager.SaveItemAsync(key, taskGroup);
        }

        /// <summary>
        /// 保存任务组信息
        /// </summary>
        public Task SaveAsync(List<TaskGroupDO> lstTaskGroup)
        {
            var key = CacheKeys.TaskGroupKey(EumCacheStoreType.MemoryAndRedis);
            return RedisContext.Instance.CacheManager.SaveListAsync(key, lstTaskGroup);
        }

        /// <summary>
        /// 当前任务组的列表
        /// </summary>
        public Task<List<TaskGroupDO>> ToListAsync(EumCacheStoreType cacheStoreType)
        {
            var key = CacheKeys.TaskGroupKey(cacheStoreType);
            return RedisContext.Instance.CacheManager.GetListAsync(key, () => TaskGroupAgent.ToListAsync().MapAsync<TaskGroupDO, TaskGroupPO>());
        }

        /// <summary>
        /// 获取任务组
        /// </summary>
        public Task<TaskGroupDO> ToEntityAsync(EumCacheStoreType cacheStoreType, int taskGroupId)
        {
            var key = CacheKeys.TaskGroupKey(cacheStoreType);
            return RedisContext.Instance.CacheManager.GetItemAsync(key, taskGroupId, () => TaskGroupAgent.ToListAsync().MapAsync<TaskGroupDO, TaskGroupPO>());
        }
    }
}