using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FS.DI;
using FS.Extends;
using FSS.Application.Clients.Client.Entity;
using FSS.Application.Tasks.TaskGroup.Entity;
using FSS.Domain.Tasks.TaskGroup.Entity;
using FSS.Domain.Tasks.TaskGroup.Repository;

namespace FSS.Application.Tasks.TaskGroup;

public class TaskSchedulerApp : ISingletonDependency
{
    public ITaskGroupRepository TaskGroupRepository { get; set; }

    /// <summary>
    ///     任务调度
    /// </summary>
    public Task<List<TaskDTO>> TaskSchedulerAsync(ClientDTO client, int requestTaskCount)
    {
        if (requestTaskCount == 0) requestTaskCount = 3;

        return TaskGroupRepository.GetCanSchedulerTaskGroup(jobsName: client.Jobs, ts: TimeSpan.FromSeconds(value: 15), count: requestTaskCount, client).MapAsync<TaskDTO, TaskDO>();
    }
}