using System;
using System.Threading.Tasks;
using FS.DI;
using FS.MQ.RedisStream;
using FSS.Abstract.Entity.MetaInfo;
using FSS.Abstract.Server.MetaInfo;
using FSS.Com.MetaInfoServer.RunLog.Dal;
using Microsoft.Extensions.Configuration;
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
        public RunLogCache    RunLogCache   { get; set; }

        static readonly bool UseEs;

        static RunLogAdd()
        {
            var configurationSection = FS.DI.IocManager.Instance.Resolve<IConfigurationRoot>().GetSection("ElasticSearch:0:Server").Value;
            UseEs = !string.IsNullOrWhiteSpace(configurationSection);
        }

        /// <summary>
        /// 添加日志记录
        /// </summary>
        public async Task AddAsync(int taskGroupId, LogLevel logLevel, string content)
        {
            var groupInfo = await TaskGroupInfo.ToInfoAsync(taskGroupId) ?? new TaskGroupVO();
            await AddAsync(groupInfo, logLevel, content);
        }

        /// <summary>
        /// 添加日志记录
        /// </summary>
        public async Task AddAsync(TaskGroupVO groupInfo, LogLevel logLevel, string content)
        {
            if (logLevel is LogLevel.Error or LogLevel.Warning) IocManager.Logger<RunLogAdd>().Log(logLevel, content);
            var runLogPO = new RunLogPO
            {
                TaskGroupId = groupInfo.Id,
                Caption     = groupInfo.Caption ?? "",
                JobName     = groupInfo.JobName ?? "",
                LogLevel    = logLevel,
                Content     = content,
                CreateAt    = DateTime.Now
            };

            await RunLogCache.AddQueueAsync(runLogPO);
        }

        /// <summary>
        /// 将日志写入ES或数据库
        /// </summary>
        public async Task<int> AddToDbAsync(int top)
        {
            var lstLog = await RunLogCache.GetQueueAsync(top);
            if (lstLog.Count == 0) return 0;

            if (UseEs)
            {
                await LogContext.Data.RunLog.InsertAsync(lstLog);
            }
            else
            {
                await MetaInfoContext.Data.RunLog.InsertAsync(lstLog);
            }
            
            return lstLog.Count;
        }
    }
}