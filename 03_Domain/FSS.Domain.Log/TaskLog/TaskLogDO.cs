using FS.DI;
using FSS.Domain.Log.TaskLog.Repository;
using Microsoft.Extensions.Logging;

namespace FSS.Domain.Log.TaskLog;

/// <summary>
///     运行日志
/// </summary>
public class TaskLogDO
{
    /// <summary>
    ///     主键
    /// </summary>
    public long? Id { get; set; }

    /// <summary>
    ///     任务组记录ID
    /// </summary>
    public int TaskGroupId { get; set; }

    /// <summary>
    ///     任务组标题
    /// </summary>
    public string Caption { get; set; }

    /// <summary>
    ///     实现Job的特性名称（客户端识别哪个实现类）
    /// </summary>
    public string JobName { get; set; }

    /// <summary>
    ///     日志级别
    /// </summary>
    public LogLevel LogLevel { get; set; }

    /// <summary>
    ///     日志内容
    /// </summary>
    public string Content { get; set; }

    /// <summary>
    ///     日志时间
    /// </summary>
    public DateTime CreateAt { get; set; }

    /// <summary>
    ///     添加日志到队列
    /// </summary>
    public Task AddAsync()
    {
        if (LogLevel is LogLevel.Error or LogLevel.Warning) IocManager.Instance.Logger<TaskLogDO>().Log(logLevel: LogLevel, message: Content);
        Caption  ??= "";
        JobName  ??= "";
        CreateAt =   DateTime.Now;

        return IocManager.GetService<ITaskLogRepository>().AddAsync(taskLogDO: this);
    }
}