using System.Collections.Generic;
using System.Threading.Tasks;
using FS.Core.Net;
using FSS.Application.Log.TaskLog;
using FSS.Application.Tasks.TaskGroup;
using FSS.Application.Tasks.TaskGroup.Entity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FSS.Service.Controllers
{
    /// <summary>
    /// 任务相关信息
    /// </summary>
    [ApiController]
    [Route("task")]
    public class TaskController : BaseController
    {
        public TaskLogApp       TaskLogApp       { get; set; }
        public TaskProcessApp   TaskProcessApp   { get; set; }
        public TaskQueryApp     TaskQueryApp     { get; set; }
        public TaskSchedulerApp TaskSchedulerApp { get; set; }

        public TaskController(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }

        /// <summary>
        /// 客户端拉取任务
        /// </summary>
        [HttpPost]
        [Route("Pull")]
        public async Task<ApiResponseJson<List<TaskDTO>>> Pull(PullDTO dto)
        {
            // 拉取任务
            var lstTask = await TaskSchedulerApp.TaskSchedulerAsync(Client, dto.TaskCount) ?? new List<TaskDTO>();
            return await ApiResponseJson<List<TaskDTO>>.SuccessAsync("默认", lstTask);
        }

        /// <summary>
        /// 客户端执行任务
        /// </summary>
        [HttpPost]
        [Route("JobInvoke")]
        public async Task<ApiResponseJson> JobInvoke(JobInvokeDTO dto)
        {
            var taskGroup = await TaskQueryApp.ToEntityAsync(dto.TaskGroupId);
            if (taskGroup == null)
            {
                await TaskLogApp.AddAsync(dto.TaskGroupId, "", "", LogLevel.Warning, $"所属的任务组：{dto.TaskGroupId} 不存在");
                return await ApiResponseJson.ErrorAsync($"所属的任务组：{dto.TaskGroupId} 不存在");
            }

            await TaskProcessApp.JobInvoke(dto, taskGroup, Client);
            return await ApiResponseJson.SuccessAsync($"任务组：TaskGroupId={dto.TaskGroupId}，Caption={taskGroup.Caption}，JobName={taskGroup.JobName} 处理成功");
        }
    }
}