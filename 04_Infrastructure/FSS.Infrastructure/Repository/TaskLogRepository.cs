using FS.Core;
using FS.Core.Abstract.Data;
using FS.Core.Abstract.MQ.Queue;
using FS.DI;
using FS.Extends;
using FS.MQ.Queue;
using FSS.Domain.Log.TaskLog;
using FSS.Domain.Log.TaskLog.Repository;
using FSS.Infrastructure.Repository.Log;
using FSS.Infrastructure.Repository.Log.Model;
using Mapster;
using Microsoft.Extensions.Logging;

namespace FSS.Infrastructure.Repository;

public class TaskLogRepository : ITaskLogRepository
{
    public LogAgent LogAgent { get; set; }

    public PageList<TaskLogDO> GetList(string jobName, LogLevel? logLevel, int pageSize, int pageIndex) => LogAgent.GetList(jobName, logLevel, pageSize, pageIndex).Map<TaskLogDO>();

    public void Add(TaskLogDO taskLogDO) => IocManager.GetService<IQueueProduct>(name: "TaskLogQueue").Send(taskLogDO.Map<TaskLogPO>());
}