using System;
using System.Linq;
using FSS.Abstract.Entity.RegisterCenter;
using FSS.Abstract.Server.RegisterCenter;
using FSS.Com.RegisterCenterServer.Abstract;

namespace FSS.Com.RegisterCenterServer.Client
{
    // ReSharper disable once UnusedType.Global
    public class ClientSlb : IClientSlb
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

        /// <summary>
        /// 更新客户端调用的使用时间
        /// </summary>
        public void UpdateUseAt(string clientId, DateTime useAt)
        {
            var clientVO = ClientEndpoint.ToEntity(clientId);
            if (clientVO == null) return;
            clientVO.UseAt = useAt;
            ClientEndpoint.Save(clientId, clientVO);
        }

        /// <summary>
        /// 下线客户端
        /// </summary>
        public void Remove(string clientId)
        {
            ClientEndpoint.Remove(clientId);
        }
    }
}