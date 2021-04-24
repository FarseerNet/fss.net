using FSS.Abstract.Server.RegisterCenter;
using FSS.Com.RegisterCenterServer.Abstract;

namespace FSS.Com.RegisterCenterServer.Client
{
    /// <summary>
    /// 客户端注册
    /// </summary>
    public class ClientRegister : IClientRegister
    {
        public IClientEndpointAdd ClientEndpointAdd { get; set; }
        
        /// <summary>
        /// 注册
        /// </summary>
        public bool Register(string clientId, string endpoint)
        {
            ClientEndpointAdd.Add(clientId, endpoint);
            return true;
        }
    }
}