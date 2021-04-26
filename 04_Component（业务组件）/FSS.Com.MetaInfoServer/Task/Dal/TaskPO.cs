using System;
using FS.Core.Mapping.Attribute;
using FS.Mapper;
using FSS.Abstract.Entity.MetaInfo;
using FSS.Abstract.Enum;

namespace FSS.Com.MetaInfoServer.Task.Dal
{
    /// <summary>
    /// 任务记录
    /// </summary>
    [Map(typeof(TaskVO))]
    public class TaskPO
    {
        /// <summary>
        /// 主键
        /// </summary>
        [Field(Name = "id", IsPrimaryKey = true, IsDbGenerated = true)]
        public int? Id { get; set; }

        /// <summary>
        /// 任务组ID
        /// </summary>
        [Field(Name = "task_group_id")] public int TaskGroupId { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        [Field(Name = "start_at")] public DateTime? StartAt { get; set; }

        /// <summary>
        /// 运行耗时
        /// </summary>
        [Field(Name = "run_speed")] public int? RunSpeed { get; set; }

        /// <summary>
        /// 客户端
        /// </summary>
        [Field(Name = "client_id")] public string ClientId { get; set; }

        /// <summary>
        /// 客户端IP
        /// </summary>
        [Field(Name = "client_endpoint")] public string ClientEndpoint { get; set; }

        /// <summary>
        /// 进度0-100
        /// </summary>
        [Field(Name = "progress")] public int? Progress { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        [Field(Name = "status")] public EumTaskType? Status { get; set; }

        /// <summary>
        /// 任务创建时间
        /// </summary>
        [Field(Name = "create_at")] public DateTime? CreateAt { get; set; }
    }
}