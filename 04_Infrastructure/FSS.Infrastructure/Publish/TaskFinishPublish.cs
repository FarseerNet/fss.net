using FS.DI;
using FS.EventBus;
using FSS.Domain.Tasks.TaskGroup;
using FSS.Domain.Tasks.TaskGroup.Entity;
using FSS.Domain.Tasks.TaskGroup.Publish;

namespace FSS.Infrastructure.Publish;

public class TaskFinishPublish : ITaskFinishPublish
{
    /// <summary>
    ///     发布任务组完成事件
    /// </summary>
    public void Publish(object sender, TaskGroupDO taskGroup)
    {
        IocManager.GetService<IEventProduct>(name: "TaskFinish").SendSync(sender: sender, message: taskGroup);
    }
}