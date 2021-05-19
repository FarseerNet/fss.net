using System.Collections.Generic;
using System.Threading.Tasks;
using FS.Cache;
using FS.Cache.Redis;
using FS.Extends;
using FSS.Abstract.Entity.MetaInfo;
using FSS.Abstract.Server.MetaInfo;
using FSS.Com.MetaInfoServer.Abstract;
using FSS.Com.MetaInfoServer.TaskGroup.Dal;
using StackExchange.Redis;

namespace FSS.Com.MetaInfoServer.TaskGroup
{
    /// <summary>
    /// 任务列表
    /// </summary>
    public class TaskGroupList : ITaskGroupList
    {
        public ITaskGroupAgent    TaskGroupAgent    { get; set; }
        public IRedisCacheManager RedisCacheManager { get; set; }

        /// <summary>
        /// 获取全部任务列表
        /// </summary>
        public async Task<List<TaskGroupVO>> ToListAndSaveAsync()
        {
            var taskGroupVos = await TaskGroupAgent.ToListAsync().MapAsync<TaskGroupVO, TaskGroupPO>();
            await RedisCacheManager.CacheManager.SaveAsync(TaskGroupCache.Key, taskGroupVos, o => o.Id);
            return taskGroupVos;
        }

        /// <summary>
        /// 获取全部任务列表
        /// </summary>
        public Task<List<TaskGroupVO>> ToListAsync()
        {
            return RedisCacheManager.CacheManager.GetListAsync(TaskGroupCache.Key,
                _ => TaskGroupAgent.ToListAsync().MapAsync<TaskGroupVO, TaskGroupPO>()
                , o => o.Id);
        }

        /// <summary>
        /// 获取任务组数量
        /// </summary>
        public Task<long> Count()
        {
            return RedisCacheManager.Db.HashLengthAsync(TaskGroupCache.Key);
        }

        /// <summary>
        /// 删除整个缓存
        /// </summary>
        public Task ClearAsync() => RedisCacheManager.CacheManager.RemoveAsync(TaskGroupCache.Key);
    }
}