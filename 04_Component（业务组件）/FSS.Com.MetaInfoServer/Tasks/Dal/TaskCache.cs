using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FS.Core;
using FS.DI;
using FS.Extends;
using FSS.Abstract.Entity.MetaInfo;
using FSS.Infrastructure.Repository;
using Nest;
using Newtonsoft.Json;

namespace FSS.Com.MetaInfoServer.Tasks.Dal
{
    /// <summary>
    /// 任务缓存
    /// </summary>
    // ReSharper disable once UnusedType.Global
    public class TaskCache : ISingletonDependency
    {
        private const string FinishTaskQueueKey = "FSS_FinishTaskQueue";

        /// <summary>
        /// 保存任务信息
        /// </summary>
        public Task SaveAsync(TaskVO task)
        {
            var key = CacheKeys.TaskForGroupKey;
            return RedisContext.Instance.CacheManager.SaveItemAsync(key, task);
        }

        /// <summary>
        /// 队列中取出已完成的任务
        /// </summary>
        public async Task<List<TaskVO>> GetFinishTaskListAsync(int top)
        {
            var sortedSetEntries = await RedisContext.Instance.Db.SortedSetPopAsync(FinishTaskQueueKey,top);
            return sortedSetEntries.Select(s => Jsons.ToObject<TaskVO>(s.Element)).ToList();
        }
    }
}