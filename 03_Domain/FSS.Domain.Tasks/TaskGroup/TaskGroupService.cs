using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FSS.Domain.Tasks.TaskGroup.Entity;
using FSS.Domain.Tasks.TaskGroup.Enum;
using FSS.Domain.Tasks.TaskGroup.Interface;
using FSS.Domain.Tasks.TaskGroup.Repository;

namespace FSS.Domain.Tasks.TaskGroup
{
    public class TaskGroupService : ITaskGroupService
    {
        public ITaskGroupRepository TaskGroupRepository { get; set; }

        /// <summary>
        /// 获取任务组信息
        /// </summary>
        public Task<TaskGroupDO> ToEntityAsync(int taskGroupId) => TaskGroupRepository.ToEntityAsync(taskGroupId);

        /// <summary>
        /// 删除任务组
        /// </summary>
        public async Task DeleteAsync(int taskGroupId)
        {
            var taskGroup = await TaskGroupRepository.ToEntityAsync(taskGroupId);
            await taskGroup.DeleteAsync();
        }

        /// <summary>
        /// 获取所有任务组中的任务
        /// </summary>
        public async Task<List<TaskGroupDO>> ToListAsync()
        {
            var lstTaskGroup = await TaskGroupRepository.ToListAsync();

            foreach (var taskGroupPO in lstTaskGroup)
            {
                if (taskGroupPO.Task == null) await taskGroupPO.CreateTask();
            }
            return lstTaskGroup;
        }

        /// <summary>
        /// 获取所有任务组中的任务
        /// </summary>
        public async Task<List<TaskGroupDO>> ToListAsync(string[] jobs, TimeSpan ts, int count)
        {
            var lstTaskGroup = await ToListAsync();
            return lstTaskGroup.Where(o => o.Task != null && o.Task.Status == EumTaskType.None && jobs.Contains(o.JobName) && o.StartAt < DateTime.Now.Add(ts)).OrderBy(o => o.StartAt).Take(count).ToList();
        }

    }
}