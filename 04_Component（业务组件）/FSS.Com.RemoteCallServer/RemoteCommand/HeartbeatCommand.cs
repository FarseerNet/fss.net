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
    /// 心跳命令
    /// </summary>
    public class HeartbeatCommand : IRemoteCommand
    {
        public IClientRegister ClientRegister { get; set; }
        public IIocManager     IocManager      { get; set; }

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
        public Task InvokeAsync(ServerCallContext context, object requestStream, object responseStream)
        {
            _requestStream  = (IAsyncStreamReader<ChannelRequest>) requestStream;
            _responseStream = (IServerStreamWriter<CommandResponse>) responseStream;

            // 心跳
            var serverHost = $"{context.Host}_{context.Peer}";
            //var clientConnectVO = new ClientConnectVO
            //{
            //    ServerHost     = serverHost,
            //    ClientIp       = context.RequestHeaders.GetValue("client_ip"),
            //    RequestStream  = requestStream,
            //    ResponseStream = responseStream,
            //    RegisterAt     = _requestStream.Current.RequestAt.ToTimestamps(),
            //    HeartbeatAt    = DateTime.Now
            //};
            ClientRegister.UpdateHeartbeatAt(serverHost, DateTime.Now);
            
            IocManager.Logger<HeartbeatCommand>().LogDebug($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} 收到客户端===> {serverHost}的心跳");
            return Task.FromResult(0);
        }
    }
}