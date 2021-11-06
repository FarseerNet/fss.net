using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FS.Core;
using FS.Core.Net;
using FSS.Abstract.Entity;
using FSS.Abstract.Entity.MetaInfo;
using FSS.Abstract.Server.MetaInfo;
using FSS.Abstract.Server.RegisterCenter;
using FSS.Service.Request;
using Microsoft.AspNetCore.Mvc;

namespace FSS.Service.Controllers
{
    /// <summary>
    /// 基础信息
    /// </summary>
    [ApiController]
    [Route("meta")]
    public class MetaController : ControllerBase
    {
        public IClientRegister  ClientRegister  { get; set; }
        public ITaskGroupAdd    TaskGroupAdd    { get; set; }
        public ITaskGroupList   TaskGroupList   { get; set; }
        public ITaskGroupUpdate TaskGroupUpdate { get; set; }
        public ITaskAdd         TaskAdd         { get; set; }
        public ITaskInfo        TaskInfo        { get; set; }
        public ITaskList        TaskList        { get; set; }
        public ITaskGroupInfo   TaskGroupInfo   { get; set; }
        public ITaskGroupDelete TaskGroupDelete { get; set; }
        public ITaskUpdate      TaskUpdate      { get; set; }

        /// <summary>
        /// 客户端拉取任务
        /// </summary>
        [HttpPost]
        [Route("GetClientList")]
        public async Task<ApiResponseJson<List<ClientVO>>> GetClientList()
        {
            // 取出全局客户端列表
            var lst = await ClientRegister.ToListAsync();
            return await ApiResponseJson<List<ClientVO>>.SuccessAsync(lst);
        }

        /// <summary>
        /// 取出全局客户端数量
        /// </summary>
        [HttpPost]
        [Route("GetClientCount")]
        public async Task<ApiResponseJson<long>> GetClientCount()
        {
            // 取出全局客户端列表
            var count = await ClientRegister.GetClientCountAsync();
            return await ApiResponseJson<long>.SuccessAsync(count);
        }

        /// <summary>
        /// 添加任务组
        /// </summary>
        [HttpPost]
        [Route("AddTaskGroup")]
        public async Task<ApiResponseJson<int>> AddTaskGroup(TaskGroupVO request)
        {
            if (request.Caption == null || request.Cron == null || request.Data == null || request.JobName == null)
            {
                return await ApiResponseJson<int>.ErrorAsync("标题、时间间隔、传输数据、Job名称 必须填写");
            }
            request.Id = await TaskGroupAdd.AddAsync(request);
            await TaskAdd.GetOrCreateAsync(request.Id);
            return await ApiResponseJson<int>.SuccessAsync("添加成功", request.Id);
        }

        /// <summary>
        /// 复制任务组
        /// </summary>
        [HttpPost]
        [Route("CopyTaskGroup")]
        public async Task<ApiResponseJson<int>> CopyTaskGroup(OnlyIdRequest request)
        {
            var info = await TaskGroupInfo.ToInfoAsync(request.Id);
            if (info == null) return await ApiResponseJson<int>.ErrorAsync("要复制的任务组不存在");

            info.Caption     += "复制";
            info.Id          =  0;
            info.ActivateAt  =  DateTime.MinValue;
            info.NextAt      =  DateTime.MinValue;
            info.TaskId      =  0;
            info.RunCount    =  0;
            info.LastRunAt   =  DateTime.MinValue;
            info.RunSpeedAvg =  0;
            if (info.IntervalMs > 0)
            {
                info.Cron       = info.IntervalMs.ToString();
                info.IntervalMs = 0;
            }

            request.Id = await TaskGroupAdd.AddAsync(info);
            return await ApiResponseJson<int>.SuccessAsync("复制成功", request.Id);
        }

        /// <summary>
        /// 删除任务组
        /// </summary>
        [HttpPost]
        [Route("DeleteTaskGroup")]
        public async Task<ApiResponseJson> DeleteTaskGroup(OnlyIdRequest request)
        {
            await TaskGroupDelete.DeleteAsync(request.Id);
            return await ApiResponseJson.SuccessAsync("删除成功", request.Id);
        }

        /// <summary>
        /// 获取任务组信息
        /// </summary>
        [HttpPost]
        [Route("GetTaskGroupInfo")]
        public async Task<ApiResponseJson<TaskGroupVO>> GetTaskGroupInfo(OnlyIdRequest request)
        {
            var info = await TaskGroupInfo.ToInfoAsync(request.Id);
            return await ApiResponseJson<TaskGroupVO>.SuccessAsync(info);
        }

        /// <summary>
        /// 获取全部任务列表
        /// </summary>
        [HttpPost]
        [Route("GetTaskGroupListAndSave")]
        public async Task<ApiResponseJson<List<TaskGroupVO>>> GetTaskGroupListAndSave()
        {
            var lst = await TaskGroupList.ToListAndSaveAsync();
            return await ApiResponseJson<List<TaskGroupVO>>.SuccessAsync(lst);
        }

        /// <summary>
        /// 获取全部任务组列表
        /// </summary>
        [HttpPost]
        [Route("GetTaskGroupList")]
        public async Task<ApiResponseJson<List<TaskGroupVO>>> GetTaskGroupList()
        {
            var lst = await TaskGroupList.ToListInCacheAsync();
            return await ApiResponseJson<List<TaskGroupVO>>.SuccessAsync(lst);
        }

        /// <summary>
        /// 获取全部任务组列表
        /// </summary>
        [HttpPost]
        [Route("GetTaskGroupCount")]
        public async Task<ApiResponseJson<long>> GetTaskGroupCount()
        {
            var count = await TaskGroupList.Count();
            return await ApiResponseJson<long>.SuccessAsync(count);
        }

