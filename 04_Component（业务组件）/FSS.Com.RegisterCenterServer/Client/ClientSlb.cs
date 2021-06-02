using System;
using System.Linq;
using FSS.Abstract.Entity.RegisterCenter;
using FSS.Abstract.Server.RegisterCenter;

namespace FSS.Com.RegisterCenterServer.Client
{
    // ReSharper disable once UnusedType.Global
    public class ClientSlb : IClientSlb
    {
        public IClientRegister ClientRegister { get; set; }

        /// <summary>
        /// 通过轮询的方式，取出客户端
        /// </summary>
        public ClientConnectVO Slb(string jobName)
        {
            var clientVos = ClientRegister.ToList();
            if (clientVos == null || clientVos.Count == 0) return null;

            // 简单实现：取使用时间最后的。
            return clientVos.Where(o => o.JobName==jobName).OrderBy(o => o.UseAt).FirstOrDefault();
            //return clientVos.Find(o => o.JobName == jobName);
        }
    }
}