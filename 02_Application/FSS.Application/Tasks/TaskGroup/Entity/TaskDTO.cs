using System;
using System.Collections.Generic;
using FS.Extends;
using FS.Mapper;
using FSS.Domain.Tasks.TaskGroup.Entity;
using FSS.Domain.Tasks.TaskGroup.Enum;

namespace FSS.Application.Tasks.TaskGroup.Entity;

/// <summary>
///     任务记录
/// </summary>
public class TaskDTO
{
    /// <summary>
    ///     主键
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    ///     任务组ID
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
    ///     开始时间
    /// </summary>
    public DateTime StartAt { get; set; }

    /// <summary>
    ///     实际执行时间
    /// </summary>
    public DateTime RunAt { get; set; }

    /// <summary>
    ///     运行耗时
    /// </summary>
    public long RunSpeed { get; set; }

    /// <summary>
    ///     客户端Id
    /// </summary>
    public long ClientId { get; set; }

    /// <summary>
    ///     客户端IP
    /// </summary>
    public string ClientIp { get; set; }

    /// <summary>
    ///     客户端名称
    /// </summary>
    public string ClientName { get; set; }

    /// <summary>
    ///     进度0-100
    /// </summary>
    public int Progress { get; set; }

    /// <summary>
    ///     状态
    /// </summary>
    public EumTaskType Status { get; set; }

    /// <summary>
    ///     任务创建时间
    /// </summary>
    public DateTime CreateAt { get; set; }

    /// <summary>
    ///     调度时间
    /// </summary>
    public DateTime SchedulerAt { get; set; }

    /// <summary>
    ///     数据
    /// </summary>
    public Dictionary<string, string> Data { get; set; }
    
    public static implicit operator TaskDTO(TaskDO task) => task.Map<TaskDTO>();
}