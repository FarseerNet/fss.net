using System;
using System.Threading.Tasks;
using FS.DI;
using FSS.Domain.Log.TaskLog.Entity;
using Microsoft.Extensions.Logging;

namespace FSS.Domain.Log.TaskLog
{
    public class TaskLogService : ISingletonDependency
    {
        /// <summary>
        /// 添加日志记录
        /// </summary>
        public Task AddAsync(int taskGroupId, string jobName, string caption, LogLevel logLevel, string content)
        {
            return new TaskLogDO
            {
                TaskGroupId = taskGroupId,
                Caption     = caption ?? "",
                JobName     = jobName ?? "",
                LogLevel    = logLevel,
                Content     = content,
                CreateAt    = DateTime.Now
            }.AddAsync();
        }
    }
}