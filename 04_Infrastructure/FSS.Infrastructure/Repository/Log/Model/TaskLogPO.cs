using System;
using FS.Core.Mapping.Attribute;
using FS.Mapper;
using FSS.Domain.Log.TaskLog;
using Nest;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace FSS.Infrastructure.Repository.Log.Model;

/// <summary>
///     运行日志
/// </summary>
[Map(typeof(TaskLogDO))]
public class TaskLogPO
{
    /// <summary>
    ///     主键
    /// </summary>
    [Field(Name = "id", IsPrimaryKey = true, IsDbGenerated = true)] [Number(type: NumberType.Long)]
    public long? Id { get; set; }

    /// <summary>
    ///     任务组记录ID
    /// </summary>
    [Field(Name = "task_group_id")] [Number(type: NumberType.Integer)]
    public int TaskGroupId { get; set; }

    /// <summary>
    ///     任务组标题
    /// </summary>
    [Field(Name = "caption")] [Keyword]
    public string Caption { get; set; }

    /// <summary>
    ///     实现Job的特性名称（客户端识别哪个实现类）
    /// </summary>
    [Field(Name = "job_name")] [Keyword]
    public string JobName { get; set; }

    /// <summary>
    ///     日志级别
    /// </summary>
    [Field(Name = "log_level")] [Number(type: NumberType.Byte)]
    public LogLevel LogLevel { get; set; }

    /// <summary>
    ///     日志内容
    /// </summary>
    [Field(Name = "content")] [Text]
    public string Content { get; set; }

    /// <summary>
    ///     日志时间
    /// </summary>
    [Field(Name = "create_at")] [Date]
    public DateTime CreateAt { get; set; }
}