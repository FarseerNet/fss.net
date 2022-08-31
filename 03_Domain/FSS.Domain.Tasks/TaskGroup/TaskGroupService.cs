using FS.DI;
using FSS.Domain.Tasks.TaskGroup.Entity;
using FSS.Domain.Tasks.TaskGroup.Repository;

namespace FSS.Domain.Tasks.TaskGroup;

public class TaskGroupService : ISingletonDependency
{
    public ITaskGroupRepository TaskGroupRepository { get; set; }

    /// <summary>
    ///     计算平均耗时
    /// </summary>
    public async Task UpdateAvgSpeed(int taskGroupId)
    {
        var speedList   = await TaskGroupRepository.ToTaskSpeedListAsync(taskGroupId: taskGroupId);
        var runSpeedAvg = new TaskSpeed(speedList).GetAvgSpeed();

        if (runSpeedAvg > 0)
        {
            var taskGroup = await TaskGroupRepository.ToEntityAsync(taskGroupId);
            taskGroup.RunSpeedAvg = runSpeedAvg;
            TaskGroupRepository.Save(taskGroup);
        }
    }
}