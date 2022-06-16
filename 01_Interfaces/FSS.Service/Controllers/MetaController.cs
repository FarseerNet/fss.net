using System.Collections.Generic;
using System.Threading.Tasks;
using Collections.Pooled;
using FS.Core.Abstract.Data;
using FS.Core.Extend;
using FS.Core.Net;
using FS.Extends;
using FSS.Application.Clients.Client;
using FSS.Application.Clients.Client.Entity;
using FSS.Application.Log.TaskLog;
using FSS.Application.Log.TaskLog.Entity;
using FSS.Application.Tasks.TaskGroup;
using FSS.Application.Tasks.TaskGroup.Entity;
using FSS.Domain.Tasks.TaskGroup;
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

    // /// <summary>
    // ///     客户端拉取任务
    // /// </summary>
    // [HttpPost]
    // [Route(template: "GetClientList")]
    // public ApiResponseJson<PooledList<ClientDTO>> GetClientList()
    // {
    //     // 取出全局客户端列表
    //     return ClientApp.ToList().ToSuccess();
    // }
    //
    // /// <summary>
    // ///     取出全局客户端数量
    // /// </summary>
    // [HttpPost]
    // [Route(template: "GetClientCount")]
    // public Task<ApiResponseJson<long>> GetClientCount()
    // {
    //     // 取出全局客户端列表
    //     return ApiResponseJson<long>.SuccessAsync(data: ClientApp.GetCount());
    // }
    //
    // /// <summary>
    // ///     复制任务组
    // /// </summary>
    // [HttpPost]
    // [Route(template: "CopyTaskGroup")]
    // public Task<ApiResponseJson<int>> CopyTaskGroup(OnlyIdRequest request)
    // {
    //     return TaskGroupApp.CopyTaskGroupAsync(taskGroupId: request.Id).ToSuccessAsync("复制成功");
    // }
    //
    // /// <summary>
    // ///     删除任务组
    // /// </summary>
    // [HttpPost]
    // [Route(template: "DeleteTaskGroup")]
    // public Task<ApiResponseJson> DeleteTaskGroup(OnlyIdRequest request)
    // {
    //     return TaskGroupApp.DeleteAsync(taskGroupId: request.Id).ToSuccessAsync("删除成功");
    // }
    //
    // /// <summary>
    // ///     获取任务组信息
    // /// </summary>
    // [HttpPost]
    // [Route(template: "GetTaskGroupInfo")]
    // public Task<ApiResponseJson<TaskGroupDTO>> GetTaskGroupInfo(OnlyIdRequest request)
    // {
    //     var mapAsync = TaskQueryApp.ToEntityAsync(taskGroupId: request.Id);
    //     return mapAsync.ToSuccessAsync();
    // }
    //
    // /// <summary>
    // ///     同步缓存到数据库
    // /// </summary>
    // [HttpPost]
    // [Route(template: "SyncCacheToDb")]
    // public Task<ApiResponseJson> SyncCacheToDb()
    // {
    //     return TaskGroupApp.SyncTaskGroup().ToSuccessAsync();
    // }
    //
    // /// <summary>
    // ///     获取全部任务组列表
    // /// </summary>
    // [HttpPost]
    // [Route(template: "GetTaskGroupList")]
    // public Task<ApiResponseJson<PooledList<TaskGroupDTO>>> GetTaskGroupList()
    // {
    //     return TaskQueryApp.ToListAsync().MapAsync<TaskGroupDTO, TaskGroupDO>().ToSuccessAsync();
    // }
    //
    // /// <summary>
    // ///     获取任务组数量
    // /// </summary>
    // [HttpPost]
    // [Route(template: "GetTaskGroupCount")]
    // public Task<ApiResponseJson<int>> GetTaskGroupCount()
    // {
    //     return TaskQueryApp.GetTaskGroupCount().ToSuccessAsync();
    // }
    //
    // /// <summary>
    // ///     获取未执行的任务数量
    // /// </summary>
    // [HttpPost]
    // [Route(template: "GetTaskGroupUnRunCount")]
    // public Task<ApiResponseJson<int>> GetTaskGroupUnRunCount()
    // {
    //     return TaskQueryApp.ToUnRunCountAsync().ToSuccessAsync();
    // }
    //
    // /// <summary>
    // ///     添加任务组
    // /// </summary>
    // [HttpPost]
    // [Route(template: "AddTaskGroup")]
    // public Task<ApiResponseJson<int>> AddTaskGroup(TaskGroupDTO request)
    // {
    //     if (request.Caption == null || request.Cron == null || request.Data == null || request.JobName == null) return ApiResponseJson<int>.ErrorAsync(statusMessage: "标题、时间间隔、传输数据、Job名称 必须填写");
    //     return TaskGroupApp.AddAsync(dto: request).ToSuccessAsync();
    // }
    //
    // /// <summary>
    // ///     修改任务组或设置Enable
    // /// </summary>
    // [HttpPost]
    // [Route(template: "SaveTaskGroup")]
    // public Task<ApiResponseJson> SaveTaskGroup(TaskGroupDTO request)
    // {
    //     return TaskGroupApp.Save(dto: request).ToSuccessAsync();
    // }
    //
    // /// <summary>
    // ///     今日执行失败数量
    // /// </summary>
    // [HttpPost]
    // [Route(template: "TodayTaskFailCount")]
    // public Task<ApiResponseJson<int>> TodayTaskFailCount()
    // {
    //     return TaskQueryApp.TodayFailCountAsync().ToSuccessAsync();
    // }
    //
    // /// <summary>
    // ///     获取进行中的任务
    // /// </summary>
    // [HttpPost]
    // [Route(template: "GetTaskUnFinishList")]
    // public Task<ApiResponseJson<List<TaskDTO>>> GetTaskUnFinishList(OnlyTopRequest request)
    // {
    //     return TaskQueryApp.GetTaskUnFinishList(top: request.Top).ToSuccessAsync();
    // }
    //
    // /// <summary>
    // ///     获取在用的任务
    // /// </summary>
    // [HttpPost]
    // [Route(template: "GetEnableTaskList")]
    // public ApiResponseJson<PageList<TaskDTO>> GetEnableTaskList(GetAllTaskListRequest request)
    // {
    //     return TaskQueryApp.GetEnableTaskList(status: request.Status, pageSize: request.PageSize, pageIndex: request.PageIndex).ToSuccess();
    // }
    //
    // /// <summary>
    // ///     获取指定任务组的任务列表
    // /// </summary>
    // [HttpPost]
    // [Route(template: "GetTaskList")]
    // public ApiResponseJson<Task<PageList<TaskDTO>>> GetTaskList(GetTaskListRequest request)
    // {
    //     return TaskQueryApp.ToListAsync(groupId: request.GroupId, pageSize: request.PageSize, pageIndex: request.PageIndex).ToSuccess();
    // }
    //
    // /// <summary>
    // ///     获取已完成的任务列表
    // /// </summary>
    // [HttpPost]
    // [Route(template: "GetTaskFinishList")]
    // public ApiResponseJson<Task<PageList<TaskDTO>>> GetTaskFinishList(PageSizeRequest request)
    // {
    //     return TaskQueryApp.ToFinishPageListAsync(pageSize: request.PageSize, pageIndex: request.PageIndex).ToSuccess();
    // }
    //
    // /// <summary>
    // ///     取消任务
    // /// </summary>
    // [HttpPost]
    // [Route(template: "CancelTask")]
    // public Task<ApiResponseJson> CancelTask(OnlyIdRequest request)
    // {
    //     return TaskGroupApp.CancelTask(taskGroupId: request.Id).ToSuccessAsync();
    // }
    //
    // /// <summary>
    // ///     获取日志
    // /// </summary>
    // [HttpPost]
    // [Route(template: "GetRunLogList")]
    // public ApiResponseJson<PageList<TaskLogDTO>> GetRunLogList(GetRunLogRequest request)
    // {
    //     return TaskLogApp.GetList(jobName: request.JobName, logLevel: request.LogLevel, pageSize: request.PageSize, pageIndex: request.PageIndex).ToSuccess();
    // }
}