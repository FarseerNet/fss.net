using System.Collections.Generic;
using System.Threading.Tasks;
using FS.Extends;
using FSS.Domain.Client.Clients.Repository;
using FSS.Infrastructure.Repository.Client;
using FSS.Infrastructure.Repository.Client.Model;

namespace FSS.Infrastructure.Repository;

public class ClientRepository : IClientRepository
{
    public ClientCache ClientCache { get; set; }

    public Task<List<Domain.Client.Clients.Entity.Client>> ToListAsync() => ClientCache.ToListAsync().MapAsync<Domain.Client.Clients.Entity.Client, ClientPO>();

    public Task RemoveClientAsync(Domain.Client.Clients.Entity.Client client) => ClientCache.ClientClear(clientId: client.Id);

    public Task<Domain.Client.Clients.Entity.Client> ToEntityAsync(long clientId) => ClientCache.ToInfoAsync(clientId: clientId).MapAsync<Domain.Client.Clients.Entity.Client, ClientPO>();

    public void Update(Domain.Client.Clients.Entity.Client client) => ClientCache.UpdateClient(client: client.Map<ClientPO>());

    public Task<long> GetCountAsync() => ClientCache.GetCountAsync();
}