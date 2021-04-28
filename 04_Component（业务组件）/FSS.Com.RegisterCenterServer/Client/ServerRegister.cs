using System;
using System.Linq;
using FS.Cache.Redis;
using FS.Utils.Common;
using FS.Extends;
using FSS.Abstract.Server.RegisterCenter;

namespace FSS.Com.RegisterCenterServer.Client
{
    public class ServerRegister : IServerRegister
    {
        public string             Key = "ServerList";
        public IRedisCacheManager RedisCacheManager { get; set; }

        /// <summary>
        /// 注册当前IP到服务列表中
        /// </summary>
        public void Register()
        {
            foreach (var ipAddress in IpHelper.GetIps().Select(o => o.Address.MapToIPv4().ToString()).Distinct())
            {
                RedisCacheManager.Db.HashSet(Key, ipAddress, DateTime.Now.ToTimestamp());
            }
        }
    }
}