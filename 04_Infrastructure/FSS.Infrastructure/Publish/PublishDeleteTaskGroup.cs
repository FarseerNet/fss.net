using FS.DI;
using FS.EventBus;
using FSS.Domain.Tasks.TaskGroup.Publish;

namespace FSS.Infrastructure.Publish;

public class PublishDeleteTaskGroup : IPublishDeleteTaskGroup
{
    /// <summary>
    ///     发布删除任务组事件
    /// </summary>
    public void Publish(object sender, int taskGroupId)
    {
        IocManager.GetService<IEventProduct>(name: "DeleteTaskGroup").SendSync(sender: sender, message: taskGroupId);
    }
}