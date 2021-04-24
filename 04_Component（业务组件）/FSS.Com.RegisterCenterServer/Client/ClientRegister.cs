using System;
using FSS.Abstract.Entity.RegisterCenter;
using FSS.Abstract.Server.RegisterCenter;
using FSS.Com.RegisterCenterServer.Abstract;

namespace FSS.Com.RegisterCenterServer.Client
{
    /// <summary>
    /// 客户端注册
    /// </summary>
    public class ClientRegister : IClientRegister
    {
        public IClientEndpoint ClientEndpoint { get; set; }
        
        /// <summary>
        /// 注册
        /// </summary>
        public bool Register(string clientId, string endpoint)
        {
            // 找到已注册的客户端
            var clientVO = ClientEndpoint.ToEntity(clientId)?? new ClientVO
            {
                Endpoint = endpoint,
                UseAt    = DateTime.Now
            };
            
            // 更新激活时间
            clientVO.ActivityAt = DateTime.Now;
            
            ClientEndpoint.Add(clientId, clientVO);
            return true;
        }
    }
}