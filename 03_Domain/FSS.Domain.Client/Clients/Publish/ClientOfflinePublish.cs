using FS.Core.DomainDriven.DomainEvent;

namespace FSS.Domain.Client.Clients.Publish;

public class ClientOfflinePublish : BaseDomainEvent
{
    protected override string EventName => "ClientOffline";

    public ClientDO Client { get; set; }

    public ClientOfflinePublish(ClientDO client)
    {
        Client = client;
    }
}