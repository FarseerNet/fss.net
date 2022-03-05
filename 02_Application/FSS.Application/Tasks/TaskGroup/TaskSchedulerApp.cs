using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FS.DI;
using FSS.Application.Clients.Client.Entity;
using FSS.Application.Tasks.TaskGroup.Entity;
using FSS.Domain.Tasks.TaskGroup.Repository;

namespace FSS.Application.Tasks.TaskGroup;

public class TaskSchedulerApp : ISingletonDependency
{
    public ITaskGroupRepository TaskGroupRepository { get; set; }

    /// <summary>
    ///     任务调度
    /// </summary>
    public async Task<List<TaskDTO>> TaskSchedulerAsync(ClientDTO client, int requestTaskCount)
    {
        if (requestTaskCount == 0) requestTaskCount = 3;

        var lst = new List<TaskDTO>();

        var lstTaskGroup = await TaskGroupRepository.GetCanSchedulerTaskGroup(jobsName: client.Jobs, ts: TimeSpan.FromSeconds(value: 15), count: requestTaskCount);
        foreach (var taskGroupDO in lstTaskGroup)
        {
            // 设为调度状态
            await taskGroupDO.SchedulerAsync(client: client);
            
            // 如果不相等，说明被其它客户端拿了
            if (taskGroupDO.Task.Client.ClientId == client.Id) lst.Add(item: taskGroupDO.Task);
        }
        return lst;
    }
}