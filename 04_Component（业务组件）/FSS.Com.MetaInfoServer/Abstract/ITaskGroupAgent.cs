using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FS.DI;
using FSS.Com.MetaInfoServer.TaskGroup.Dal;

namespace FSS.Com.MetaInfoServer.Abstract
{
    public interface ITaskGroupAgent : ITransientDependency
    {
        /// <summary>
        /// 获取所有任务列表
        /// </summary>
        Task<List<TaskGroupPO>> ToListAsync();
        
        /// <summary>
        /// 获取任务信息
        /// </summary>
        Task<TaskGroupPO> ToEntityAsync(int id);

        /// <summary>
        /// 更新任务组信息
        /// </summary>
        Task UpdateAsync(int id, TaskGroupPO taskGroup);

        /// <summary>
        /// 更新任务ID
        /// </summary>
        Task UpdateTaskIdAsync(int taskGroupId, int taskId);

        /// <summary>
        /// 更新任务时间
        /// </summary>
        Task UpdateNextAtAsync(int taskGroupId, DateTime nextAt);
    }
}