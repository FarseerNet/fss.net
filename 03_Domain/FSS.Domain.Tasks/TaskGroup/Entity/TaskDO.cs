using System;
using System.Threading.Tasks;
using FS.DI;
using FS.Extends;
using FS.Mapper;
using FSS.Infrastructure.Repository.Tasks.Enum;
using FSS.Infrastructure.Repository.Tasks.Interface;
using FSS.Infrastructure.Repository.Tasks.Model;

namespace FSS.Domain.Tasks.TaskGroup.Entity
{
    /// <summary>
    /// 任务记录
    /// </summary>
    [Map(typeof(TaskPO))]
    public class TaskDO
    {
        public ITaskAgent TaskAgent { get; set; }

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
        /// 实际执行时间
        /// </summary>
        public DateTime RunAt { get; set; }

        /// <summary>
        /// 运行耗时
        /// </summary>
        public long RunSpeed { get; set; }

        /// <summary>
        /// 客户端Id
        /// </summary>
        public long ClientId { get; set; }

        /// <summary>
        /// 客户端IP
        /// </summary>
        public string ClientIp { get; set; }

        /// <summary>
        /// 客户端名称
        /// </summary>
        public string ClientName { get; set; }

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
        /// 调度时间
        /// </summary>
        public DateTime SchedulerAt { get; set; }

        public static implicit operator TaskPO(TaskDO taskDO) => taskDO.Map<TaskPO>();

        /// <summary>
        /// 创建任务
        /// </summary>
        public Task AddQueueAsync()
        {
            return IocManager.GetService<ITaskAgent>().AddQueueAsync(this);
        }
    }
}