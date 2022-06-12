using FS.Core.DomainDriven.DomainEvent;

namespace FSS.Domain.Tasks.TaskGroup.Publish;

/// <summary>
/// 删除任务组事件
/// </summary>
public class DeleteTaskGroupEvent : BaseDomainEvent
{
    protected override string EventName => "DeleteTaskGroup";

    public DeleteTaskGroupEvent(int taskGroupId)
    {
        TaskGroupId = taskGroupId;
    }

    public int TaskGroupId { get; set; }
}