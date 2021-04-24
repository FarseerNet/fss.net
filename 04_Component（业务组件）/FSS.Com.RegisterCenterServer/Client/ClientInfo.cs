using System;
using System.Linq;
using FSS.Abstract.Entity.RegisterCenter;
using FSS.Abstract.Server.RegisterCenter;
using FSS.Com.RegisterCenterServer.Abstract;

namespace FSS.Com.RegisterCenterServer.Client
{
    public class ClientInfo : IClientInfo
    {
        public IClientEndpoint ClientEndpoint { get; set; }
        
        /// <summary>
        /// 通过轮询的方式，取出客户端
        /// </summary>
        public ClientVO Slb()
        {
            var clientVos = ClientEndpoint.ToList();
            if (clientVos == null || clientVos.Count == 0) return null;

            // 心跳时间在30秒以内，使用时间最后的。
            return clientVos.OrderBy(o => o.UseAt).FirstOrDefault(o => (DateTime.Now - o.ActivityAt).TotalSeconds <= 30);
        }
    }
}