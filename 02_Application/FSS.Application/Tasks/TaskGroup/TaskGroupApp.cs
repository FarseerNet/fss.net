using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FS.Core;
using FS.Extends;
using FSS.Application.Clients.Dto;
using FSS.Application.Tasks.TaskGroup.Entity;
using FSS.Application.Tasks.TaskGroup.Interface;
using FSS.Domain.Tasks.TaskGroup.Entity;
using FSS.Domain.Tasks.TaskGroup.Enum;
using FSS.Domain.Tasks.TaskGroup.Interface;
using FSS.Domain.Tasks.TaskGroup.Repository;

namespace FSS.Application.Tasks.TaskGroup
{
    public class TaskGroupApp : ITaskGroupApp
    {
        public ITaskGroupService    TaskGroupService    { get; set; }
        public ITaskGroupRepository TaskGroupRepository { get; set; }

        /// <summary>
        /// 添加任务组信息
        /// </summary>
        public Task<int> AddAsync(TaskGroupDTO dto) => dto.Map<TaskGroupDO>().AddAsync();

        /// <summary>
        /// 获取任务组信息
        /// </summary>
        public Task<TaskGroupDO> ToEntityAsync(int taskGroupId) => TaskGroupRepository.ToEntityAsync(taskGroupId);

        /// <summary>
        /// 取消任务
        /// </summary>
        public async Task CancelTask(int taskGroupId)
        {
            var taskGroup = await TaskGroupRepository.ToEntityAsync(taskGroupId);
            await taskGroup.CancelTask();
        }

        /// <summary>
        /// 今日执行失败数量
        /// </summary>
        public Task<int> TodayFailCountAsync() => TaskGroupRepository.TodayFailCountAsync();

        /// <summary>
        /// 计算任务的平均运行速度
        /// </summary>
        public async Task UpdateAvgSpeed(int taskGroupId)
        {
            var taskGroup = await TaskGroupRepository.ToEntityAsync(taskGroupId);
            await taskGroup.UpdateAvgSpeed();
        }

        /// <summary>
        /// 获取所有任务组中的任务
        /// </summary>
        public Task<List<TaskGroupDO>> ToListAsync() => TaskGroupService.ToListAsync();

        /// <summary>
        /// 获取任务组数量
        /// </summary>
        public Task<long> GetTaskGroupCount() => TaskGroupRepository.GetTaskGroupCountAsync();

        /// <summary>
        /// 创建Task
        /// </summary>
        public async Task<TaskDO> GetOrCreateAsync(int taskGroupId)
        {
            var taskGroup = await TaskGroupRepository.ToEntityAsync(taskGroupId);
            await taskGroup.CreateTask();
            return taskGroup.Task;
        }

        /// <summary>
        /// 同步数据
        /// </summary>
        public Task SyncTaskGroup()
        {
            return TaskGroupRepository.SyncToData();
        }

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
        /// 任务调度
        /// </summary>
        public async Task<List<TaskDTO>> TaskSchedulerAsync(ClientDTO client, int requestTaskCount)
        {
            if (requestTaskCount == 0) requestTaskCount = 3;

            var lst = new List<TaskDTO>();

            var lstTaskGroup = await TaskGroupRepository.GetMyCanSchedulerTaskGroup(client.Jobs, TimeSpan.FromSeconds(15), requestTaskCount);
            foreach (var taskGroupDO in lstTaskGroup)
            {
                // 设为调度状态
                await taskGroupDO.SetClient(client.Id, client.ClientIp, client.ClientName);

                var taskDto = (TaskDTO)taskGroupDO.Task;
                taskDto.Data = Jsons.ToObject<Dictionary<string, string>>(taskGroupDO.Data);
                lst.Add(taskDto);
            }
            return lst;
        }

        /// <summary>
        /// 获取指定任务组的任务列表（FOPS）
        /// </summary>
        public async Task<DataSplitList<TaskDTO>> ToListAsync(int groupId, int pageSize, int pageIndex)
        {
            var lst = await TaskGroupRepository.ToListAsync(groupId, pageSize, pageIndex, out var totalCount);
            return new DataSplitList<TaskDTO>(lst.Map<TaskDTO>(), totalCount);
        }

        /// <summary>
        /// 获取指定任务组执行成功的任务列表
        /// </summary>
        public Task<List<TaskDTO>> ToTaskGroupFinishListAsync(int taskGroupId, int top) => TaskGroupRepository.ToFinishListAsync(taskGroupId, top).MapAsync<TaskDTO, TaskDO>();

        /// <summary>
        /// 获取已完成的任务列表
        /// </summary>
        public async Task<DataSplitList<TaskDTO>> ToFinishListAsync(int pageSize, int pageIndex)
        {
            var lst = await TaskGroupRepository.ToFinishListAsync(pageSize, pageIndex, out var totalCount);
            return new DataSplitList<TaskDTO>(lst.Map<TaskDTO>(), totalCount);
        }

        /// <summary>
        /// 获取执行中的任务
        /// </summary>
        public Task<List<TaskGroupDO>> ToSchedulerWorkingListAsync() => TaskGroupRepository.ToSchedulerWorkingListAsync();

        /// <summary>
        /// 复制任务组
        /// </summary>
        public async Task<int> CopyTaskGroup(int taskGroupId)
        {
            var taskGroup = await TaskGroupRepository.ToEntityAsync(taskGroupId);
            if (taskGroup == null) throw new Exception("要复制的任务组不存在");
            return await taskGroup.CopyAsync();
        }

        public async Task DeleteAsync(int taskGroupId)
        {
            var taskGroup = await TaskGroupRepository.ToEntityAsync(taskGroupId);
            if (taskGroup == null) throw new Exception("要删除的任务组不存在");
            await taskGroup.DeleteAsync();
        }

        /// <summary>
        /// 获取未执行的任务数量
        /// </summary>
        public Task<int> ToUnRunCountAsync() => TaskGroupRepository.ToUnRunCountAsync();

        /// <summary>
        /// 获取进行中的任务
        /// </summary>
        public async Task<List<TaskDTO>> GetTaskUnFinishList(int top)
        {
            var taskUnFinishList = await TaskGroupRepository.GetTaskUnFinishList(top);
            return taskUnFinishList.Select(o => o.Task.Map<TaskDTO>()).ToList();
        }

        /// <summary>
        /// 获取在用的任务组
        /// </summary>
        public DataSplitList<TaskDTO> GetEnableTaskList(EumTaskType? status, int pageSize, int pageIndex)
        {
            var lst = TaskGroupRepository.GetEnableTaskList(status, pageSize, pageIndex, out var totalCount);
            return new DataSplitList<TaskDTO>(lst.Select(o => o.Task).Map<TaskDTO>(), totalCount);
        }
    }
}