using System.Collections.Generic;
using System.Threading.Tasks;
using FS.Cache;
using FS.DI;
using FS.Extends;
using FSS.Domain.Tasks.TaskGroup;
using FSS.Domain.Tasks.TaskGroup.Entity;
using FSS.Infrastructure.Repository.Context;
using FSS.Infrastructure.Repository.TaskGroup.Model;

namespace FSS.Infrastructure.Repository.TaskGroup;

/// <summary>
///     任务组数据库层
/// </summary>
public class TaskGroupCache : ISingletonDependency
{
    public TaskGroupAgent TaskGroupAgent { get; set; }

    /// <summary> 任务组缓存 </summary>
    private CacheKey<TaskGroupDO, int> TaskGroupKey(EumCacheStoreType cacheStoreType) => new(key: "FSS_TaskGroup", getField: o => o.Id, cacheStoreType: cacheStoreType);

    /// <summary>
    ///     保存任务组信息
    /// </summary>
    public Task SaveAsync(TaskGroupDO taskGroup)
    {
        var key = TaskGroupKey(cacheStoreType: EumCacheStoreType.Redis);
        return RedisContext.Instance.CacheManager.SaveItemAsync(cacheKey: key, entity: taskGroup);
    }

    /// <summary>
    ///     当前任务组的列表
    /// </summary>
    public Task<List<TaskGroupDO>> ToListAsync()
    {
        var key = TaskGroupKey(cacheStoreType: EumCacheStoreType.Redis);
        return RedisContext.Instance.CacheManager.GetListAsync(cacheKey: key, get: () => TaskGroupAgent.ToListAsync().MapAsync<TaskGroupDO, TaskGroupPO>());
    }

    /// <summary>
    ///     当前任务组的列表
    /// </summary>
    public List<TaskGroupDO> ToList()
    {
        var key = TaskGroupKey(cacheStoreType: EumCacheStoreType.Redis);
        return RedisContext.Instance.CacheManager.GetList(cacheKey: key);
    }

    /// <summary>
    ///     获取任务组
    /// </summary>
    public Task<TaskGroupDO> ToEntityAsync(int taskGroupId)
    {
        var key = TaskGroupKey(cacheStoreType: EumCacheStoreType.Redis);
        return RedisContext.Instance.CacheManager.GetItemAsync(cacheKey: key, fieldKey: taskGroupId, get: () => TaskGroupAgent.ToEntityAsync(id: taskGroupId).MapAsync<TaskGroupDO, TaskGroupPO>());
    }

    /// <summary>
    ///     获取任务组数量
    /// </summary>
    public Task<long> CountAsync()
    {
        var key = TaskGroupKey(cacheStoreType: EumCacheStoreType.Redis);
        return RedisContext.Instance.CacheManager.GetCountAsync(cacheKey: key);
    }

    public Task TaskGroupClear(int taskGroupId) => RedisContext.Instance.CacheManager.RemoveItemAsync(cacheKey: TaskGroupKey(cacheStoreType: EumCacheStoreType.MemoryAndRedis), fieldKey: taskGroupId);
}