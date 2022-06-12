using System;
using FS.Core.Mapping.Attribute;
using FS.Extends;
using FSS.Domain.Tasks.TaskGroup.Entity;
using FSS.Domain.Tasks.TaskGroup.Enum;

namespace FSS.Infrastructure.Repository.Tasks.Model;

/// <summary>
///     任务记录
/// </summary>
public class TaskPO
{
    /// <summary>
    ///     主键
    /// </summary>
    [Field(Name = "id", IsPrimaryKey = true, IsDbGenerated = true)]
    public int? Id { get; set; }

    /// <summary>
    ///     任务组ID
    /// </summary>
    [Field(Name = "task_group_id")] public int? TaskGroupId { get; set; }

    /// <summary>
    ///     任务组标题
    /// </summary>
    [Field(Name = "caption")]
    public string Caption { get; set; }

    /// <summary>
    ///     实现Job的特性名称（客户端识别哪个实现类）
    /// </summary>
    [Field(Name = "job_name")]
    public string JobName { get; set; }

    /// <summary>
    ///     开始时间
    /// </summary>
    [Field(Name = "start_at")] public DateTime? StartAt { get; set; }

    /// <summary>
    ///     实际执行时间
    /// </summary>
    [Field(Name = "run_at")] public DateTime? RunAt { get; set; }

    /// <summary>
    ///     运行耗时
    /// </summary>
    [Field(Name = "run_speed")] public long? RunSpeed { get; set; }

    /// <summary>
    ///     客户端Id
    /// </summary>
    [Field(Name = "client_id")] public long ClientId { get; set; }

    /// <summary>
    ///     客户端IP
    /// </summary>
    [Field(Name = "client_ip")] public string ClientIp { get; set; }

    /// <summary>
    ///     客户端名称
    /// </summary>
    [Field(Name = "client_name")] public string ClientName { get; set; }

    /// <summary>
    ///     进度0-100
    /// </summary>
    [Field(Name = "progress")] public int? Progress { get; set; }

    /// <summary>
    ///     状态
    /// </summary>
    [Field(Name = "status")] public EumTaskType? Status { get; set; }

    /// <summary>
    ///     任务创建时间
    /// </summary>
    [Field(Name = "create_at")] public DateTime? CreateAt { get; set; }

    /// <summary>
    ///     调度时间
    /// </summary>
    [Field(Name = "scheduler_at")] public DateTime? SchedulerAt { get; set; }

    public static implicit operator TaskPO(TaskDO task) => task.Map<TaskPO>();
    public static implicit operator TaskDO(TaskPO po)   => po.Map<TaskDO>();
}