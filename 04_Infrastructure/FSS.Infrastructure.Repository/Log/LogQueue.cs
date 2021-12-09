using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FS.Core;
using FS.Extends;
using FSS.Infrastructure.Repository.Log.Interface;
using FSS.Infrastructure.Repository.Log.Model;
using Newtonsoft.Json;

namespace FSS.Infrastructure.Repository.Log
{
    public class LogQueue : ILogQueue
    {
        private const string RunLogQueueKey = "FSS_LogQueue";

        /// <summary>
        /// 将日志写入队列中
        /// </summary>
        public Task AddQueueAsync(RunLogPO log)
        {
            return RedisContext.Instance.Db.SortedSetAddAsync(RunLogQueueKey, JsonConvert.SerializeObject(log),DateTime.Now.ToTimestamps());
        }

        /// <summary>
        /// 队列中取出已完成的任务
        /// </summary>
        public async Task<List<RunLogPO>> GetQueueAsync(int top)
        {
            var sortedSetEntries = await RedisContext.Instance.Db.SortedSetPopAsync(RunLogQueueKey,top);
            return sortedSetEntries.Select(s => Jsons.ToObject<RunLogPO>(s.Element)).ToList();
        }
    }
}