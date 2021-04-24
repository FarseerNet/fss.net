using System;
using System.Collections.Generic;
using System.Linq;
using FS.Cache.Redis;
using FSS.Abstract.Entity.RegisterCenter;
using FSS.Com.RegisterCenterServer.Abstract;
using Newtonsoft.Json;

namespace FSS.Com.RegisterCenterServer.Client
{
    /// <summary>
    /// 维护客户端注册列表
    /// </summary>
    // ReSharper disable once UnusedType.Global
    public class ClientEndpoint : IClientEndpoint
    {
        public IRedisCacheManager RedisCacheManager { get; set; }

        private string key = "ClientEndpoint";

        /// <summary>
        /// 添加客户端信息到客户端列表
        /// </summary>
        public void Add(string clientId, ClientVO client)
        {
            RedisCacheManager.Db.HashSet(key, clientId, JsonConvert.SerializeObject(client));
        }

        /// <summary>
        /// 当前注册的列表
        /// </summary>
        public List<ClientVO> ToList()
        {
            return RedisCacheManager.Db.HashGetAll(key).Select(o => JsonConvert.DeserializeObject<ClientVO>(o.Value)).ToList();
        }

        /// <summary>
        /// 获取客户端
        /// </summary>
        public ClientVO ToEntity(string clientId)
        {
            var redisValue = RedisCacheManager.Db.HashGet(key, clientId);
            return !redisValue.HasValue ? null : JsonConvert.DeserializeObject<ClientVO>(redisValue.ToString());
        }
    }
}