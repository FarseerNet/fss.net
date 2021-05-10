using System;
using FSS.Abstract.Enum;

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
        /// 任务组ID
        /// </summary>
        public int TaskGroupId { get; set; }
        
        /// <summary>
        /// 任务组标题
        /// </summary>
        public string Caption { get; set; }
        
        /// <summary>
        /// 实现Job的特性名称（客户端识别哪个实现类）
        /// </summary>
        public string JobName { get; set; }
        
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartAt { get; set; }
        
        /// <summary>
        /// 运行耗时
        /// </summary>
        public int RunSpeed { get; set; }
        
        /// <summary>
        /// 服务端节点
        /// </summary>
        public string ServerNode { get; set; }
        
        /// <summary>
        /// 客户端
        /// </summary>
        public string ClientHost { get; set; }
        
        /// <summary>
        /// 客户端IP
        /// </summary>
        public string ClientIp { get; set; }
        
        /// <summary>
        /// 进度0-100
        /// </summary>
        public int Progress { get; set; }
        
        /// <summary>
        /// 状态
        /// </summary>
        public EumTaskType Status { get; set; }
        
        /// <summary>
        /// 任务创建时间
        /// </summary>
        public DateTime CreateAt { get; set; }
        
        /// <summary>
        /// 调度时间（不存DB）
        /// </summary>
        public DateTime SchedulerAt { get; set; }
    }
}