using System.Threading.Tasks;
using FS.Core;
using FS.Extends;
using FSS.Abstract.Entity.MetaInfo;
using FSS.Application.Log.Interface;
using FSS.Domain.Log.TaskLog.Entity;
using FSS.Domain.Log.TaskLog.Interface;
using Microsoft.Extensions.Logging;

namespace FSS.Application.Log
{
    public class LogAddApp : ILogAddApp
    {
        public ITaskLogService TaskLogService { get; set; }

        /// <summary>
        /// 添加日志记录
        /// </summary>
        public Task AddAsync(int taskGroupId, string jobName, string caption, LogLevel logLevel, string content)
        {
            return TaskLogService.AddAsync(taskGroupId, jobName, caption, logLevel, content);
        }

        /// <summary>
        /// 获取日志
        /// </summary>
        public DataSplitList<RunLogDO> GetList(string jobName, LogLevel? logLevel, int pageSize, int pageIndex)
        {
            return TaskLogService.GetList(jobName, logLevel, pageSize, pageIndex);
        }
    }
}