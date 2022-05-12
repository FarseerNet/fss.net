using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FS.MQ.Queue;
using FS.MQ.Queue.Attr;
using FSS.Infrastructure.Repository.Log;
using FSS.Infrastructure.Repository.Log.Model;

namespace FSS.Infrastructure.Consumer
{
    /// <summary>
    ///     添加任务日志
    /// </summary>
    [Consumer(Enable = true, Name = "TaskLogQueue", PullCount = 1000, SleepTime = 500)]
    public class TaskLogQueueConsumer : IListenerMessage
    {
        public LogAgent LogAgent { get; set; }
        
        public async Task<bool> Consumer(List<object> queueList)
        {
            var lst = queueList.Select(o => (TaskLogPO)o).ToList();
            await LogAgent.AddAsync(lst);
            
            return true;
        }

        public Task<bool> FailureHandling(List<object> messages) => Task.FromResult(false);
    }
}