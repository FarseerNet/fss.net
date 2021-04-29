using System;
using System.Linq;
using FS.Cache.Redis;
using FS.Utils.Common;
using FS.Extends;
using FSS.Abstract.Server.RegisterCenter;

namespace FSS.Com.RegisterCenterServer.Client
{
    /// <summary>
    /// 注册服务节点
    /// </summary>
    public class NodeRegister : INodeRegister
    {
        public string             Key = "NodeList";
        public IRedisCacheManager RedisCacheManager { get; set; }

        /// <summary>
        /// 注册当前IP到服务列表中
        /// </summary>
        public void Register()
        {
            var ipAddresses = IpHelper.GetIps().Select(o => o.Address.MapToIPv4().ToString()).Distinct().ToList();
            RedisCacheManager.HashSetTransaction(Key, ipAddresses, field => field, value => DateTime.Now.ToTimestamp().ToString());
        }

        /// <summary>
        /// 获取当前节点IP
        /// </summary>
        public string GetNodeIp() => IpHelper.GetIps().Select(o => o.Address.MapToIPv4().ToString()).FirstOrDefault();

        /// <summary>
        /// 检查服务节点是否在线
        /// </summary>
        public bool IsNodeExists(string ip)
        {
            var redisValue = RedisCacheManager.Db.HashGet(Key, ip);
            if (!redisValue.HasValue) return false;
            var activeAt = redisValue.ToString().ConvertType(0L);
            // 超过10S，没有活动，判定为挂了
            if (DateTime.Now.ToTimestamp() - activeAt > 10000)
            {
                RedisCacheManager.Db.HashDelete(Key, ip);
                return false;
            }

            return true;
        }
    }
}