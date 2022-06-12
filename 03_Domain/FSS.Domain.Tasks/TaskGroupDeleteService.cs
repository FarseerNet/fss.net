using FS.DI;
using FSS.Domain.Tasks.TaskGroup.Event;
using FSS.Domain.Tasks.TaskGroup.Repository;

namespace FSS.Domain.Tasks;

/// <summary>
///     删除任务组
/// </summary>
public class TaskGroupDeleteService : ISingletonDependency
{
    public ITaskGroupRepository TaskGroupRepository { get; set; }
    
    /// <summary>
    ///     删除任务组
    /// </summary>
    public async Task DeleteAsync(int taskGroupId)
    {
        var taskGroup = await TaskGroupRepository.ToEntityAsync(taskGroupId: taskGroupId);
        if (taskGroup == null) throw new Exception(message: "要删除的任务组不存在");
        
        // 如果任务组是开启状态，则需要先暂定任务组
        if (taskGroup.IsEnable)
        {
            taskGroup.Disable();
            TaskGroupRepository.Save(taskGroup);
        }

        await TaskGroupRepository.DeleteAsync(taskGroupId: taskGroup.Id);

        // 发布删除任务组事件
        new DeleteTaskGroupEvent(taskGroup.Id).PublishEvent();
    }
}