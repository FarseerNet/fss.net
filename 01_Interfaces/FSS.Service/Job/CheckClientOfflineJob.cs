using System.Threading.Tasks;
using FS.Core.Job;
using FS.Job;
using FSS.Application.Clients.Client;

namespace FSS.Service.Job
{
    /// <summary>
    /// 检查超时离线的客户端
    /// </summary>
    [FssJob(Name = "FSS.CheckClientOffline")]
    public class CheckClientOfflineJob : IFssJob
    {
        public ClientApp ClientApp { get; set; }
        public async Task<bool> Execute(IFssContext context)
        {
            await ClientApp.CheckTimeoutAsync();
            return true;
        }
    }
}