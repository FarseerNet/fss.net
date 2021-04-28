using System;
using System.Collections.Generic;
using System.Linq;
using FS.Cache.Redis;
using FSS.Abstract.Entity.RegisterCenter;
using FSS.Abstract.Server.RegisterCenter;
using Newtonsoft.Json;

namespace FSS.Com.RegisterCenterServer.Client
{
    /// <summary>
    /// 客户端注册
    /// </summary>
    // ReSharper disable once UnusedType.Global
    public class ClientRegister : IClientRegister
    {
        public string             Key = "ClientList";
        
        public IRedisCacheManager RedisCacheManager { get; set; }

        /// <summary>
        /// 注册
        /// </summary>
        public void Register(ClientConnectVO client)
        {
            client.HeartbeatAt = DateTime.Now;
            RedisCacheManager.Db.HashSet(Key, client.ServerHost, client.ToString());
        }

        /// <summary>
        /// 取出客户端列表
        /// </summary>
        public List<ClientConnectVO> ToList()
        {
            var redisValue = RedisCacheManager.Db.HashGetAll(Key);
            if (redisValue.Length == 0) return null;
            var clientConnectVos = redisValue.Select(o=>JsonConvert.DeserializeObject<ClientConnectVO>(o.Value)).ToList();
            for (int i = 0; i < clientConnectVos.Count; i++)
            {
                if ((DateTime.Now - clientConnectVos[i].HeartbeatAt).TotalMilliseconds > 3000)
                {
                    Remove(clientConnectVos[i].ServerHost);
                    clientConnectVos.RemoveAt(i);
                    i--;
                }
            }
            return clientConnectVos;
        }

        /// <summary>
        /// 取出客户端
        /// </summary>
        public ClientConnectVO ToInfo(string serverHost)
        {
            var redisValue = RedisCacheManager.Db.HashGet(Key, serverHost);
            if (!redisValue.HasValue) return null;
            var client = JsonConvert.DeserializeObject<ClientConnectVO>(redisValue.ToString());
            if ((DateTime.Now - client.HeartbeatAt).TotalMilliseconds > 3000)
            {
                Remove(serverHost);
                return null;
            }

            return client;
        }
        
        /// <summary>
        /// 更新客户端调用的使用时间
        /// </summary>
        public void UpdateUseAt(string serverHost, DateTime useAt)
        {
            var client = ToInfo(serverHost);
            if (client == null) return;
            client.UseAt = useAt;
            RedisCacheManager.Db.HashSet(Key, client.ServerHost, client.ToString());
        }

        /// <summary>
        /// 更新客户端心跳时间
        /// </summary>
        public void UpdateHeartbeatAt(string serverHost, DateTime heartbeatAt)
        {
            var client = ToInfo(serverHost);
            if (client == null) return;
            client.HeartbeatAt = heartbeatAt;
            RedisCacheManager.Db.HashSet(Key, client.ServerHost, client.ToString());
        }

        /// <summary>
        /// 移除客户端
        /// </summary>
        public void Remove(string serverHost)
        {
            RedisCacheManager.Db.HashDelete(Key, serverHost);
        }
    }
}