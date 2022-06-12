using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FS.Core.Abstract.MQ.Queue;
using FSS.Infrastructure.Repository.Tasks;
using FSS.Infrastructure.Repository.Tasks.Model;

namespace FSS.Infrastructure.Consumer
{
    /// <summary>
    ///     任务写入数据库
    /// </summary>
    [Consumer(Enable = true, Name = "TaskQueue", PullCount = 1000, SleepTime = 500)]
    public class TaskQueueConsumer : IListenerMessage
    {
        public TaskAgent TaskAgent { get; set; }
        
        public async Task<bool> Consumer(IEnumerable<object> queueList)
        {
            var lst    = queueList.Select(o => (TaskPO)o).ToList();
            await TaskAgent.AddToDbAsync(lst);
            return true;
        }

        public Task<bool> FailureHandling(IEnumerable<object> messages) => Task.FromResult(false);
    }
}