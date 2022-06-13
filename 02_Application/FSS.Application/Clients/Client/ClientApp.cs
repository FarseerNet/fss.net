using System.Collections.Generic;
using Collections.Pooled;
using FS.DI;
using FS.Extends;
using FSS.Application.Clients.Client.Entity;
using FSS.Domain.Client.Clients;
using FSS.Domain.Client.Clients.Repository;

namespace FSS.Application.Clients.Client;

public class ClientApp : ISingletonDependency
{
    public IClientRepository ClientRepository { get; set; }

    /// <summary>
    ///     取出全局客户端列表
    /// </summary>
    public PooledList<ClientDTO> ToList() => ClientRepository.ToList().Map<ClientDTO>();

    /// <summary>
    ///     更新客户端的使用时间
    /// </summary>
    public void UpdateClient(ClientDO clientDO) => ClientRepository.Update(clientDO);

    /// <summary>
    ///     客户端数量
    /// </summary>
    public long GetCount() => ClientRepository.GetCount();
}