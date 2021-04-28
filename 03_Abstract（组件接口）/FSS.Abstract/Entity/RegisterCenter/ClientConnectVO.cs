using System;
using Grpc.Core;

namespace FSS.Abstract.Entity.RegisterCenter
{
    /// <summary>
    /// 客户端连接信息
    /// </summary>
    public class ClientConnectVO
    {
        /// <summary>
        /// 上下文
        /// </summary>
        public ServerCallContext Context { get; set; }
        
        /// <summary>
        /// 服务端打开通道的端口
        /// </summary>
        public string ServerHost { get;  set; }
        
        /// <summary>
        /// 客户端请求流
        /// </summary>
        public object RequestStream  { get; set; }
        
        /// <summary>
        /// 客户端响应流
        /// </summary>
        public object ResponseStream { get; set; }
        
        /// <summary>
        /// 客户端
        /// </summary>
        public string ClientIp { get; set; }
        
        /// <summary>
        /// 客户端连接时间
        /// </summary>
        public DateTime RegisterAt { get; set; }
        
        /// <summary>
        /// 服务端最后一次使用的时间（用来做负载）
        /// </summary>
        public DateTime UseAt { get; set; }
    }
}