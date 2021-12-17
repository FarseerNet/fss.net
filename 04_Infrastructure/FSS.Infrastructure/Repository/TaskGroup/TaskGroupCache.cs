using System.Collections.Generic;
using System.Threading.Tasks;
using FS.Cache;
using FS.DI;
using FS.Extends;
using FSS.Domain.Tasks.TaskGroup.Entity;
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
        public Task SaveAsync(TaskGroupDO taskGroup)
        {
            var key = CacheKeys.TaskGroupKey(EumCacheStoreType.Redis);
            return RedisContext.Instance.CacheManager.SaveItemAsync(key, taskGroup);
        }

        /// <summary>
        /// 当前任务组的列表
        /// </summary>
        public Task<List<TaskGroupDO>> ToListAsync()
        {
            var key = CacheKeys.TaskGroupKey(EumCacheStoreType.Redis);
            return RedisContext.Instance.CacheManager.GetListAsync(key, () => TaskGroupAgent.ToListAsync().MapAsync<TaskGroupDO, TaskGroupPO>());
        }

        /// <summary>
        /// 当前任务组的列表
        /// </summary>
        public List<TaskGroupDO> ToList()
        {
            var key = CacheKeys.TaskGroupKey(EumCacheStoreType.Redis);
            return RedisContext.Instance.CacheManager.GetList(key);
        }

        /// <summary>
        /// 获取任务组
        /// </summary>
        public Task<TaskGroupDO> ToEntityAsync(int taskGroupId)
        {
            var key = CacheKeys.TaskGroupKey(EumCacheStoreType.Redis);
            return RedisContext.Instance.CacheManager.GetItemAsync(key, taskGroupId, () => TaskGroupAgent.ToEntityAsync(taskGroupId).MapAsync<TaskGroupDO, TaskGroupPO>());
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