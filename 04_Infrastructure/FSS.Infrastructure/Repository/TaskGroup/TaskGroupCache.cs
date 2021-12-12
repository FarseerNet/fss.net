using System.Collections.Generic;
using System.Threading.Tasks;
using FS.Cache;
using FS.DI;
using FS.Extends;
using FSS.Infrastructure.Repository.TaskGroup.Model;

namespace FSS.Infrastructure.Repository.TaskGroup
{
    /// <summary>
    /// 任务组数据库层
    /// </summary>
    public class TaskGroupCache : ISingletonDependency
    {
        public TaskGroupAgent TaskGroupAgent { get; set; }
        
        /// <summary>
        /// 保存任务组信息
        /// </summary>
        public Task SaveAsync(TaskGroupPO taskGroup)
        {
            var key = CacheKeys.TaskGroupKey(EumCacheStoreType.MemoryAndRedis);
            return RedisContext.Instance.CacheManager.SaveItemAsync(key, taskGroup);
        }

        /// <summary>
        /// 保存任务组信息
        /// </summary>
        public Task SaveAsync(List<TaskGroupPO> lstTaskGroup)
        {
            var key = CacheKeys.TaskGroupKey(EumCacheStoreType.MemoryAndRedis);
            return RedisContext.Instance.CacheManager.SaveListAsync(key, lstTaskGroup);
        }

        /// <summary>
        /// 当前任务组的列表
        /// </summary>
        public Task<List<TaskGroupPO>> ToListAsync(EumCacheStoreType cacheStoreType)
        {
            var key = CacheKeys.TaskGroupKey(cacheStoreType);
            return RedisContext.Instance.CacheManager.GetListAsync(key, () => TaskGroupAgent.ToListAsync());
        }

        /// <summary>
        /// 当前任务组的列表
        /// </summary>
        public List<TaskGroupPO> ToList(EumCacheStoreType cacheStoreType)
        {
            var key = CacheKeys.TaskGroupKey(cacheStoreType);
            return RedisContext.Instance.CacheManager.GetList(key);
        }

        /// <summary>
        /// 获取任务组
        /// </summary>
        public Task<TaskGroupPO> ToEntityAsync(EumCacheStoreType cacheStoreType, int taskGroupId)
        {
            var key = CacheKeys.TaskGroupKey(cacheStoreType);
            return RedisContext.Instance.CacheManager.GetItemAsync(key, taskGroupId, () => TaskGroupAgent.ToEntityAsync(taskGroupId).MapAsync<TaskGroupPO, TaskGroupPO>());
        }
        
        /// <summary>
        /// 获取任务组数量
        /// </summary>
        public Task<long> CountAsync()
        {
            var key = CacheKeys.TaskGroupKey(EumCacheStoreType.Redis);
            return RedisContext.Instance.CacheManager.GetCountAsync(key);
        }
    }
}