using FS.DI;
using FSS.Domain.Tasks.TaskGroup.Entity;
using FSS.Domain.Tasks.TaskGroup.Repository;

namespace FSS.Domain.Tasks.TaskGroup;

public class TaskGroupService : ISingletonDependency
{
    public ITaskGroupRepository TaskGroupRepository { get; set; }

    /// <summary>
    ///     获取所有任务组中的任务
    /// </summary>
    public async Task<List<TaskGroupDO>> ToListAsync()
    {
        var lstTaskGroup = await TaskGroupRepository.ToListAsync();

        foreach (var taskGroupPO in lstTaskGroup)
            if (taskGroupPO.Task == null)
                await taskGroupPO.CreateTask();
        return lstTaskGroup;
    }
    

    /// <summary>
    ///     计算平均耗时
    /// </summary>
    public async Task UpdateAvgSpeed(int taskGroupId)
    {
        var speedList   = await IocManager.GetService<ITaskGroupRepository>().ToTaskSpeedListAsync(taskGroupId: taskGroupId);
        var runSpeedAvg = new TaskSpeed(speedList: speedList).GetAvgSpeed();
        
        if (runSpeedAvg > 0)
        {
            var taskGroup = await TaskGroupRepository.ToEntityAsync(taskGroupId);
            taskGroup.RunSpeedAvg = runSpeedAvg;
            await IocManager.GetService<ITaskGroupRepository>().SaveAsync(taskGroup);
        }
    }
}