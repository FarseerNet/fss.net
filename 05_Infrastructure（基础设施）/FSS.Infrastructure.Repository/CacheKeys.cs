using System.Threading.Tasks;
using FS.Cache;

namespace FSS.Infrastructure.Repository
{
    public class CacheKeys
    {
        /// <summary> 任务组缓存 </summary>
        public static CacheKey TaskGroupKey(EumCacheStoreType cacheStoreType) => new($"FSS_TaskGroup", cacheStoreType);
        public static Task TaskGroupClear(int taskGroupId) => RedisContext.Instance.CacheManager.RemoveAsync(TaskGroupKey(EumCacheStoreType.MemoryAndRedis), taskGroupId);


        /// <summary> 任务缓存 </summary>
        public static CacheKey TaskForGroupKey => new($"FSS_TaskForGroup", EumCacheStoreType.Redis);
        public static Task TaskForGroupClear(int taskGroupId) => RedisContext.Instance.CacheManager.RemoveAsync(TaskForGroupKey, taskGroupId);
        public static Task TaskForGroupClear() => RedisContext.Instance.CacheManager.RemoveAsync(TaskForGroupKey);
    }
}