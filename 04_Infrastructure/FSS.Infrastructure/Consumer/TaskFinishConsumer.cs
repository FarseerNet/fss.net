using System.Threading.Tasks;
using FS.Core.Abstract.EventBus;
using FSS.Domain.Tasks.TaskGroup.Entity;
using FSS.Domain.Tasks.TaskGroup.Repository;

namespace FSS.Infrastructure.Consumer;

/// <summary>
///     删除任务组
/// </summary>
[FS.EventBus.Attr.Consumer(EventName = "TaskFinish")]
public class TaskFinishConsumer : IListenerMessage
{
    public ITaskGroupRepository TaskGroupRepository { get; set; }

    public Task<bool> Consumer(object message, DomainEventArgs ea)
    {
        var task = (TaskDO)message;
        TaskGroupRepository.AddTask(task);
        return Task.FromResult(true);
    }
}