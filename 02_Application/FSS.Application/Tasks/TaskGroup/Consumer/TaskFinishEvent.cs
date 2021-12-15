using System.Threading.Tasks;
using FS.EventBus;
using FS.EventBus.Attr;
using FSS.Domain.Tasks.TaskGroup.Entity;
using FSS.Domain.Tasks.TaskGroup.Interface;

namespace FSS.Application.Tasks.TaskGroup.Consumer
{
    /// <summary>
    /// 任务完成事件
    /// </summary>
    [Consumer(EventName = "TaskFinish")]
    public class TaskFinishEvent : IListenerMessage
    {
        public ITaskGroupService TaskGroupService { get; set; }

        public async Task<bool> Consumer(object taskGroupDO, DomainEventArgs ea)
        {
            var taskGroup = (TaskGroupDO)taskGroupDO;
            
            // 将当前的Task写入Redis队列
            if (taskGroup.Task != null) await taskGroup.Task.AddQueueAsync();
            
            // 完成后，立即生成一个新的任务
            if (taskGroup.IsEnable)
            {
                // 创建任务
                await taskGroup.CreateTask();
            }
            
            return true;
        }
    }
}