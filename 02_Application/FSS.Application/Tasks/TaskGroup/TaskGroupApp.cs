using System;
using System.Threading.Tasks;
using FS.DI;
using FS.Extends;
using FSS.Application.Tasks.TaskGroup.Entity;
using FSS.Domain.Tasks.TaskGroup.Entity;
using FSS.Domain.Tasks.TaskGroup.Repository;

namespace FSS.Application.Tasks.TaskGroup
{
    public class TaskGroupApp : ISingletonDependency
    {
        public ITaskGroupRepository   TaskGroupRepository    { get; set; }

        /// <summary>
        /// 添加任务组信息
        /// </summary>
        public Task<int> AddAsync(TaskGroupDTO dto) => dto.Map<TaskGroupDO>().AddAsync();

        /// <summary>
        /// 保存任务组
        /// </summary>
        public async Task Save(TaskGroupDTO dto)
        {
            var oldTaskGroup = await TaskGroupRepository.ToEntityAsync(dto.Id);
            if (oldTaskGroup == null) throw new Exception("任务组不存在");

            await oldTaskGroup.SaveAsync(dto);
        }

        /// <summary>
        /// 设置任务组状态
        /// </summary>
        public async Task SetEnable(int taskGroupId, bool enable)
        {
            var taskGroupDO = await TaskGroupRepository.ToEntityAsync(taskGroupId);
            if (taskGroupDO == null) throw new Exception("任务组不存在");
            await taskGroupDO.SetEnable(enable);
        }

        /// <summary>
        /// 取消任务
        /// </summary>
        public async Task CancelTask(int taskGroupId)
        {
            var taskGroup = await TaskGroupRepository.ToEntityAsync(taskGroupId);
            await taskGroup.CancelAsync();
        }

        /// <summary>
        /// 计算任务的平均运行速度
        /// </summary>
        public async Task UpdateAvgSpeed(int taskGroupId)
        {
            var taskGroup = await TaskGroupRepository.ToEntityAsync(taskGroupId);
            await taskGroup.UpdateAvgSpeed();
        }

        /// <summary>
        /// 同步数据
        /// </summary>
        public Task SyncTaskGroup() => TaskGroupRepository.SyncToData();
        /// <summary>
        /// 复制任务组
        /// </summary>
        public async Task<int> CopyTaskGroup(int taskGroupId)
        {
            var taskGroup = await TaskGroupRepository.ToEntityAsync(taskGroupId);
            if (taskGroup == null) throw new Exception("要复制的任务组不存在");
            return await taskGroup.CopyAsync();
        }

        /// <summary>
        /// 删除任务组
        /// </summary>
        public async Task DeleteAsync(int taskGroupId)
        {
            var taskGroup = await TaskGroupRepository.ToEntityAsync(taskGroupId);
            if (taskGroup == null) throw new Exception("要删除的任务组不存在");
            await taskGroup.DeleteAsync();
        }
    }
}