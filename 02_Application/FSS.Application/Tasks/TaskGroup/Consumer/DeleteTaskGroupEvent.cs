using System.Threading.Tasks;
using FS.EventBus;
using FS.EventBus.Attr;
using FS.Extends;
using FSS.Domain.Tasks.TaskGroup.Repository;

namespace FSS.Application.Tasks.TaskGroup.Consumer;

/// <summary>
///     删除任务组
/// </summary>
[Consumer(EventName = "DeleteTaskGroup")]
public class DeleteTaskGroupEvent : IListenerMessage
{
    public ITaskGroupRepository TaskGroupRepository { get; set; }

    public Task<bool> Consumer(object message, DomainEventArgs ea)
    {
        var taskGroupId = message.ConvertType(defValue: 0);
        return Task.FromResult(result: true);
    }
}