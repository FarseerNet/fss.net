using System;
using FS.Core.Mapping.Attribute;

namespace FSS.Com.MetaInfoServer.Task.Dal
{
    /// <summary>
    /// 任务记录
    /// </summary>
    public class TaskPO
    {
        /// <summary>
        /// 主键
        /// </summary>
        [Field(Name = "id",IsPrimaryKey = true)]
        public int? Id { get; set; }
        
        /// <summary>
        /// 任务的标题
        /// </summary>
        [Field(Name = "caption")]
        public string Caption { get; set; }
        
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
        /// 上一次运行耗时
        /// </summary>
        [Field(Name = "last_run_speed")]
        public int? LastRunSpeed { get; set; }
        
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