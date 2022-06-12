using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Collections.Pooled;
using FS.Core;
using FS.Core.Abstract.Data;
using FS.DI;
using FS.Extends;
using FSS.Application.Tasks.TaskGroup.Entity;
using FSS.Domain.Client.Clients.Repository;
using FSS.Domain.Tasks.TaskGroup;
using FSS.Domain.Tasks.TaskGroup.Entity;
using FSS.Domain.Tasks.TaskGroup.Enum;
using FSS.Domain.Tasks.TaskGroup.Repository;
using Mapster;

namespace FSS.Application.Tasks.TaskGroup;

public class TaskQueryApp : ISingletonDependency
{
    public ITaskGroupRepository TaskGroupRepository { get; set; }
    public IClientRepository    ClientRepository    { get; set; }

    /// <summary>
    ///     获取任务组信息
    /// </summary>
    public Task<TaskGroupDO> ToEntityAsync(int taskGroupId) => TaskGroupRepository.ToEntityAsync(taskGroupId: taskGroupId);

    /// <summary>
    ///     获取所有任务组中的任务
    /// </summary>
    public async Task<PooledList<TaskGroupDO>> ToListAsync()
    {
        var lstTaskGroup = await TaskGroupRepository.ToListAsync();

        foreach (var taskGroup in lstTaskGroup)
        {
            if (taskGroup.Task == null)
            {
                taskGroup.CreateTask();
                TaskGroupRepository.Save(taskGroup);
            }
        }
        return lstTaskGroup;
    }

    /// <summary>
    ///     今日执行失败数量
    /// </summary>
    public Task<int> TodayFailCountAsync() => TaskGroupRepository.TodayFailCountAsync();

    /// <summary>
    ///     获取任务组数量
    /// </summary>
    public Task<int> GetTaskGroupCount() => TaskGroupRepository.GetTaskGroupCountAsync();

    /// <summary>
    ///     获取未执行的任务数量
    /// </summary>
    public Task<int> ToUnRunCountAsync() => TaskGroupRepository.ToUnRunCountAsync();

    /// <summary>
    ///     获取指定任务组的任务列表（FOPS）
    /// </summary>
    public Task<PageList<TaskDTO>> ToListAsync(int groupId, int pageSize, int pageIndex) => TaskGroupRepository.ToListAsync(groupId: groupId, pageSize: pageSize, pageIndex: pageIndex).MapAsync(mapRule: TaskDTO.MapToDTO);

    /// <summary>
    ///     获取已完成的任务列表
    /// </summary>
    public Task<PageList<TaskDTO>> ToFinishPageListAsync(int pageSize, int pageIndex) => TaskGroupRepository.ToFinishPageListAsync(pageSize: pageSize, pageIndex: pageIndex).MapAsync(mapRule: TaskDTO.MapToDTO);

    /// <summary>
    ///     获取进行中的任务
    /// </summary>
    public async Task<List<TaskDTO>> GetTaskUnFinishList(int top)
    {
        var lstClient = ClientRepository.ToList();
        if (lstClient.Count == 0) return new List<TaskDTO>();

        var taskUnFinishList = await TaskGroupRepository.GetTaskUnFinishList(lstClient.SelectMany(o => o.Jobs), top: top);
        return taskUnFinishList.Select(selector: o => (TaskDTO)o.Task).ToList();
    }

    /// <summary>
    ///     获取在用的任务组
    /// </summary>
    public PageList<TaskDTO> GetEnableTaskList(EumTaskType? status, int pageSize, int pageIndex)
    {
        var lst = TaskGroupRepository.GetEnableTaskList(status: status, pageSize: pageSize, pageIndex: pageIndex);
        return new PageList<TaskDTO>(lst.List.Select(o => (TaskDTO)o).ToPooledList(), lst.RecordCount);
    }
}