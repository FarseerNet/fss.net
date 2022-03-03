using System.Collections.Generic;
using System.Threading.Tasks;
using FS.Core;
using FS.Core.Net;
using FS.Extends;
using FSS.Application.Clients.Client;
using FSS.Application.Clients.Client.Entity;
using FSS.Application.Log.TaskLog;
using FSS.Application.Log.TaskLog.Entity;
using FSS.Application.Tasks.TaskGroup;
using FSS.Application.Tasks.TaskGroup.Entity;
using FSS.Domain.Tasks.TaskGroup.Entity;
using FSS.Service.Request;
using Microsoft.AspNetCore.Mvc;

namespace FSS.Service.Controllers;

/// <summary>
///     基础信息
/// </summary>
[ApiController]
[Route(template: "meta")]
public class MetaController : ControllerBase
{
    public ClientApp    ClientApp    { get; set; }
    public TaskLogApp   TaskLogApp   { get; set; }
    public TaskGroupApp TaskGroupApp { get; set; }
    public TaskQueryApp TaskQueryApp { get; set; }

    /// <summary>
    ///     客户端拉取任务
    /// </summary>
    [HttpPost]
    [Route(template: "GetClientList")]
    public async Task<ApiResponseJson<List<ClientDTO>>> GetClientList()
    {
        // 取出全局客户端列表
        var lst = await ClientApp.ToListAsync();
        return await ApiResponseJson<List<ClientDTO>>.SuccessAsync(data: lst);
    }

    /// <summary>
    ///     取出全局客户端数量
    /// </summary>
    [HttpPost]
    [Route(template: "GetClientCount")]
    public async Task<ApiResponseJson<long>> GetClientCount()
    {
        // 取出全局客户端列表
        var count = await ClientApp.GetCountAsync();
        return await ApiResponseJson<long>.SuccessAsync(data: count);
    }

    /// <summary>
    ///     复制任务组
    /// </summary>
    [HttpPost]
    [Route(template: "CopyTaskGroup")]
    public async Task<ApiResponseJson<int>> CopyTaskGroup(OnlyIdRequest request)
    {
        var taskGroupId = await TaskGroupApp.CopyTaskGroup(taskGroupId: request.Id);
        return await ApiResponseJson<int>.SuccessAsync(statusMessage: "复制成功", data: taskGroupId);
    }

    /// <summary>
    ///     删除任务组
    /// </summary>
    [HttpPost]
    [Route(template: "DeleteTaskGroup")]
    public async Task<ApiResponseJson> DeleteTaskGroup(OnlyIdRequest request)
    {
        await TaskGroupApp.DeleteAsync(taskGroupId: request.Id);
        return await ApiResponseJson.SuccessAsync(statusMessage: "删除成功", data: request.Id);
    }

    /// <summary>
    ///     获取任务组信息
    /// </summary>
    [HttpPost]
    [Route(template: "GetTaskGroupInfo")]
    public async Task<ApiResponseJson<TaskGroupDTO>> GetTaskGroupInfo(OnlyIdRequest request)
    {
        var info = await TaskQueryApp.ToEntityAsync(taskGroupId: request.Id).MapAsync<TaskGroupDTO, TaskGroupDO>();
        return await ApiResponseJson<TaskGroupDTO>.SuccessAsync(data: info);
    }

    /// <summary>
    ///     同步缓存到数据库
    /// </summary>
    [HttpPost]
    [Route(template: "SyncCacheToDb")]
    public async Task<ApiResponseJson<List<TaskGroupDO>>> SyncCacheToDb()
    {
        await TaskGroupApp.SyncTaskGroup();
        return await ApiResponseJson.SuccessAsync();
    }

    /// <summary>
    ///     获取全部任务组列表
    /// </summary>
    [HttpPost]
    [Route(template: "GetTaskGroupList")]
    public async Task<ApiResponseJson<List<TaskGroupDTO>>> GetTaskGroupList()
    {
        var lst = await TaskQueryApp.ToListAsync().MapAsync<TaskGroupDTO, TaskGroupDO>();
        return await ApiResponseJson<List<TaskGroupDTO>>.SuccessAsync(data: lst);
    }

    /// <summary>
    ///     获取任务组数量
    /// </summary>
    [HttpPost]
    [Route(template: "GetTaskGroupCount")]
    public async Task<ApiResponseJson<long>> GetTaskGroupCount()
    {
        var count = await TaskQueryApp.GetTaskGroupCount();
        return await ApiResponseJson<long>.SuccessAsync(data: count);
    }

