using System.Threading.Tasks;
using FS.DI;

namespace FSS.Domain.Client.Clients.Interface
{
    public interface IClientService: ISingletonDependency
    {
        /// <summary>
        /// 检查超时离线的客户端
        /// </summary>
        Task CheckTimeoutAsync();
    }
}