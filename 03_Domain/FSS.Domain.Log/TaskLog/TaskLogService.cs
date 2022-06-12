using FS.DI;
using FSS.Domain.Log.TaskLog.Repository;
using Microsoft.Extensions.Logging;

namespace FSS.Domain.Log.TaskLog;

public class TaskLogService : ISingletonDependency
{
    public ITaskLogRepository TaskLogRepository { get; set; }

    /// <summary>
    ///     添加日志记录
    /// </summary>
    public void Add(int taskGroupId, string jobName, string caption, LogLevel logLevel, string content)
    {
        TaskLogRepository.Add(new TaskLogDO(taskGroupId, jobName, caption, logLevel, content));
    }
}