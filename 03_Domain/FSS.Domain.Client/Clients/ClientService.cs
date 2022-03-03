using FS.DI;
using FSS.Domain.Client.Clients.Publish;
using FSS.Domain.Client.Clients.Repository;

namespace FSS.Domain.Client.Clients;

public class ClientService : ISingletonDependency
{
    public IClientRepository ClientRepository { get; set; }

    /// <summary>
    ///     检查超时离线的客户端
    /// </summary>
    public async Task CheckTimeoutAsync()
    {
        var lst = await ClientRepository.ToListAsync();
        // 心跳大于1分钟，认为已经下线了
        foreach (var client in lst.Where(client => (DateTime.Now - client.ActivateAt).TotalMinutes >= 1))
        {
            await client.RemoveAsync();
            IocManager.GetService<IClientOfflinePublish>().Publish(sender: this, message: client);
        }
    }
}