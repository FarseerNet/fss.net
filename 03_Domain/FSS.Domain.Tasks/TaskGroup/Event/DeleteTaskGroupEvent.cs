using System.Threading.Tasks;
using FS.EventBus;
using FS.EventBus.Attr;
using FS.Extends;
using FSS.Domain.Tasks.TaskGroup.Repository;

namespace FSS.Domain.Tasks.TaskGroup.Event
{
    /// <summary>
    /// 删除任务组
    /// </summary>
    [Consumer(EventName = "DeleteTaskGroup")]
    public class DeleteTaskGroupEvent : IListenerMessage
    {
        public ITaskGroupRepository TaskGroupRepository { get; set; }
        
        public async Task<bool> Consumer(object message, DomainEventArgs ea)
        {
            var taskGroupId = message.ConvertType(0);
            return true;
        }
    }
}