using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FS.Core;
using FS.DI;
using FS.Extends;
using FSS.Application.Tasks.TaskGroup.Entity;
using FSS.Domain.Tasks.TaskGroup;
using FSS.Domain.Tasks.TaskGroup.Entity;
using FSS.Domain.Tasks.TaskGroup.Enum;
using FSS.Domain.Tasks.TaskGroup.Repository;

namespace FSS.Application.Tasks.TaskGroup
{
    public class TaskQueryApp : ISingletonDependency
    {
        public TaskGroupService    TaskGroupService    { get; set; }
        public ITaskGroupRepository TaskGroupRepository { get; set; }

        /// <summary>
        /// 获取任务组信息
        /// </summary>
        public Task<TaskGroupDO> ToEntityAsync(int taskGroupId) => TaskGroupRepository.ToEntityAsync(taskGroupId);

        /// <summary>
        /// 获取所有任务组中的任务
        /// </summary>
        public Task<List<TaskGroupDO>> ToListAsync() => TaskGroupService.ToListAsync();

        /// <summary>
        /// 今日执行失败数量
        /// </summary>
        public Task<int> TodayFailCountAsync() => TaskGroupRepository.TodayFailCountAsync();

        /// <summary>
        /// 获取任务组数量
        /// </summary>
        public Task<long> GetTaskGroupCount() => TaskGroupRepository.GetTaskGroupCountAsync();

        /// <summary>
        /// 获取未执行的任务数量
        /// </summary>
        public Task<int> ToUnRunCountAsync() => TaskGroupRepository.ToUnRunCountAsync();

        /// <summary>
        /// 获取指定任务组的任务列表（FOPS）
        /// </summary>
        public Task<PageList<TaskDTO>> ToListAsync(int groupId, int pageSize, int pageIndex)
        {
            return TaskGroupRepository.ToListAsync(groupId, pageSize, pageIndex).MapAsync(TaskDTO.MapToDTO);
        }

        /// <summary>
        /// 获取已完成的任务列表
        /// </summary>
        public Task<PageList<TaskDTO>> ToFinishPageListAsync(int pageSize, int pageIndex)
        {
            return TaskGroupRepository.ToFinishPageListAsync(pageSize, pageIndex).MapAsync(TaskDTO.MapToDTO);
        }

        /// <summary>
        /// 获取进行中的任务
        /// </summary>
        public async Task<List<TaskDTO>> GetTaskUnFinishList(int top)
        {
            var taskUnFinishList = await TaskGroupRepository.GetTaskUnFinishList(top);
            return taskUnFinishList.Select(o => (TaskDTO)o.Task).ToList();
        }

        /// <summary>
        /// 获取在用的任务组
        /// </summary>
        public PageList<TaskDTO> GetEnableTaskList(EumTaskType? status, int pageSize, int pageIndex)
        {
            var lst = TaskGroupRepository.GetEnableTaskList(status, pageSize, pageIndex, out var totalCount);
            return new PageList<TaskDTO>(lst.Select(o => (TaskDTO)o.Task), totalCount);
        }
    }
}