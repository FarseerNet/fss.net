using System.Threading.Tasks;
using Grpc.Core;

namespace FSS.Abstract.Server.RemoteCall
{
    /// <summary>
    /// 根据不同的命令，提供不同的处理
    /// </summary>
    public interface IRemoteCommand
    {
        /// <summary>
        /// 处理
        /// </summary>
        /// <param name="context"> </param>
        /// <param name="requestStream">请求流</param>
        /// <param name="responseStream">响应流</param>
        Task InvokeAsync(ServerCallContext context, object requestStream, object responseStream);
    }
}