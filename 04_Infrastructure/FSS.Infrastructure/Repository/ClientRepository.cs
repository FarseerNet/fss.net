using System.Collections.Generic;
using System.Threading.Tasks;
using FS.Extends;
using FSS.Domain.Client.Clients;
using FSS.Domain.Client.Clients.Repository;
using FSS.Infrastructure.Repository.Client;
using FSS.Infrastructure.Repository.Client.Model;

namespace FSS.Infrastructure.Repository;

public class ClientRepository : IClientRepository
{
    public ClientCache ClientCache { get; set; }

    public Task<List<ClientDO>> ToListAsync() => ClientCache.ToListAsync().MapAsync<ClientDO, ClientPO>();

    public Task RemoveClientAsync(ClientDO clientDO) => ClientCache.ClientClear(clientId: clientDO.Id);

    public Task<ClientDO> ToEntityAsync(long clientId) => ClientCache.ToInfoAsync(clientId: clientId).MapAsync<ClientDO, ClientPO>();

    public void Update(ClientDO clientDO) => ClientCache.UpdateClient(client: clientDO.Map<ClientPO>());

    public Task<long> GetCountAsync() => ClientCache.GetCountAsync();
}