using FS.Core;
using FS.DI;
using Microsoft.Extensions.Logging;

namespace FSS.Domain.Log.TaskLog.Repository;

public interface ITaskLogRepository : ISingletonDependency
{
    /// <summary>
    ///     获取日志
    /// </summary>
    PageList<TaskLogDO> GetList(string jobName, LogLevel? logLevel, int pageSize, int pageIndex);
    /// <summary>
    ///     添加日志
    /// </summary>
    void Add(TaskLogDO taskLogDO);
}