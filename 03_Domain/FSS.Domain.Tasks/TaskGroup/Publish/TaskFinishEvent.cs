using FS.Core.DomainDriven.DomainEvent;
using FSS.Domain.Tasks.TaskGroup.Entity;

namespace FSS.Domain.Tasks.TaskGroup.Publish;

/// <summary>
/// 任务完成事件
/// </summary>
public class TaskFinishEvent : BaseDomainEvent
{
    protected override string EventName => "TaskFinish";
    
    public TaskFinishEvent(TaskDO task)
    {
        Task = task;
    }

    public TaskDO Task { get; set; }
    
}