using System.Threading.Tasks;
using FS.Core;
using FS.Extends;
using FSS.Domain.Log.TaskLog;
using FSS.Domain.Log.TaskLog.Repository;
using FSS.Infrastructure.Repository.Log;
using FSS.Infrastructure.Repository.Log.Model;
using Microsoft.Extensions.Logging;

namespace FSS.Infrastructure.Repository;

public class TaskLogRepository : ITaskLogRepository
{
    public LogAgent LogAgent { get; set; }
    public LogQueue LogQueue { get; set; }

    public PageList<TaskLogDO> GetList(string jobName, LogLevel? logLevel, int pageSize, int pageIndex) => LogAgent.GetList(jobName: jobName, logLevel: logLevel, pageSize: pageSize, pageIndex: pageIndex).Map<TaskLogDO>();

    public Task AddAsync(TaskLogDO taskLogDO) => LogQueue.AddQueueAsync(log: taskLogDO.Map<TaskLogPO>());
}