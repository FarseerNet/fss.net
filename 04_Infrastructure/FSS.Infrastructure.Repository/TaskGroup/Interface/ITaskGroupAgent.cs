using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FS.Cache;
using FS.DI;
using FSS.Infrastructure.Repository.TaskGroup.Model;

namespace FSS.Infrastructure.Repository.TaskGroup.Interface
{
    public interface ITaskGroupAgent: ISingletonDependency
    {
        /// <summary>
        /// 获取所有任务组列表
        /// </summary>
        Task<List<TaskGroupPO>> ToListAsync();
        /// <summary>
        /// 当前任务组的列表
        /// </summary>
        Task<List<TaskGroupPO>> ToListAsync(EumCacheStoreType cacheStoreType);
        /// <summary>
        /// 获取任务组信息
        /// </summary>
        Task<TaskGroupPO> ToEntityAsync(int id);
        /// <summary>
        /// 获取任务组
        /// </summary>
        Task<TaskGroupPO> ToEntityAsync(EumCacheStoreType cacheStoreType, int taskGroupId);
        /// <summary>
        /// 更新任务组信息
        /// </summary>
        Task UpdateAsync(int id, TaskGroupPO taskGroup);
        /// <summary>
        /// 添加任务组
        /// </summary>
        Task<int> AddAsync(TaskGroupPO po);
        /// <summary>
        /// 更新任务时间
        /// </summary>
        Task UpdateNextAtAsync(int taskGroupId, DateTime nextAt);
        /// <summary>
        /// 删除当前任务组下的所有
        /// </summary>
        Task DeleteAsync(int taskGroupId);
        /// <summary>
        /// 保存任务组信息
        /// </summary>
        Task SaveAsync(TaskGroupPO taskGroup);
        /// <summary>
        /// 保存任务组信息
        /// </summary>
        Task SaveAsync(List<TaskGroupPO> lstTaskGroup);
    }
}