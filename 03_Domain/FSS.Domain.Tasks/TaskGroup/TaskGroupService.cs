using FS.DI;
using FSS.Domain.Tasks.TaskGroup.Entity;
using FSS.Domain.Tasks.TaskGroup.Repository;

namespace FSS.Domain.Tasks.TaskGroup;

public class TaskGroupService : ISingletonDependency
{
    public ITaskGroupRepository TaskGroupRepository { get; set; }

    /// <summary>
    ///     获取任务组信息
    /// </summary>
    public Task<TaskGroupDO> ToEntityAsync(int taskGroupId) => TaskGroupRepository.ToEntityAsync(taskGroupId: taskGroupId);

    /// <summary>
    ///     删除任务组
    /// </summary>
    public async Task DeleteAsync(int taskGroupId)
    {
        var taskGroup = await TaskGroupRepository.ToEntityAsync(taskGroupId: taskGroupId);
        await taskGroup.DeleteAsync();
    }

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
}