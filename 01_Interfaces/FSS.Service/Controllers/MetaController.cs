using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FS.Core;
using FS.Core.Net;
using FS.Extends;
using FSS.Abstract.Entity;
using FSS.Abstract.Entity.MetaInfo;
using FSS.Abstract.Enum;
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
        public IRunLogList      RunLogList      { get; set; }

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
        /// 同步缓存到数据库
        /// </summary>
        [HttpPost]
        [Route("SyncCacheToDb")]
        public async Task<ApiResponseJson<List<TaskGroupVO>>> SyncCacheToDb()
        {
            await TaskGroupUpdate.SyncCacheToDb();
            return await ApiResponseJson.SuccessAsync();
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
            var count = await TaskList.ToUnRunCountAsync();
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
        /// 修改任务组或设置Enable
        /// </summary>
        [HttpPost]
        [Route("SaveTaskGroup")]
        public async Task<ApiResponseJson> SaveTaskGroup(TaskGroupVO request)
        {
            var taskGroup = await TaskGroupInfo.ToInfoAsync(request.Id);
            if (taskGroup == null) return await ApiResponseJson.ErrorAsync("任务组不存在");

            await TaskGroupUpdate.SaveAsync(request);

            // 更新了JobName，则要立即更新Task的JobName
            if (taskGroup.JobName != request.JobName)
            {
                var task = await TaskInfo.ToInfoByGroupIdAsync(request.Id);
                if (task.Status == EumTaskType.None)
                {
                    task.JobName = request.JobName;
                    await TaskUpdate.UpdateAsync(task);
                }
            }

            // 任务是重新开启状态时，立即创建一个任务
            if (request.IsEnable && !taskGroup.IsEnable)
            {
                await TaskUpdate.CancelTask(request.Id);
            }

            return await ApiResponseJson.SuccessAsync();
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
        /// 获取进行中的任务
        /// </summary>
        [HttpPost]
        [Route("GetTaskUnFinishList")]
        public async Task<ApiResponseJson<List<TaskVO>>> GetTaskUnFinishList(OnlyTopRequest request)
        {
            var lst = await TaskList.ToGroupListAsync();
            lst = lst.Where(o => o.Status != EumTaskType.Success && o.Status != EumTaskType.Fail).OrderBy(o => o.StartAt).Take(request.Top).ToList();
            return await ApiResponseJson<List<TaskVO>>.SuccessAsync(lst);
        }

        /// <summary>
        /// 获取在用的任务
        /// </summary>
        [HttpPost]
        [Route("GetEnableTaskList")]
        public async Task<ApiResponseJson<DataSplitList<TaskVO>>> GetEnableTaskList(GetAllTaskListRequest request)
        {
            var lst          = await TaskList.ToGroupListAsync();
            // 移除被禁用的任务组
            var lstTaskGroup = await TaskGroupList.ToListInCacheAsync();
            foreach (var taskGroupVO in lstTaskGroup.Where(o => !o.IsEnable))
            {
                lst.RemoveAll(o => o.TaskGroupId == taskGroupVO.Id);
            }

            if (request.Status.HasValue) lst = lst.Where(o => o.Status == request.Status.GetValueOrDefault()).ToList();

            var totalCount = lst.Count;
            lst = lst.OrderBy(o => o.JobName).ToList(request.PageSize, request.PageIndex);
            return await ApiResponseJson<DataSplitList<TaskVO>>.SuccessAsync(new DataSplitList<TaskVO>(lst, totalCount));
        }

        /// <summary>
        /// 获取进行中的任务
        /// </summary>
        [HttpPost]
        [Route("GetTopTaskList")]
        public async Task<ApiResponseJson<List<TaskVO>>> GetTopTaskList(OnlyTopRequest request)
        {
            var lst = await TaskList.ToGroupListAsync();
            lst = lst.Where(o => o.Status != EumTaskType.Success && o.Status != EumTaskType.Fail).Take(request.Top).OrderBy(o => o.StartAt).ToList();
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
        /// 获取已完成的任务列表
        /// </summary>
        [HttpPost]
        [Route("GetTaskFinishList")]
        public async Task<ApiResponseJson<DataSplitList<TaskVO>>> GetTaskFinishList(PageSizeRequest request)
        {
            var lst = await TaskList.ToFinishListAsync(request.PageSize, request.PageIndex, out var totalCount);
            return await ApiResponseJson<DataSplitList<TaskVO>>.SuccessAsync(new DataSplitList<TaskVO>(lst, totalCount));
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

        /// <summary>
        /// 获取日志
        /// </summary>
        [HttpPost]
        [Route("GetRunLogList")]
        public async Task<ApiResponseJson<DataSplitList<RunLogVO>>> GetRunLogList(GetRunLogRequest request)
        {
            var lst = RunLogList.GetList(request.JobName, request.LogLevel, request.PageSize, request.PageIndex);
            return await ApiResponseJson<DataSplitList<RunLogVO>>.SuccessAsync(lst);
        }
    }
}