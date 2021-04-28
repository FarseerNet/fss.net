using System;
using FSS.Abstract.Enum;
using Microsoft.Extensions.Logging;

namespace FSS.Abstract.Entity.RemoveCall
{
    /// <summary>
    /// 任务状态
    /// </summary>
    public class JobStatusVO
    {
        /// <summary>
        /// 任务ID
        /// </summary>
        public int TaskId { get; set; }
        
        /// <summary>
        /// 任务组标题
        /// </summary>
        public string Caption { get; set; }
        
        /// <summary>
        /// 实现Job的特性名称（客户端识别哪个实现类）
        /// </summary>
        public string JobTypeName { get; set; }
        
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartAt { get; set; }
        
        /// <summary>
        /// 下次执行时间
        /// </summary>
        public DateTime NextAt { get; set; }
        
        /// <summary>
        /// 进度0-100
        /// </summary>
        public int Progress { get; set; }
        
        /// <summary>
        /// 运行耗时
        /// </summary>
        public int RunSpeed { get; set; }
        
        /// <summary>
        /// 日志等级
        /// </summary>
        public LogLevel LogLevel { get; set; }
        
        /// <summary>
        /// 状态
        /// </summary>
        public EumTaskType Status { get; set; }
        
        /// <summary>
        /// 日志内容
        /// </summary>
        public string Log { get; set; }
    }
}