using System.Threading.Tasks;
using FS.Core.Job;
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
    public async Task<bool> Execute(IFssContext context)
    {
        await ClientService.CheckTimeoutAsync();
        return true;
    }
}