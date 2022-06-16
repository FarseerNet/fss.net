using System.Threading.Tasks;
using FS.Core;
using FS.Core.Abstract.AspNetCore;
using FS.Core.Abstract.Data;
using FS.DI;
using FS.Extends;
using FSS.Application.Log.TaskLog.Entity;
using FSS.Domain.Log.TaskLog;
using FSS.Domain.Log.TaskLog.Repository;
using FSS.Domain.Tasks.TaskGroup;
using FSS.Domain.Tasks.TaskGroup.Entity;
using FSS.Service.Request;
using Microsoft.Extensions.Logging;

namespace FSS.Application.Log.TaskLog;

[UseApi(Area = "meta")]
public class TaskLogApp : ISingletonDependency
{
    public TaskLogService     TaskLogService    { get; set; }
    public ITaskLogRepository TaskLogRepository { get; set; }

    /// <summary>
    ///     添加日志记录
    /// </summary>
    public void Add(int taskGroupId, string jobName, string caption, LogLevel logLevel, string content) => TaskLogService.Add(taskGroupId: taskGroupId, jobName: jobName, caption: caption, logLevel: logLevel, content: content);

    /// <summary>
    ///     获取日志
    /// </summary>
    [Api("GetRunLogList")]
    public PageList<TaskLogDTO> GetList(GetRunLogRequest request) => TaskLogRepository.GetList(jobName: request.JobName, logLevel: request.LogLevel, pageSize: request.PageSize, pageIndex: request.PageIndex).Map<TaskLogDTO>();
}