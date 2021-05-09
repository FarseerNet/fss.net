using System;
using System.Threading.Tasks;
using FS.DI;
using FSS.Abstract.Server.MetaInfo;
using FSS.Com.MetaInfoServer.Abstract;
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
        public IRunLogAgent RunLogAgent { get; set; }
        public IIocManager  IocManager  { get; set; }

        /// <summary>
        /// 添加日志记录
        /// </summary>
        public async Task AddAsync(int taskGroupId, int taskId, LogLevel logLevel, string content)
        {
            if (logLevel is LogLevel.Error or LogLevel.Warning) IocManager.Logger<RunLogAdd>().Log(logLevel, content);
            var runLogPO = new RunLogPO
            {
                TaskGroupId = taskGroupId,
                TaskId      = taskId,
                LogLevel    = logLevel,
                Content     = content,
                CreateAt    = DateTime.Now
            };
            var configurationSection = IocManager.Resolve<IConfigurationRoot>().GetSection("ElasticSearch:[0]:Server");
            if (!string.IsNullOrWhiteSpace(configurationSection.Value)) await LogContext.Data.RunLog.InsertAsync(runLogPO);
            else await RunLogAgent.AddAsync(runLogPO);
        }
    }
}