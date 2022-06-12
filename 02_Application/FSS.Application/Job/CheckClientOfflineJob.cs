using System.Threading.Tasks;
using FS.Core.Abstract.Fss;
using FS.Fss;
using FSS.Domain.Client.Clients;

namespace FSS.Application.Job;

/// <summary>
///     检查超时离线的客户端
/// </summary>
[FssJob(Name = "FSS.CheckClientOffline")]
public class CheckClientOfflineJob : IFssJob
{
    public ClientService ClientService { get; set; }
    public Task<bool> Execute(IFssContext context)
    {
        ClientService.CheckTimeout();
        return Task.FromResult(true);
    }
}