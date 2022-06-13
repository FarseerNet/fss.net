using System;
using System.Linq;
using System.Threading.Tasks;
using FS.Core.Abstract.Fss;
using FS.DI;
using FS.Fss;
using FSS.Domain.Client.Clients.Publish;
using FSS.Domain.Client.Clients.Repository;

namespace FSS.Application.Job;

/// <summary>
///     检查超时离线的客户端
/// </summary>
[FssJob(Name = "FSS.CheckClientOffline")]
public class CheckClientOfflineJob : IFssJob
{
    public IClientRepository ClientRepository { get; set; }

    public Task<bool> Execute(IFssContext context)
    {
        var lst = ClientRepository.ToList();
        
        // 心跳大于1分钟，认为已经下线了
        foreach (var client in lst.Where(client => client.IsTimeout()))
        {
            IocManager.GetService<IClientRepository>().RemoveClient(client.Id);
            new ClientOfflinePublish(client).PublishEvent();
        }

        return Task.FromResult(true);
    }
}