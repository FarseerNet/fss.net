using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FS.Core;
using FS.DI;
using FS.Extends;
using FSS.Infrastructure.Repository.Log.Model;
using Newtonsoft.Json;

namespace FSS.Infrastructure.Repository.Log;

public class LogQueue : ISingletonDependency
{
    private const string RunLogQueueKey = "FSS_LogQueue";

    /// <summary>
    ///     将日志写入队列中
    /// </summary>
    public Task AddQueueAsync(TaskLogPO log) => RedisContext.Instance.Db.SortedSetAddAsync(key: RunLogQueueKey, member: JsonConvert.SerializeObject(value: log), score: DateTime.Now.ToTimestamps());

    /// <summary>
    ///     队列中取出已完成的任务
    /// </summary>
    public async Task<List<TaskLogPO>> GetQueueAsync(int top)
    {
        var sortedSetEntries = await RedisContext.Instance.Db.SortedSetPopAsync(key: RunLogQueueKey, count: top);
        return sortedSetEntries.Select(selector: s => Jsons.ToObject<TaskLogPO>(obj: s.Element)).ToList();
    }
}