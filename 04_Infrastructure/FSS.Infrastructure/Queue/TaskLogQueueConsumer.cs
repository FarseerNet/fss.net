using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Collections.Pooled;
using FS.Core.Abstract.MQ.Queue;
using FSS.Infrastructure.Repository.Log;
using FSS.Infrastructure.Repository.Log.Model;

namespace FSS.Infrastructure.Queue
{
    /// <summary>
    ///     添加任务日志
    /// </summary>
    [Consumer(Enable = true, Name = "TaskLogQueue", PullCount = 1000, SleepTime = 500)]
    public class TaskLogQueueConsumer : IListenerMessage
    {
        public LogAgent LogAgent { get; set; }
        
        public async Task<bool> Consumer(IEnumerable<object> queueList)
        {
            using var lst = queueList.Select(o => (TaskLogPO)o).ToPooledList();
            await LogAgent.AddAsync(lst);
            
            return true;
        }

        public Task<bool> FailureHandling(IEnumerable<object> messages) => Task.FromResult(false);
    }
}