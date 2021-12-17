using System.Threading.Tasks;
using FS.Cache;
using FSS.Domain.Tasks.TaskGroup.Entity;
using FSS.Infrastructure.Repository.Client.Model;
using FSS.Infrastructure.Repository.TaskGroup.Model;
using FSS.Infrastructure.Repository.Tasks.Model;

namespace FSS.Infrastructure.Repository
{
    public class CacheKeys
    {
        /// <summary> 任务组缓存 </summary>
        public static CacheKey<TaskGroupDO, int> TaskGroupKey(EumCacheStoreType cacheStoreType) => new($"FSS_TaskGroup", o => o.Id, cacheStoreType);
        public static Task TaskGroupClear(int taskGroupId) => RedisContext.Instance.CacheManager.RemoveItemAsync(TaskGroupKey(EumCacheStoreType.MemoryAndRedis), taskGroupId);

        /// <summary> 客户端缓存 </summary>
        public static CacheKey<ClientPO, long> ClientKey => new($"FSS_ClientList", o => o.Id, EumCacheStoreType.Redis);
        public static Task ClientClear(long clientId) => RedisContext.Instance.CacheManager.RemoveItemAsync(ClientKey, clientId);
    }
}