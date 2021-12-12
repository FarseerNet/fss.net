using System.Collections.Generic;
using System.Threading.Tasks;
using FSS.Domain.Log.TaskLog.Entity;
using Microsoft.Extensions.Logging;

namespace FSS.Domain.Log.TaskLog.Repository
{
    public interface ITaskLogRepository
    {
        /// <summary>
        /// 获取日志
        /// </summary>
        List<TaskLogDO> GetList(string jobName, LogLevel? logLevel, int pageSize, int pageIndex, out long totalCount);
        /// <summary>
        /// 添加日志
        /// </summary>
        Task AddAsync(TaskLogDO taskLogDO);
    }
}