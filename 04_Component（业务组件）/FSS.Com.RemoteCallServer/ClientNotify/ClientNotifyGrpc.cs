using FSS.Abstract.Entity.MetaInfo;
using FSS.Abstract.Entity.RegisterCenter;
using FSS.Abstract.Server.RemoteCall;
using FSS.Client;
using Grpc.Net.Client;

namespace FSS.Com.RemoteCallServer.ClientNotify
{
    public class ClientNotifyGrpc : IClientNotifyGrpc
    {
        /// <summary>
        /// 远程通知客户端执行JOB
        /// </summary>
        public void Invoke(ClientVO client, TaskVO task)
        {
            var registerCenterClient = new ReceiveNotify.ReceiveNotifyClient(GrpcChannel.ForAddress(client.Endpoint));
            var rpc = registerCenterClient.JobInvoke(new JobInvokeRequest
            {
                TaskId      = task.Id,
                Caption     = "aaa",
                JobTypeName = "bbb",
                StartAt     = 0
            });
        }
    }
}