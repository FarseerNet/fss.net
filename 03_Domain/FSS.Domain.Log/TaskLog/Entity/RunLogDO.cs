using System;
using System.Threading.Tasks;
using FS.DI;
using FS.Extends;
using FS.Mapper;
using FSS.Infrastructure.Repository.Log.Entity;
using FSS.Infrastructure.Repository.Log.Interface;
using Microsoft.Extensions.Logging;

namespace FSS.Domain.Log.TaskLog.Entity
{
    /// <summary>
    /// 运行日志
    /// </summary>
    [Map(typeof(RunLogPO))]
    public class RunLogDO
    {
        /// <summary>
        /// 主键
        /// </summary>
        public long? Id { get; set; }

        /// <summary>
        /// 任务组记录ID
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
        /// 日志级别
        /// </summary>
        public LogLevel LogLevel { get; set; }

        /// <summary>
        /// 日志内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 日志时间
        /// </summary>
        public DateTime CreateAt { get; set; }

        public static implicit operator RunLogPO(RunLogDO runLogDo) => runLogDo.Map<RunLogPO>();

        /// <summary>
        /// 添加日志到队列
        /// </summary>
        public Task AddAsync()
        {
            if (LogLevel is LogLevel.Error or LogLevel.Warning) IocManager.Instance.Logger<RunLogDO>().Log(LogLevel, Content);
            Caption  ??= "";
            JobName  ??= "";
            CreateAt =   DateTime.Now;

            return IocManager.GetService<ILogQueue>().AddQueueAsync(this);
        }
    }
}