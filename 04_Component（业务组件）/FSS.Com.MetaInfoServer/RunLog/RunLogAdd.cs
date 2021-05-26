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
            if (logLevel is LogLevel.Error) IocManager.Logger<RunLogAdd>().Log(logLevel, content);
            var groupInfo = await TaskGroupInfo.ToInfoAsync(taskGroupId) ?? new TaskGroupVO();
            var runLogPO = new RunLogPO
            {
                TaskGroupId = taskGroupId,
                TaskId      = taskId,
                Caption     = groupInfo.Caption ?? "",
                JobName     = groupInfo.JobName ?? "",
                LogLevel = logLevel,
                Content  = content,
                CreateAt = DateTime.Now
            };

            await IocManager.Resolve<IRedisStreamProduct>("RonLogQueue").SendAsync(runLogPO);
        }
    }
}