using System;
using System.Threading.Tasks;
using FS.DI;
using FSS.Abstract.Entity.MetaInfo;
using FSS.Abstract.Server.RegisterCenter;
using FSS.Abstract.Server.RemoteCall;
using FSS.Infrastructure.Common;
using Grpc.Core;

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
            var ip = context.Peer.Split(':')[1];
            // 持续注册
            await foreach (var registerRequest in requestStream.ReadAllAsync())
            {
                // 注册
                var result = await GrpcTools.Try(() =>
                {
                    var registerResult = _ioc.Resolve<IClientRegister>().Register(registerRequest.ClientId, $"http://{ip}:{registerRequest.ReceiveNotifyPort}");

                    // 测试使用
                    var clientVO = _ioc.Resolve<IClientSlb>().Slb();
                    //_ioc.Resolve<IClientNotifyGrpc>().Invoke(clientVO, new TaskVO());
                    
                    return registerResult;
                }, "注册成功");

                await responseStream.WriteAsync(result);
            }
        }
    }
}