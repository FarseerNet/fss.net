using FS.DI;
using Microsoft.Extensions.Logging;

namespace FSS.Domain.Log.TaskLog;

public class TaskLogService : ISingletonDependency
{
    /// <summary>
    ///     添加日志记录
    /// </summary>
    public void Add(int taskGroupId, string jobName, string caption, LogLevel logLevel, string content) => new TaskLogDO
    {
        TaskGroupId = taskGroupId,
        Caption     = caption ?? "",
        JobName     = jobName ?? "",
        LogLevel    = logLevel,
        Content     = content,
        CreateAt    = DateTime.Now
    }.Add();
}