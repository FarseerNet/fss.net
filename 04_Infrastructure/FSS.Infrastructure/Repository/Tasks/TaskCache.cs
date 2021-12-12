using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FS.Core;
using FS.DI;
using FS.Extends;
using FSS.Infrastructure.Repository.Tasks.Model;
using Newtonsoft.Json;

namespace FSS.Infrastructure.Repository.Tasks
{
    public class TaskCache: ISingletonDependency
    {
        private const string FinishTaskQueueKey = "FSS_FinishTaskQueue";

        /// <summary>
        /// 将Task写入队列中
        /// </summary>
        public Task AddQueueAsync(TaskPO task)
        {
            return RedisContext.Instance.Db.SortedSetAddAsync(FinishTaskQueueKey, JsonConvert.SerializeObject(task), DateTime.Now.ToTimestamps());
        }

        /// <summary>
        /// 队列中取出已完成的任务
        /// </summary>
        public async Task<List<TaskPO>> GetFinishTaskListAsync(int top)
        {
            var sortedSetEntries = await RedisContext.Instance.Db.SortedSetPopAsync(FinishTaskQueueKey, top);
            return sortedSetEntries.Select(s => Jsons.ToObject<TaskPO>(s.Element)).ToList();
        }
    }
}