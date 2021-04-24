using System;

namespace FSS.Abstract.Entity.RegisterCenter
{
    public class ClientVO
    {
        /// <summary>
        /// 客户端节点
        /// </summary>
        public string Endpoint { get; set; }
        /// <summary>
        /// 活动时间（客户端发起的心跳）
        /// </summary>
        public DateTime ActivityAt { get; set; }
        /// <summary>
        /// 服务端最后一次使用的时间（用来做负载）
        /// </summary>
        public DateTime UseAt { get; set; }
    }
}