        /// <summary>
        /// 获取未执行的任务数量
        /// </summary>
        [HttpPost]
        [Route("GetTaskGroupUnRunCount")]
        public async Task<ApiResponseJson<long>> GetTaskGroupUnRunCount()
        {
            var count = await TaskGroupList.ToUnRunCountAsync();
            return await ApiResponseJson<long>.SuccessAsync(count);
        }

        /// <summary>
        /// 更新TaskGroup
        /// </summary>
        [HttpPost]
        [Route("UpdateTaskGroup")]
        public async Task<ApiResponseJson> UpdateTaskGroup(TaskGroupVO vo)
        {
            await TaskGroupUpdate.UpdateAsync(vo);
            return await ApiResponseJson.SuccessAsync();
        }

        /// <summary>
        /// 保存TaskGroup
        /// </summary>
        [HttpPost]
        [Route("SaveTaskGroup")]
        public async Task<ApiResponseJson> SaveTaskGroup(TaskGroupVO vo)
        {
            await TaskGroupUpdate.SaveAsync(vo);
            return await ApiResponseJson.SuccessAsync();
        }

        /// <summary>
        /// 创建Task，并更新到缓存
        /// </summary>
        [HttpPost]
        [Route("GetOrCreateTask")]
        public async Task<ApiResponseJson<TaskVO>> GetOrCreateTask(OnlyIdRequest request)
        {
            var task = await TaskAdd.GetOrCreateAsync(request.Id);
            return await ApiResponseJson<TaskVO>.SuccessAsync(task);
        }

        /// <summary>
        /// 获取任务信息
        /// </summary>
        [HttpPost]
        [Route("GetTaskInfoByDb")]
        public async Task<ApiResponseJson<TaskVO>> GetTaskInfoByDb(OnlyIdRequest request)
        {
            var task = await TaskInfo.ToInfoByDbAsync(request.Id);
            return await ApiResponseJson<TaskVO>.SuccessAsync(task);
        }

        /// <summary>
        /// 今日执行失败数量
        /// </summary>
        [HttpPost]
        [Route("TodayTaskFailCount")]
        public async Task<ApiResponseJson<int>> TodayTaskFailCount()
        {
            var count = await TaskInfo.TodayFailCountAsync();
            return await ApiResponseJson<int>.SuccessAsync(count);
        }

        /// <summary>
        /// 获取全部任务列表
        /// </summary>
        [HttpPost]
        [Route("GetTopTaskList")]
        public async Task<ApiResponseJson<List<TaskVO>>> GetTopTaskList(OnlyTopRequest request)
        {
            var lst = await TaskList.ToTopListAsync(request.Top);
            return await ApiResponseJson<List<TaskVO>>.SuccessAsync(lst);
        }

        /// <summary>
        /// 获取指定任务组的任务列表
        /// </summary>
        [HttpPost]
        [Route("GetTaskList")]
        public async Task<ApiResponseJson<DataSplitList<TaskVO>>> GetTaskList(GetTaskListRequest request)
        {
            var lst = await TaskList.ToListAsync(request.GroupId, request.PageSize, request.PageIndex, out var totalCount);
            return await ApiResponseJson<DataSplitList<TaskVO>>.SuccessAsync(new DataSplitList<TaskVO>(lst, totalCount));
        }

        /// <summary>
        /// 获取失败的任务数量
        /// </summary>
        [HttpPost]
        [Route("GetTaskFailList")]
        public async Task<ApiResponseJson<DataSplitList<TaskVO>>> GetTaskFailList(PageSizeRequest request)
        {
            var lst = await TaskList.ToFailListAsync(request.PageSize, request.PageIndex, out var totalCount);
            return await ApiResponseJson<DataSplitList<TaskVO>>.SuccessAsync(new DataSplitList<TaskVO>(lst, totalCount));
        }

        /// <summary>
        /// 获取未执行的任务列表
        /// </summary>
        [HttpPost]
        [Route("GetTaskUnRunList")]
        public async Task<ApiResponseJson<DataSplitList<TaskVO>>> GetTaskUnRunList(PageSizeRequest request)
        {
            var lst = await TaskList.ToUnRunListAsync(request.PageSize, request.PageIndex, out var totalCount);
            return await ApiResponseJson<DataSplitList<TaskVO>>.SuccessAsync(new DataSplitList<TaskVO>(lst, totalCount));
        }

        /// <summary>
        /// 移除缓存
        /// </summary>
        [HttpPost]
        [Route("ClearTaskCache")]
        public async Task<ApiResponseJson> ClearTaskCache()
        {
            await TaskUpdate.ClearCacheAsync();
            return await ApiResponseJson.SuccessAsync();
        }

        /// <summary>
        /// 任务组修改时，需要同步JobName
        /// </summary>
        [HttpPost]
        [Route("UpdateTaskJobName")]
        public async Task<ApiResponseJson> UpdateTaskJobName(UpdateTaskJobNameRequest request)
        {
            await TaskUpdate.UpdateJobName(request.TaskId, request.JobName);
            return await ApiResponseJson.SuccessAsync();
        }

        /// <summary>
        /// 取消任务
        /// </summary>
        [HttpPost]
        [Route("CancelTask")]
        public async Task<ApiResponseJson> CancelTask(OnlyIdRequest request)
        {
            await TaskUpdate.CancelTask(request.Id);
            return await ApiResponseJson.SuccessAsync();
        }
    }
}