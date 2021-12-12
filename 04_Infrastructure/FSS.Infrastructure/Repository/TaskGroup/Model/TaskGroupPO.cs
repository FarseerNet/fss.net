using System;
using FS.Core.Mapping.Attribute;
using FS.Mapper;
using FSS.Domain.Tasks.TaskGroup.Entity;
using FSS.Infrastructure.Repository.Tasks.Model;

namespace FSS.Infrastructure.Repository.TaskGroup.Model
{
    /// <summary>
    /// 任务组记录
    /// </summary>
    [Map(typeof(TaskGroupDO))]
    public class TaskGroupPO
    {
        /// <summary>
        /// 主键
        /// </summary>
        [Field(Name = "id", IsPrimaryKey = true, IsDbGenerated = true)]
        public int? Id { get; set; }

        /// <summary>
        /// 任务
        /// </summary>
        [Field(IsMap = false)]
        public TaskPO Task { get; set; }

        /// <summary>
        /// 任务组标题
        /// </summary>
        [Field(Name = "caption")]
        public string Caption { get; set; }

        /// <summary>
        /// 实现Job的特性名称（客户端识别哪个实现类）
        /// </summary>
        [Field(Name = "job_name")]
        public string JobName { get; set; }

        /// <summary>
        /// 传给客户端的参数，按逗号分隔
        /// </summary>
        [Field(Name = "data")]
        public string Data { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        [Field(Name = "start_at")]
        public DateTime? StartAt { get; set; }

        /// <summary>
        /// 下次执行时间
        /// </summary>
        [Field(Name = "next_at")]
        public DateTime? NextAt { get; set; }

        /// <summary>
        /// 时间间隔
        /// </summary>
        [Field(Name = "interval_ms")]
        public long? IntervalMs { get; set; }

        /// <summary>
        /// 时间定时器表达式
        /// </summary>
        [Field(Name = "cron")]
        public string Cron { get; set; }

        /// <summary>
        /// 活动时间
        /// </summary>
        [Field(Name = "activate_at")]
        public DateTime? ActivateAt { get; set; }

        /// <summary>
        /// 最后一次完成时间
        /// </summary>
        [Field(Name = "last_run_at")]
        public DateTime? LastRunAt { get; set; }

        /// <summary>
        /// 运行平均耗时
        /// </summary>
        [Field(Name = "run_speed_avg")]
        public long? RunSpeedAvg { get; set; }

        /// <summary>
        /// 运行次数
        /// </summary>
        [Field(Name = "run_count")]
        public int? RunCount { get; set; }

        /// <summary>
        /// 是否开启
        /// </summary>
        [Field(Name = "is_enable")]
        public bool? IsEnable { get; set; }
    }
}