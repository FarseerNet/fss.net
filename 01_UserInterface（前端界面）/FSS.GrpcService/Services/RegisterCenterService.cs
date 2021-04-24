using System.Threading.Tasks;
using FS.DI;
using FSS.Abstract.Server.RegisterCenter;
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
        public override Task<RpcResponse> Register(RegisterRequest request, ServerCallContext context)
        {
            var ip = context.Peer.Split(':')[1];
            // 注册
            return GrpcTools.Try(() => _ioc.Resolve<IClientRegister>().Register(request.ClientId, $"http://{ip}:{request.ReceiveNotifyPort}"));
        }
    }
}