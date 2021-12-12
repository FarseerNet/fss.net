using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FSS.Infrastructure.Repository.Client.Interface;
using FSS.Infrastructure.Repository.Client.Model;

namespace FSS.Infrastructure.Repository.Client
{
    public class ClientCache : IClientAgent
    {
        /// <summary>
        /// 更新客户端调用的使用时间
        /// </summary>
        public void UpdateClient(ClientPO client)
        {
            var key = CacheKeys.ClientKey;
            RedisContext.Instance.CacheManager.SaveItem(key, client);
        }

        /// <summary>
        /// 取出全局客户端
        /// </summary>
        public Task<ClientPO> ToInfoAsync(long clientId)
        {
            var key = CacheKeys.ClientKey;
            return RedisContext.Instance.CacheManager.GetItemAsync(key, clientId, () => new List<ClientPO>());
        }

        /// <summary>
        /// 取出全局客户端列表
        /// </summary>
        public async Task<List<ClientPO>> ToListAsync()
        {
            var key = CacheKeys.ClientKey;
            var lst = await RedisContext.Instance.CacheManager.GetListAsync(key, () => new List<ClientPO>());

            for (int i = 0; i < lst.Count; i++)
            {
                // 心跳大于1秒中，任为已经下线了
                if ((DateTime.Now - lst[i].ActivateAt).TotalMinutes >= 1)
                {
                    await CacheKeys.ClientClear(lst[i].Id);
                    lst.RemoveAt(i);
                    i--;
                }
            }

            return lst;
        }

        /// <summary>
        /// 移除客户端
        /// </summary>
        public Task RemoveClientAsync(long id) => CacheKeys.ClientClear(id);
        
        /// <summary>
        /// 取出全局客户端数量（fops在用）
        /// </summary>
        public Task<long> GetCountAsync()
        {
            var key = CacheKeys.ClientKey;
            return RedisContext.Instance.CacheManager.GetCountAsync(key);
        }
    }
}