using System;
using FS.Mapper;
using FSS.Domain.Tasks.TaskGroup.Entity;
using FSS.Infrastructure.Repository.TaskGroup.Interface;

namespace FSS.Application.Tasks.TaskGroup.Entity
{
    /// <summary>
    /// 任务组记录
    /// </summary>
    [Serializable]
    [Map(typeof(TaskGroupDO))]
    public class TaskGroupDTO
    {
        public ITaskGroupAgent TaskGroupAgent { get; set; }

        /// <summary>
        /// 主键
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 任务组标题
        /// </summary>
        public string Caption { get; set; }

        /// <summary>
        /// 实现Job的特性名称（客户端识别哪个实现类）
        /// </summary>
        public string JobName { get; set; }

        /// <summary>
        /// 传给客户端的参数，按逗号分隔
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartAt { get; set; }

        /// <summary>
        /// 下次执行时间
        /// </summary>
        public DateTime NextAt { get; set; }

        /// <summary>
        /// 时间间隔
        /// </summary>
        public long IntervalMs { get; set; }

        /// <summary>
        /// 时间定时器表达式
        /// </summary>
        public string Cron { get; set; }

        /// <summary>
        /// 活动时间
        /// </summary>
        public DateTime ActivateAt { get; set; }

        /// <summary>
        /// 最后一次完成时间
        /// </summary>
        public DateTime LastRunAt { get; set; }

        /// <summary>
        /// 是否开启
        /// </summary>
        public bool IsEnable { get; set; }
    }
}