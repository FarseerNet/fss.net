using System;
using System.Threading.Tasks;
using FS.DI;
using FSS.Abstract.Entity.MetaInfo;
using FSS.Abstract.Server.RegisterCenter;
using FSS.Abstract.Server.RemoteCall;
using FSS.Infrastructure.Common;
using Grpc.Core;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;

namespace FSS.GrpcService.Services
{
    public class RegisterCenterService : RegisterCenter.RegisterCenterBase
    {
        private readonly IIocManager _ioc;

        public RegisterCenterService(IIocManager ioc)
        {
            _ioc = ioc;
        }

        /// <summary>
        /// 注册
        /// </summary>
        public override async Task Register(IAsyncStreamReader<RegisterRequest> requestStream, IServerStreamWriter<RpcResponse> responseStream, ServerCallContext context)
        {
            var    ip       = context.Peer.Split(':')[1];
            var    clientId = "";
            string endpoint = null;
            try
            {
                await foreach (var registerRequest in requestStream.ReadAllAsync())
                {
                    // 注册、心跳
                    var result = await GrpcTools.Try(() =>
                    {
                        if (endpoint == null)
                        {
                            endpoint = $"http://{ip}:{registerRequest.ReceiveNotifyPort}";
                            clientId = registerRequest.ClientId;
                            _ioc.Logger<RegisterCenterService>().LogInformation($"客户端:{endpoint} 注册进来了");
                        }

                        return _ioc.Resolve<IClientRegister>().Register(registerRequest.ClientId, endpoint);
                    }, "注册成功");
                    await responseStream.WriteAsync(result);
                }
            }
            catch (Exception e)
            {
                if (e.InnerException is ConnectionAbortedException)
                {
                    if (!string.IsNullOrWhiteSpace(clientId))
                    {
                        _ioc.Resolve<IClientSlb>().Remove(clientId);
                        _ioc.Logger<RegisterCenterService>().LogWarning($"客户端{endpoint} 断开连接");
                    }
                }
            }
        }
    }
}