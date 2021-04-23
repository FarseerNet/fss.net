using System;

namespace FSS.Abstract.Entity.MetaInfo
{
    /// <summary>
    /// 任务记录
    /// </summary>
    public class TaskVO
    {
        /// <summary>
        /// 主键
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// 任务的标题
        /// </summary>
        public string Caption { get; set; }
        
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartAt { get; set; }
        
        /// <summary>
        /// 下次执行时间
        /// </summary>
        public DateTime NextAt { get; set; }
        
        /// <summary>
        /// 活动时间
        /// </summary>
        public DateTime ActivateAt { get; set; }
        
        /// <summary>
        /// 最后一次完成时间
        /// </summary>
        public DateTime LastRunAt { get; set; }
        
        /// <summary>
        /// 上一次运行耗时
        /// </summary>
        public int LastRunSpeed { get; set; }
        
        /// <summary>
        /// 运行次数
        /// </summary>
        public int RunCount { get; set; }
        
        /// <summary>
        /// 是否开启
        /// </summary>
        public bool IsEnable { get; set; }
    }
}