using System.Threading.Tasks;
using FSS.Abstract.Entity.RemoveCall;
using FSS.Abstract.Server.MetaInfo;
using FSS.Abstract.Server.RemoteCall;
using FSS.GrpcService;
using Grpc.Core;
using Newtonsoft.Json;

namespace FSS.Com.RemoteCallServer.RemoteCommand
{
    /// <summary>
    /// 任务状态（客户把JOB执行状态同步给服务端）
    /// </summary>
    public class JobStatusCommand : IRemoteCommand
    {
        public ITaskInfo        TaskInfo        { get; set; }
        public ITaskGroupInfo   TaskGroupInfo   { get; set; }
        public ITaskUpdate      TaskUpdate      { get; set; }
        public ITaskGroupUpdate TaskGroupUpdate { get; set; }
        public IRunLogAdd       RunLogAdd       { get; set; }

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

            var jobStatus = JsonConvert.DeserializeObject<JobStatusVO>(_requestStream.Current.Data);
            
            // 取出当前Task
            var task      = TaskInfo.ToGroupTask(jobStatus.);
            if (task.Id!=jobStatus.TaskId){}
            var taskGroup = TaskGroupInfo.ToInfo(task.TaskGroupId);

            // 更新Task
            task.Progress = jobStatus.Progress;
            task.Status   = jobStatus.Status;
            task.RunSpeed = jobStatus.RunSpeed;
            TaskUpdate.Update(task);

            // 设置下一次的执行时间，并更新
            taskGroup.NextAt = jobStatus.NextAt;
            TaskGroupUpdate.Update(taskGroup);

            // 如果有日志
            if (!string.IsNullOrWhiteSpace(jobStatus.Log))
            {
                RunLogAdd.Add(task.TaskGroupId, task.Id, jobStatus.LogLevel, jobStatus.Log);
            }

            return Task.FromResult(0);
        }
    }
}