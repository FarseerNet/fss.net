using System.Linq;
using System.Threading.Tasks;
using FS.Extends;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace FSS.GrpcService
{
    /// <summary>
    /// 鉴权拦截器
    /// </summary>
    public class AuthInterceptor : Interceptor
    {
        public override Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
        {
            LogCall(MethodType.Unary, context);
            return continuation(request, context);
        }

        /// <summary>
        /// 鉴权
        /// </summary>
        private void LogCall(MethodType methodType, ServerCallContext context)
        {
        }
    }
}