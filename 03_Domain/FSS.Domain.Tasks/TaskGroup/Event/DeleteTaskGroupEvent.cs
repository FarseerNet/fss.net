using System.Threading.Tasks;
using FS.EventBus;
using FS.EventBus.Attr;
using FS.Extends;
using FSS.Infrastructure.Repository.TaskGroup.Interface;
using FSS.Infrastructure.Repository.Tasks.Interface;

namespace FSS.Domain.Tasks.TaskGroup.Event
{
    /// <summary>
    /// 删除任务组
    /// </summary>
    [Consumer(EventName = "DeleteTaskGroup")]
    public class DeleteTaskGroupEvent : IListenerMessage
    {
        public ITaskGroupAgent TaskGroupAgent { get; set; }
        public ITaskAgent      TaskAgent      { get; set; }

        public async Task<bool> Consumer(object message, DomainEventArgs ea)
        {
            var taskGroupId = message.ConvertType(0);
            await TaskAgent.DeleteAsync(taskGroupId);
            await TaskGroupAgent.DeleteAsync(taskGroupId);
            return true;
        }
    }
}