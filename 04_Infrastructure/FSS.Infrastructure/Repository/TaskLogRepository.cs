using FS.Core;
using FS.DI;
using FS.Extends;
using FS.MQ.Queue;
using FSS.Domain.Log.TaskLog;
using FSS.Domain.Log.TaskLog.Repository;
using FSS.Infrastructure.Repository.Log;
using FSS.Infrastructure.Repository.Log.Model;
using Microsoft.Extensions.Logging;

namespace FSS.Infrastructure.Repository;

public class TaskLogRepository : ITaskLogRepository
{
    public   LogAgent      LogAgent { get; set; }

    public PageList<TaskLogDO> GetList(string jobName, LogLevel? logLevel, int pageSize, int pageIndex) => LogAgent.GetList(jobName: jobName, logLevel: logLevel, pageSize: pageSize, pageIndex: pageIndex).Map<TaskLogDO>();

    public void Add(TaskLogDO taskLogDO) => IocManager.GetService<IQueueManager>(name: "TaskLogQueue").Product.Send(taskLogDO.Map<TaskLogPO>());
}