using System;
using System.Threading.Tasks;
using FS.DI;
using FS.MQ.RedisStream;
using FSS.Abstract.Entity.MetaInfo;
using FSS.Abstract.Server.MetaInfo;
using FSS.Com.MetaInfoServer.RunLog.Dal;
using Microsoft.Extensions.Logging;

namespace FSS.Com.MetaInfoServer.RunLog
{
    /// <summary>
    /// 运行日志添加操作
    /// </summary>
    public class RunLogAdd : IRunLogAdd
    {
        public IIocManager    IocManager    { get; set; }
        public ITaskGroupInfo TaskGroupInfo { get; set; }

        /// <summary>
        /// 添加日志记录
        /// </summary>
        public async Task AddAsync(int taskGroupId, int taskId, LogLevel logLevel, string content)
        {
            var groupInfo = await TaskGroupInfo.ToInfoAsync(taskGroupId) ?? new TaskGroupVO();
            await AddAsync(groupInfo, taskId, logLevel, content);
        }

        /// <summary>
        /// 添加日志记录
        /// </summary>
        public async Task AddAsync(TaskGroupVO groupInfo, int taskId, LogLevel logLevel, string content)
        {
            if (logLevel is LogLevel.Error or LogLevel.Warning) IocManager.Logger<RunLogAdd>().Log(logLevel, content);
            var runLogPO = new RunLogPO
            {
                TaskGroupId = groupInfo.Id,
                TaskId      = taskId,
                Caption     = groupInfo.Caption ?? "",
                JobName     = groupInfo.JobName ?? "",
                LogLevel    = logLevel,
                Content     = content,
                CreateAt    = DateTime.Now
            };

            await IocManager.Resolve<IRedisStreamProduct>("RunLogQueue").SendAsync(runLogPO);
        }
    }
}