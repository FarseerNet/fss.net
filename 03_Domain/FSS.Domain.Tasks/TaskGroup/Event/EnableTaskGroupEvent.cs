using System.Threading.Tasks;
using FS.EventBus;
using FS.EventBus.Attr;
using FSS.Domain.Tasks.TaskGroup.Entity;
using FSS.Domain.Tasks.TaskGroup.Interface;

namespace FSS.Domain.Tasks.TaskGroup.Event
{
    /// <summary>
    /// 任务组开启
    /// </summary>
    [Consumer(EventName = "EnableTaskGroup")]
    public class EnableTaskGroupEvent : IListenerMessage
    {
        public ITaskGroupService TaskGroupService { get; set; }

        public async Task<bool> Consumer(object taskGroupDO, DomainEventArgs ea)
        {
            // 创建任务
            await ((TaskGroupDO)taskGroupDO).CreateTask();
            return true;
        }
    }
}