using FS.Cache.Redis;
using FSS.Com.RegisterCenterServer.Abstract;

namespace FSS.Com.RegisterCenterServer.Client
{
    /// <summary>
    /// 维护客户端注册列表
    /// </summary>
    // ReSharper disable once UnusedType.Global
    public class ClientEndpointAdd : IClientEndpointAdd
    {
        public IRedisCacheManager RedisCacheManager { get; set; }

        private string key = "ClientEndpoint";

        /// <summary>
        /// 添加客户端信息到客户端列表
        /// </summary>
        public void Add(string clientId, string endpoint)
        {
            RedisCacheManager.Db.HashSet(key, clientId, endpoint);
        }
    }
}