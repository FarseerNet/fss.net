using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FS.Core;
using FS.DI;
using FS.Extends;
using FSS.Abstract.Entity.MetaInfo;
using FSS.Infrastructure.Repository;
using Newtonsoft.Json;

namespace FSS.Com.MetaInfoServer.RunLog.Dal
{
    /// <summary>
    /// 日志
    /// </summary>
    // ReSharper disable once UnusedType.Global
    public class RunLogCache : ISingletonDependency
    {
        private const string RunLogQueueKey = "FSS_RunLogQueue";

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