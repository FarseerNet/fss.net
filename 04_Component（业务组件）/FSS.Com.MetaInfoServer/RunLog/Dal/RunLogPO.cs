using System;
using FS.Core.Mapping.Attribute;
using FS.Mapper;
using Nest;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace FSS.Com.MetaInfoServer.RunLog.Dal
{
    /// <summary>
    /// 运行日志
    /// </summary>
    //[Map(typeof(RunLogVO))]
    public class RunLogPO
    {
        /// <summary>
        /// 主键
        /// </summary>
        [Field(Name = "id",IsPrimaryKey = true,IsDbGenerated = true),Number(NumberType.Long)]
        public long? Id { get; set; }

        /// <summary>
        /// 任务组记录ID
        /// </summary>
        [Field(Name = "task_group_id"),Number(NumberType.Integer)]
        public int TaskGroupId { get; set; }
        
        /// <summary>
        /// 任务记录ID
        /// </summary>
        [Field(Name = "task_id"),Number(NumberType.Integer)]
        public int TaskId { get; set; }
        
        /// <summary>
        /// 任务组标题
        /// </summary>
        [Field(Name = "caption"),Keyword]
        public string Caption { get; set; }
        
        /// <summary>
        /// 实现Job的特性名称（客户端识别哪个实现类）
        /// </summary>
        [Field(Name = "job_name"),Keyword]
        public string JobName { get; set; }
        
        /// <summary>
        /// 日志级别
        /// </summary>
        [Field(Name = "log_level"),Number(NumberType.Byte)]
        public LogLevel LogLevel { get; set; }
        
        /// <summary>
        /// 日志内容
        /// </summary>
        [Field(Name = "content"),Text]
        public string Content { get; set; }
        
        /// <summary>
        /// 日志时间
        /// </summary>
        [Field(Name = "create_at"),Date]
        public DateTime CreateAt { get; set; }
    }
}