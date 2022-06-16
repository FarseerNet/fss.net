using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Collections.Pooled;
using FS.Core;
using FS.Core.Abstract.AspNetCore;
using FS.Core.Abstract.Data;
using FS.Core.Net;
using FS.DI;
using FS.Extends;
using FSS.Application.Tasks.TaskGroup.Entity;
using FSS.Domain.Client.Clients.Repository;
using FSS.Domain.Tasks.TaskGroup;
using FSS.Domain.Tasks.TaskGroup.Entity;
using FSS.Domain.Tasks.TaskGroup.Enum;
using FSS.Domain.Tasks.TaskGroup.Repository;
using FSS.Service.Request;
using Mapster;

namespace FSS.Application.Tasks.TaskGroup;

[UseApi(Area = "meta")]
public class TaskQueryApp : ISingletonDependency
{
    public ITaskGroupRepository TaskGroupRepository { get; set; }
    public IClientRepository    ClientRepository    { get; set; }

    /// <summary>
    ///     获取任务组信息
    /// </summary>
    [Api("GetTaskGroupInfo")]
    public Task<TaskGroupDTO> ToEntityAsync(OnlyIdRequest request) => TaskGroupRepository.ToEntityAsync(taskGroupId: request.Id).MapAsync<TaskGroupDTO, TaskGroupDO>();

    /// <summary>
    ///     获取所有任务组中的任务
    /// </summary>
    [Api("GetTaskGroupList")]
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
    [Api("TodayTaskFailCount")]
    public Task<int> TodayFailCountAsync() => TaskGroupRepository.TodayFailCountAsync();

    /// <summary>
    ///     获取任务组数量
    /// </summary>
    [Api("GetTaskGroupCount")]
    public Task<int> GetTaskGroupCount() => TaskGroupRepository.GetTaskGroupCountAsync();

    /// <summary>
    ///     获取未执行的任务数量
    /// </summary>
    [Api("GetTaskGroupUnRunCount")]
    public Task<int> ToUnRunCountAsync() => TaskGroupRepository.ToUnRunCountAsync();

    /// <summary>
    ///     获取指定任务组的任务列表（FOPS）
    /// </summary>
    [Api("GetTaskList")]
    public Task<PageList<TaskDTO>> ToListAsync(GetTaskListRequest request) => TaskGroupRepository.ToListAsync(groupId: request.GroupId, pageSize: request.PageSize, pageIndex: request.PageIndex).MapAsync<TaskDTO, TaskDO>();

    /// <summary>
    ///     获取已完成的任务列表
    /// </summary>
    [Api("GetTaskFinishList")]
    public Task<PageList<TaskDTO>> ToFinishPageListAsync(PageSizeRequest request) => TaskGroupRepository.ToFinishPageListAsync(pageSize: request.PageSize, pageIndex: request.PageIndex).MapAsync<TaskDTO, TaskDO>();

    /// <summary>
    ///     获取进行中的任务
    /// </summary>
    [Api("GetTaskUnFinishList")]
    public async Task<List<TaskDTO>> GetTaskUnFinishList(OnlyTopRequest request)
    {
        var lstClient = ClientRepository.ToList();
        if (lstClient.Count == 0) return new List<TaskDTO>();

        var taskUnFinishList = await TaskGroupRepository.GetTaskUnFinishList(lstClient.SelectMany(o => o.Jobs), top: request.Top);
        return taskUnFinishList.Select(selector: o => (TaskDTO)o.Task).ToList();
    }

    /// <summary>
    ///     获取在用的任务组
    /// </summary>
    [Api("GetEnableTaskList")]
    public PageList<TaskDTO> GetEnableTaskList(GetAllTaskListRequest request)
    {
        var lst = TaskGroupRepository.GetEnableTaskList(status: request.Status, pageSize: request.PageSize, pageIndex: request.PageIndex);
        return new PageList<TaskDTO>(lst.List.Select(o => (TaskDTO)o).ToPooledList(), lst.RecordCount);
    }
}