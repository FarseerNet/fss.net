using System;
using System.Threading.Tasks;
using FS.DI;
using FS.Extends;
using FS.Utils.Component;
using FSS.Abstract.Entity.MetaInfo;
using FSS.Abstract.Entity.RegisterCenter;
using FSS.Abstract.Enum;
using FSS.Abstract.Server.MetaInfo;
using FSS.Abstract.Server.RemoteCall;
using FSS.Client;
using Grpc.Net.Client;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace FSS.Com.RemoteCallServer.ClientNotify
{
    public class ClientNotifyGrpc : IClientNotifyGrpc
    {
        public ITaskGroupInfo   TaskGroupInfo   { get; set; }
        public ITaskUpdate      TaskUpdate      { get; set; }
        public ITaskGroupUpdate TaskGroupUpdate { get; set; }
        public IRunLogAdd       RunLogAdd       { get; set; }
        public IIocManager       IocManager      { get; set; }

        /// <summary>
        /// 远程通知客户端执行JOB
        /// </summary>
        public async Task<TaskVO> Invoke(ClientVO client, TaskVO task)
        {
            var taskGroup = TaskGroupInfo.ToInfo(task.TaskGroupId);
            task.ClientEndpoint = client.Endpoint;
            task.ClientId       = client.Id;
            client.UseAt        = DateTime.Now;

            // 创建客户端通道
            var registerCenterClient = new ReceiveNotify.ReceiveNotifyClient(GrpcChannel.ForAddress(client.Endpoint));
            
            // 通知客户端执行任务
            var rpc = registerCenterClient.JobInvoke(new JobInvokeRequest
            {
                TaskId      = task.Id,
                Caption     = taskGroup.Caption,
                JobTypeName = taskGroup.JobTypeName,
                StartAt     = taskGroup.StartAt.ToTimestamps(),
                NextAt      = taskGroup.NextAt.ToTimestamps(),
            });

            task.Status = EumTaskType.Working;
            TaskUpdate.Update(task);
            RunLogAdd.Add(task.TaskGroupId, task.Id, LogLevel.Information, $"任务ID：{task.Id}，开始工作");

            var speedResult    = new SpeedTestMultiple().Begin();
            var isHaveResponse = false;
            // 读取客户端返回的实时进度、状态、日志
            while (await rpc.ResponseStream.MoveNext())
            {
                isHaveResponse = true;
                var responseStreamCurrent = rpc.ResponseStream.Current;
                
                // 更新Task
                task.Progress = responseStreamCurrent.Progress;
                task.Status   = (EumTaskType) responseStreamCurrent.Status;
                task.RunSpeed = speedResult.Timer.ElapsedMilliseconds.ConvertType(0);
                TaskUpdate.Update(task);
                
                // 设置下一次的执行时间，并更新
                taskGroup.NextAt = responseStreamCurrent.NextAt.ToTimestamps();
                TaskGroupUpdate.Update(taskGroup);
                
                // 如果有日志
                if (responseStreamCurrent.Log != null && !string.IsNullOrWhiteSpace(responseStreamCurrent.Log.Log))
                {
                    RunLogAdd.Add(task.TaskGroupId, task.Id, (LogLevel)responseStreamCurrent.Log.LogLevel, responseStreamCurrent.Log.Log);
                }
            }

            // 计算耗时、并保存Task
            task.RunSpeed = speedResult.Timer.ElapsedMilliseconds.ConvertType(0);
            if (!isHaveResponse && task.Status == EumTaskType.Working)
            {
                task.Status = EumTaskType.Fail;
                RunLogAdd.Add(task.TaskGroupId, task.Id, LogLevel.Warning, $"任务ID：{task.Id}，已调度，但客户端没有响应");
                IocManager.Logger<ClientNotifyGrpc>().LogWarning($"任务ID：{task.Id}，已调度，但客户端没有响应");
            }
            TaskUpdate.Save(task);
            speedResult.Dispose();
            return task;
        }
    }
}