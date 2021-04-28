using System;
using System.Collections.Generic;
using FSS.Abstract.Entity.RegisterCenter;
using FSS.Abstract.Server.RegisterCenter;

namespace FSS.Com.RegisterCenterServer.Client
{
    /// <summary>
    /// 客户端注册
    /// </summary>
    // ReSharper disable once UnusedType.Global
    public class ClientRegister : IClientRegister
    {
        public static readonly Dictionary<string, ClientConnectVO> Clients = new();

        /// <summary>
        /// 注册
        /// </summary>
        public void Register(ClientConnectVO client)
        {
            client.UseAt               = DateTime.Now;
            Clients[client.ServerHost] = client;
        }

        /// <summary>
        /// 取出客户端
        /// </summary>
        public ClientConnectVO ToInfo(string serverHost)
        {
            Clients.TryGetValue(serverHost, out var client);
            return client;
        }

        /// <summary>
        /// 客户端是否存在
        /// </summary>
        public bool IsExists(string serverHost)
        {
            return Clients.ContainsKey(serverHost);
        }

        /// <summary>
        /// 移除客户端
        /// </summary>
        public void Remove(string serverHost)
        {
            Clients.Remove(serverHost);
        }
    }
}