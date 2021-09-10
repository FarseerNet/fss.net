using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FS.Cache;
using FS.Cache.Redis;
using FS.DI;
using FS.Extends;
using FSS.Abstract.Entity.MetaInfo;
using FSS.Abstract.Server.MetaInfo;
using FSS.Com.MetaInfoServer.Abstract;
using FSS.Com.MetaInfoServer.TaskGroup.Dal;
using Microsoft.Extensions.Caching.Memory;
using StackExchange.Redis;

namespace FSS.Com.MetaInfoServer.TaskGroup
{
    /// <summary>
    /// 任务列表
    /// </summary>
    public class TaskGroupList : ITaskGroupList
    {
        public  ITaskGroupAgent    TaskGroupAgent    { get; set; }
        private IRedisCacheManager RedisCacheManager => IocManager.Instance.Resolve<IRedisCacheManager>();

        readonly MemoryCache memoryCache = new(new MemoryCacheOptions
        {
        });

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
        /// 获取全部任务列表（数据库）
        /// </summary>
        public Task<List<TaskGroupVO>> ToListByDbAsync()
        {
            return TaskGroupAgent.ToListAsync().MapAsync<TaskGroupVO, TaskGroupPO>();
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
        /// 本地缓存获取任务组
        /// </summary>
        public Task<Dictionary<int, TaskGroupVO>> ToListByMemoryAsync()
        {
            return memoryCache.GetOrCreate(TaskGroupCache.Key, async o =>
            {
                o.AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(1);
                var lst= await ToListAsync();
                return lst.ToDictionary(o => o.Id, o => o);
            });
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