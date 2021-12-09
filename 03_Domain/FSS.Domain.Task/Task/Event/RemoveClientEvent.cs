using System.Threading.Tasks;
using FS.EventBus;
using FS.EventBus.Attr;
using FS.Extends;

namespace FSS.Domain.Task.Task.Event
{
    /// <summary>
    /// 客户端下线后移除任务
    /// </summary>
    [Consumer(EventName = "ClientOffline")]
    public class RemoveClientEvent : IListenerMessage
    {
        public async Task<bool> Consumer(string message, object sender, DomainEventArgs ea)
        {
            var clientId = message.ConvertType(0L);
            
            // 读取当前所有任务组的任务
            var groupListAsync = await TaskList.ToGroupListAsync();
            var findAll        = groupListAsync.FindAll(o => o.ClientId == clientId && o.Status is EumTaskType.Scheduler or EumTaskType.Working); // or EumTaskType.ReScheduler
            foreach (var vo in findAll)
            {
                var taskGroup = await TaskGroupInfo.ToInfoAsync(vo.TaskGroupId);
                vo.Status = EumTaskType.Fail;
                await TaskUpdate.SaveFinishAsync(vo, taskGroup);
            }
            return true;
        }
    }
}