using System;
using System.Collections.Generic;
using System.Linq;
using FS.Cache.Redis;
using FSS.Abstract.Entity.RegisterCenter;
using FSS.Abstract.Server.RegisterCenter;

namespace FSS.Com.RegisterCenterServer.Client
{
    /// <summary>
    /// 客户端注册
    /// </summary>
    // ReSharper disable once UnusedType.Global
    public class ClientRegister : IClientRegister
    {
        private                 string                              Key = "ClientList";
        public                  IRedisCacheManager                  RedisCacheManager { get; set; }
        private static readonly Dictionary<string, ClientConnectVO> Clients = new();

        /// <summary>
        /// 注册
        /// </summary>
        public void Register(ClientConnectVO client)
        {
            //client.UseAt               = DateTime.Now;
            Clients[client.ServerHost] = client;
        }

        /// <summary>
        /// 更新客户端调用的使用时间
        /// </summary>
        public void UpdateUseAt(string serverHost, DateTime useAt)
        {
            if (Clients.TryGetValue(serverHost, out var client))
            {
                client.UseAt = useAt;
            }
        }

        /// <summary>
        /// 更新客户端心跳时间
        /// </summary>
        public void UpdateHeartbeatAt(string serverHost, DateTime heartbeatAt)
        {
            if (Clients.TryGetValue(serverHost, out var client))
            {
                client.HeartbeatAt = heartbeatAt;
            }
        }

        /// <summary>
        /// 取出客户端列表
        /// </summary>
        public List<ClientConnectVO> ToList()
        {
            var lstTimeoutClient = Clients.Where(o => (DateTime.Now - o.Value.HeartbeatAt).TotalMilliseconds > 10000);
            foreach (var client in lstTimeoutClient)
            {
                Clients.Remove(client.Key);
            }

            return Clients.Select(o => o.Value).ToList();
        }

        /// <summary>
        /// 获取客户端数量
        /// </summary>
        public int Count() => Clients.Count(o => (DateTime.Now - o.Value.HeartbeatAt).TotalMilliseconds < 10000);

        /// <summary>
        /// 取出客户端
        /// </summary>
        public ClientConnectVO ToInfo(string serverHost)
        {
            if (Clients.TryGetValue(serverHost, out var client))
            {
                if ((DateTime.Now - client.HeartbeatAt).TotalMilliseconds > 10000)
                {
                    Clients.Remove(serverHost);
                    return null;
                }
            }
            return client;
        }

        /// <summary>
        /// 同步本地缓存到Redis
        /// </summary>
        public void SyncCache()
        {
            var lst = ToList();
            RedisCacheManager.Db.KeyDelete(Key);
            RedisCacheManager.HashSetTransaction(Key, lst, vo => vo.ServerHost);
        }

        /// <summary>
        /// 客户端是否存在
        /// </summary>
        public bool IsExists(string serverHost)
        {
            return Clients.ContainsKey(serverHost);
        }

        /// <summary>
        /// 移除客户端
        /// </summary>
        public void Remove(string serverHost)
        {
            Clients.Remove(serverHost);
        }
    }
}