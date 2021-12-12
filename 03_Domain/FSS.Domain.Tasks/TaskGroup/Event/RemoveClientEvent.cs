using System.Threading.Tasks;
using FS.EventBus;
using FS.EventBus.Attr;
using FS.Extends;
using FSS.Domain.Tasks.TaskGroup.Enum;
using FSS.Domain.Tasks.TaskGroup.Repository;

namespace FSS.Domain.Tasks.TaskGroup.Event
{
    /// <summary>
    /// 客户端下线后移除任务
    /// </summary>
    [Consumer(EventName = "ClientOffline")]
    public class RemoveClientEvent : IListenerMessage
    {
        public ITaskGroupRepository TaskGroupRepository { get; set; }

        public async Task<bool> Consumer(object message, DomainEventArgs ea)
        {
            var clientId = message.ConvertType(0L);

            // 读取当前所有任务组的任务
            var lstTaskGroup    = await TaskGroupRepository.ToListAsync();
            var cancelTaskGroup = lstTaskGroup.FindAll(o => o.Task != null && o.Task.ClientId == clientId && o.Task.Status is EumTaskType.Scheduler or EumTaskType.Working);
            foreach (var taskGroupPO in cancelTaskGroup)
            {
                await taskGroupPO.CancelTask();
            }
            return true;
        }
    }
}