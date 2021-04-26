using System;
using FS.DI;
using FSS.Abstract.Server.MetaInfo;
using FSS.Com.MetaInfoServer.Abstract;
using FSS.Com.MetaInfoServer.RunLog.Dal;
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
        public void Add(int taskGroupId, int taskId, LogLevel logLevel, string content)
        {
            if (logLevel == LogLevel.Error) IocManager.Logger<RunLogAdd>().LogWarning($"任务组：{taskGroupId} 任务ID：{taskId}，发生错误：{content}");
            RunLogAgent.Add(new RunLogPO
            {
                TaskGroupId = taskGroupId,
                TaskId      = taskId,
                LogLevel    = logLevel,
                Content     = content,
                CreateAt    = DateTime.Now
            });
        }
    }
}