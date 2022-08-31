using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Collections.Pooled;
using FS.Core.Abstract.AspNetCore;
using FS.DI;
using FS.Extends;
using FSS.Application.Clients.Client;
using FSS.Application.Clients.Client.Entity;
using FSS.Application.Tasks.TaskGroup.Entity;
using FSS.Domain.Client.Clients.Repository;
using FSS.Domain.Tasks.TaskGroup.Entity;
using FSS.Domain.Tasks.TaskGroup.Repository;
using Microsoft.AspNetCore.Http;

namespace FSS.Application.Tasks.TaskGroup;

[UseApi(Area = "task")]
public class TaskSchedulerApp : ISingletonDependency
{
    public ITaskGroupRepository TaskGroupRepository { get; set; }
    public ClientApp            ClientApp           { get; set; }
 
    /// <summary>
    ///     任务调度
    /// </summary>
    [Api("Pull")]
    public Task<PooledList<TaskDTO>> PullAsync(PullDTO dto)
    {
        var client = ClientApp.GetClient();
        if (dto.TaskCount == 0) dto.TaskCount = 3;
        return TaskGroupRepository.GetCanSchedulerTaskGroup(jobsName: client.Jobs, ts: TimeSpan.FromSeconds(value: 15), count: dto.TaskCount, client: client).MapAsync<TaskDTO, TaskDO>();
    }
}