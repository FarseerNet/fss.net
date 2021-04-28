using System;
using System.Linq;
using FSS.Abstract.Entity.RegisterCenter;
using FSS.Abstract.Server.RegisterCenter;

namespace FSS.Com.RegisterCenterServer.Client
{
    // ReSharper disable once UnusedType.Global
    public class ClientSlb : IClientSlb
    {
        /// <summary>
        /// 通过轮询的方式，取出客户端
        /// </summary>
        public ClientConnectVO Slb()
        {
            var clientVos = ClientRegister.Clients;
            if (clientVos == null || clientVos.Count == 0) return null;

            // 简单实现：取使用时间最后的。
            return clientVos.Values.OrderBy(o => o.UseAt).FirstOrDefault();
        }

        /// <summary>
        /// 更新客户端调用的使用时间
        /// </summary>
        public void UpdateUseAt(string serverHost, DateTime useAt)
        {
            ClientRegister.Clients.TryGetValue(serverHost, out var client);
            if (client == null) return;
            client.UseAt = useAt;
        }
    }
}