    /// <summary>
    ///     获取未执行的任务数量
    /// </summary>
    [HttpPost]
    [Route(template: "GetTaskGroupUnRunCount")]
    public async Task<ApiResponseJson<long>> GetTaskGroupUnRunCount()
    {
        var count = await TaskQueryApp.ToUnRunCountAsync();
        return await ApiResponseJson<long>.SuccessAsync(data: count);
    }

    /// <summary>
    ///     添加任务组
    /// </summary>
    [HttpPost]
    [Route(template: "AddTaskGroup")]
    public async Task<ApiResponseJson<int>> AddTaskGroup(TaskGroupDTO request)
    {
        if (request.Caption == null || request.Cron == null || request.Data == null || request.JobName == null) return await ApiResponseJson<int>.ErrorAsync(statusMessage: "标题、时间间隔、传输数据、Job名称 必须填写");
        request.Id = await TaskGroupApp.AddAsync(dto: request);
        return await ApiResponseJson<int>.SuccessAsync(statusMessage: "添加成功", data: request.Id);
    }

    /// <summary>
    ///     修改任务组或设置Enable
    /// </summary>
    [HttpPost]
    [Route(template: "SaveTaskGroup")]
    public async Task<ApiResponseJson> SaveTaskGroup(TaskGroupDTO request)
    {
        await TaskGroupApp.Save(dto: request);
        return await ApiResponseJson.SuccessAsync();
    }

    /// <summary>
    ///     今日执行失败数量
    /// </summary>
    [HttpPost]
    [Route(template: "TodayTaskFailCount")]
    public async Task<ApiResponseJson<int>> TodayTaskFailCount()
    {
        var count = await TaskQueryApp.TodayFailCountAsync();
        return await ApiResponseJson<int>.SuccessAsync(data: count);
    }

    /// <summary>
    ///     获取进行中的任务
    /// </summary>
    [HttpPost]
    [Route(template: "GetTaskUnFinishList")]
    public async Task<ApiResponseJson<List<TaskDTO>>> GetTaskUnFinishList(OnlyTopRequest request)
    {
        var lst = await TaskQueryApp.GetTaskUnFinishList(top: request.Top);
        return await ApiResponseJson<List<TaskDTO>>.SuccessAsync(data: lst);
    }

    /// <summary>
    ///     获取在用的任务
    /// </summary>
    [HttpPost]
    [Route(template: "GetEnableTaskList")]
    public async Task<ApiResponseJson<PageList<TaskDTO>>> GetEnableTaskList(GetAllTaskListRequest request)
    {
        var lst = TaskQueryApp.GetEnableTaskList(status: request.Status, pageSize: request.PageSize, pageIndex: request.PageIndex);
        return await ApiResponseJson<PageList<TaskDTO>>.SuccessAsync(data: lst);
    }

    /// <summary>
    ///     获取指定任务组的任务列表
    /// </summary>
    [HttpPost]
    [Route(template: "GetTaskList")]
    public async Task<ApiResponseJson<PageList<TaskDTO>>> GetTaskList(GetTaskListRequest request)
    {
        var lst = await TaskQueryApp.ToListAsync(groupId: request.GroupId, pageSize: request.PageSize, pageIndex: request.PageIndex);
        return await ApiResponseJson<PageList<TaskDTO>>.SuccessAsync(data: lst);
    }

    /// <summary>
    ///     获取已完成的任务列表
    /// </summary>
    [HttpPost]
    [Route(template: "GetTaskFinishList")]
    public async Task<ApiResponseJson<PageList<TaskDTO>>> GetTaskFinishList(PageSizeRequest request)
    {
        var lst = await TaskQueryApp.ToFinishPageListAsync(pageSize: request.PageSize, pageIndex: request.PageIndex);
        return await ApiResponseJson<PageList<TaskDTO>>.SuccessAsync(data: lst);
    }

    /// <summary>
    ///     取消任务
    /// </summary>
    [HttpPost]
    [Route(template: "CancelTask")]
    public async Task<ApiResponseJson> CancelTask(OnlyIdRequest request)
    {
        await TaskGroupApp.CancelTask(taskGroupId: request.Id);
        return await ApiResponseJson.SuccessAsync();
    }

    /// <summary>
    ///     获取日志
    /// </summary>
    [HttpPost]
    [Route(template: "GetRunLogList")]
    public async Task<ApiResponseJson<PageList<TaskLogDTO>>> GetRunLogList(GetRunLogRequest request)
    {
        var lst = TaskLogApp.GetList(jobName: request.JobName, logLevel: request.LogLevel, pageSize: request.PageSize, pageIndex: request.PageIndex);
        return await ApiResponseJson<PageList<TaskLogDTO>>.SuccessAsync(data: lst);
    }
}