using System;
using System.Threading.Tasks;
using FS.DI;
using FS.Extends;
using FSS.Abstract.Entity.RegisterCenter;
using FSS.Abstract.Server.RegisterCenter;
using FSS.Abstract.Server.RemoteCall;
using FSS.GrpcService;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace FSS.Com.RemoteCallServer.RemoteCommand
{
    /// <summary>
    /// 注册命令
    /// </summary>
    public class RegisterCommand : IRemoteCommand
    {
        public IClientResponse ClientResponse { get; set; }
        public IClientRegister ClientRegister { get; set; }

        /// <summary>
        /// 客户端请求流
        /// </summary>
        private IAsyncStreamReader<ChannelRequest> _requestStream;

        /// <summary>
        /// 客户端响应流
        /// </summary>
        private IServerStreamWriter<CommandResponse> _responseStream;

        /// <summary>
        /// 处理
        /// </summary>
        /// <param name="context">上下文</param>
        /// <param name="requestStream">请求流</param>
        /// <param name="responseStream">响应流</param>
        public async Task InvokeAsync(ServerCallContext context, object requestStream, object responseStream)
        {
            _requestStream  = (IAsyncStreamReader<ChannelRequest>) requestStream;
            _responseStream = (IServerStreamWriter<CommandResponse>) responseStream;

            // 注册
            var clientConnectVO = new ClientConnectVO
            {
                Context        = context,
                ServerHost     = $"{context.Host}_{context.Peer}",
                ClientIp       = context.RequestHeaders.GetValue("client_ip"),
                RequestStream  = requestStream,
                ResponseStream = responseStream,
                RegisterAt     = _requestStream.Current.RequestAt.ToTimestamps()
            };
            ClientRegister.Register(clientConnectVO);

            IocManager.Instance.Logger<RegisterCommand>().LogInformation($"客户端:{clientConnectVO.ClientIp} 注册进来了");
            await ClientResponse.PrintAsync(_responseStream, $"FSS平台==>{clientConnectVO.ServerHost}：成功建立连接，欢迎{clientConnectVO.ClientIp}");
        }
    }
}