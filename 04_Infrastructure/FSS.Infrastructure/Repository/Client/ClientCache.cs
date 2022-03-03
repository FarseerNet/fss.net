using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FS.Cache;
using FS.DI;
using FSS.Infrastructure.Repository.Client.Model;

namespace FSS.Infrastructure.Repository.Client
{
    public class ClientCache : ISingletonDependency
    {
        /// <summary> 客户端缓存 </summary>
        public CacheKey<ClientPO, long> ClientKey => new($"FSS_ClientList", o => o.Id, EumCacheStoreType.Redis);
        
        /// <summary>
        /// 更新客户端调用的使用时间
        /// </summary>
        public void UpdateClient(ClientPO client)
        {
            var key = ClientKey;
            RedisContext.Instance.CacheManager.SaveItem(key, client);
        }

        /// <summary>
        /// 取出全局客户端
        /// </summary>
        public Task<ClientPO> ToInfoAsync(long clientId)
        {
            var key = ClientKey;
            return RedisContext.Instance.CacheManager.GetItemAsync(key, clientId, () => new List<ClientPO>());
        }

        /// <summary>
        /// 取出全局客户端列表
        /// </summary>
        public async Task<List<ClientPO>> ToListAsync()
        {
            var key = ClientKey;
            var lst = await RedisContext.Instance.CacheManager.GetListAsync(key, () => new List<ClientPO>());

            for (int i = 0; i < lst.Count; i++)
            {
                // 心跳大于1秒中，任为已经下线了
                if ((DateTime.Now - lst[i].ActivateAt).TotalMinutes >= 1)
                {
                    await ClientClear(lst[i].Id);
                    lst.RemoveAt(i);
                    i--;
                }
            }

            return lst;
        }
        
        /// <summary>
        /// 取出全局客户端数量（fops在用）
        /// </summary>
        public Task<long> GetCountAsync()
        {
            var key = ClientKey;
            return RedisContext.Instance.CacheManager.GetCountAsync(key);
        }
        

        /// <summary>
        /// 移除客户端
        /// </summary>
        public Task ClientClear(long clientId) => RedisContext.Instance.CacheManager.RemoveItemAsync(ClientKey, clientId);
    }
}