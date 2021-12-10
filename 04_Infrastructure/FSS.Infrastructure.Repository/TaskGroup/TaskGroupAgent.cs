using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FS.Cache;
using FS.Extends;
using FSS.Infrastructure.Repository.TaskGroup.Interface;
using FSS.Infrastructure.Repository.TaskGroup.Model;

namespace FSS.Infrastructure.Repository.TaskGroup
{
    /// <summary>
    /// 任务组数据库层
    /// </summary>
    public class TaskGroupAgent : ITaskGroupAgent
    {
        /// <summary>
        /// 获取所有任务组列表
        /// </summary>
        public Task<List<TaskGroupPO>> ToListAsync() => MysqlContext.Data.TaskGroup.ToListAsync();

        /// <summary>
        /// 获取任务组信息
        /// </summary>
        public Task<TaskGroupPO> ToEntityAsync(int id) => MysqlContext.Data.TaskGroup.Where(o => o.Id == id).ToEntityAsync();

        /// <summary>
        /// 更新任务组信息
        /// </summary>
        public Task UpdateAsync(int id, TaskGroupPO taskGroup) => MysqlContext.Data.TaskGroup.Where(o => o.Id == id).UpdateAsync(taskGroup);

        /// <summary>
        /// 添加任务组
        /// </summary>
        public async Task<int> AddAsync(TaskGroupPO po)
        {
            await MysqlContext.Data.TaskGroup.InsertAsync(po, true);

            var entity = await MysqlContext.Data.TaskGroup.Where(o => o.Id == po.Id).ToEntityAsync();
            await SaveAsync(entity);
            return po.Id.GetValueOrDefault();
        }

        /// <summary>
        /// 更新任务时间
        /// </summary>
        public Task UpdateNextAtAsync(int taskGroupId, DateTime nextAt) => MysqlContext.Data.TaskGroup.Where(o => o.Id == taskGroupId).UpdateAsync(new TaskGroupPO { NextAt = nextAt });

        /// <summary>
        /// 删除当前任务组下的所有
        /// </summary>
        public async Task DeleteAsync(int taskGroupId)
        {
            await MysqlContext.Data.TaskGroup.Where(o => o.Id == taskGroupId).DeleteAsync();
            await CacheKeys.TaskForGroupClear(taskGroupId);
            await CacheKeys.TaskGroupClear(taskGroupId);
        }


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
            return RedisContext.Instance.CacheManager.GetListAsync(key, () => ToListAsync().MapAsync<TaskGroupPO, TaskGroupPO>());
        }

        /// <summary>
        /// 获取任务组
        /// </summary>
        public Task<TaskGroupPO> ToEntityAsync(EumCacheStoreType cacheStoreType, int taskGroupId)
        {
            var key = CacheKeys.TaskGroupKey(cacheStoreType);
            return RedisContext.Instance.CacheManager.GetItemAsync(key, taskGroupId, () => ToListAsync().MapAsync<TaskGroupPO, TaskGroupPO>());
        }
    }
}