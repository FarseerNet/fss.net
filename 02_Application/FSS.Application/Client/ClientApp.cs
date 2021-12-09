using System.Collections.Generic;
using System.Threading.Tasks;
using FS.Extends;
using FSS.Abstract.Entity;
using FSS.Application.Client.Interface;
using FSS.Domain.Client.Client.Interface;
using FSS.Infrastructure.Repository.Client.Interface;
using FSS.Infrastructure.Repository.Client.Model;

namespace FSS.Application.Client
{
    public class ClientApp : IClientApp
    {
        public IClientService ClientService { get; set; }
        public IClientAgent   ClientAgent   { get; set; }

        /// <summary>
        /// 检查超时离线的客户端
        /// </summary>
        public Task CheckTimeoutAsync() => ClientService.CheckTimeoutAsync();

        /// <summary>
        /// 取出全局客户端列表
        /// </summary>
        public Task<List<ClientVO>> ToListAsync() => ClientAgent.ToListAsync().MapAsync<ClientVO, ClientPO>();
    }
}