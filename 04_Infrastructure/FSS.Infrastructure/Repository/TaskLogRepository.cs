using System.Collections.Generic;
using System.Threading.Tasks;
using FS.Extends;
using FSS.Domain.Log.TaskLog.Entity;
using FSS.Domain.Log.TaskLog.Repository;
using FSS.Infrastructure.Repository.Log;
using FSS.Infrastructure.Repository.Log.Model;
using Microsoft.Extensions.Logging;

namespace FSS.Infrastructure.Repository
{
    public class TaskLogRepository : ITaskLogRepository
    {
        public LogAgent LogAgent { get; set; }
        public LogQueue LogQueue { get; set; }

        public List<TaskLogDO> GetList(string jobName, LogLevel? logLevel, int pageSize, int pageIndex, out long totalCount) => LogAgent.GetList(jobName, logLevel, pageSize, pageIndex, out totalCount).Map<TaskLogDO>();

        public Task AddAsync(TaskLogDO taskLogDO) => LogQueue.AddQueueAsync(taskLogDO.Map<TaskLogPO>());
    }
}