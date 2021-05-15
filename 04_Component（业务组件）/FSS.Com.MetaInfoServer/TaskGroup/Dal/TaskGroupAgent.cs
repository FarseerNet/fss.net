using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FSS.Com.MetaInfoServer.Abstract;

namespace FSS.Com.MetaInfoServer.TaskGroup.Dal
{
    /// <summary>
    /// 任务组数据库层
    /// </summary>
    public class TaskGroupAgent : ITaskGroupAgent
    {
        /// <summary>
        /// 获取所有任务组列表
        /// </summary>
        public Task<List<TaskGroupPO>> ToListAsync() => MetaInfoContext.Data.TaskGroup.ToListAsync();

        /// <summary>
        /// 获取任务组信息
        /// </summary>
        public Task<TaskGroupPO> ToEntityAsync(int id) => MetaInfoContext.Data.TaskGroup.Where(o => o.Id == id).ToEntityAsync();

        /// <summary>
        /// 更新任务组信息
        /// </summary>
        public Task UpdateAsync(int id, TaskGroupPO taskGroup) => MetaInfoContext.Data.TaskGroup.Where(o => o.Id == id).UpdateAsync(taskGroup);

        /// <summary>
        /// 添加任务组
        /// </summary>
        public Task AddAsync(TaskGroupPO po) => MetaInfoContext.Data.TaskGroup.InsertAsync(po, true);

        /// <summary>
        /// 更新任务ID
        /// </summary>
        public Task UpdateTaskIdAsync(int taskGroupId, int taskId) => MetaInfoContext.Data.TaskGroup.Where(o => o.Id == taskGroupId).UpdateAsync(new TaskGroupPO {TaskId = taskId});

        /// <summary>
        /// 更新任务时间
        /// </summary>
        public Task UpdateNextAtAsync(int taskGroupId, DateTime nextAt) => MetaInfoContext.Data.TaskGroup.Where(o => o.Id == taskGroupId).UpdateAsync(new TaskGroupPO {NextAt = nextAt});
    }
}