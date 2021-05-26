using System;
using System.Threading.Tasks;
using FS.DI;
using FS.Utils.Common;
using FSS.Abstract.Entity.MetaInfo;
using FSS.Abstract.Enum;
using FSS.Abstract.Server.MetaInfo;
using FSS.Abstract.Server.RegisterCenter;
using FSS.Abstract.Server.RemoteCall;
using FSS.Abstract.Server.Scheduler;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace FSS.Com.SchedulerServer.Scheduler
{
    public class TaskScheduler : ITaskScheduler
    {
        public IIocManager     IocManager     { get; set; }
        public ITaskUpdate     TaskUpdate     { get; set; }
        public IClientSlb      ClientSlb      { get; set; }
        public IRunLogAdd      RunLogAdd      { get; set; }
        public IClientResponse ClientResponse { get; set; }
        public INodeRegister   NodeRegister   { get; set; }
        public ISchedulerLock  SchedulerLock  { get; set; }

        /// <summary>
        /// 调度
        /// </summary>
        public async Task Scheduler(TaskGroupVO taskGroup, TaskVO task)
        {
            var logger = IocManager.Logger<TaskScheduler>();
            try
            {
                // 取出空闲客户端、开始调度执行
                var clientVO = ClientSlb.Slb(taskGroup.JobName);
                if (clientVO == null)
                {
                    logger.LogWarning($"任务：GroupId={taskGroup.Id} {taskGroup.Caption}-TaskId={task.Id} 需要在（{task.StartAt:yyyy-MM-dd HH:mm:ss}）执行，但没有找到可以调度的客户端");
                    return;
                }

                // 同一个任务，多个服务端，只能由一个节点执行调度
                if (SchedulerLock.TryLock(task.Id, clientVO.ServerHost))
                {
                    logger.LogInformation($"任务：GroupId={taskGroup.Id} TaskId={task.Id} {taskGroup.Caption} 调度给====>{clientVO.ClientIp}");

                    // 通知客户端处理JOB
                    task.Status     = EumTaskType.Scheduler;
                    task.ClientHost = clientVO.ServerHost;
                    task.ClientIp   = clientVO.ClientIp;
                    task.ServerNode = IpHelper.GetIp;
                    task.SchedulerAt = DateTime.Now;
                    await TaskUpdate.UpdateAsync(task);

                    // 通知客户端开始任务调度
                    await ClientResponse.JobSchedulerAsync(clientVO, taskGroup, task);
                }
                else
                {
                    logger.LogWarning($"任务：GroupId={taskGroup.Id} {taskGroup.Caption}-TaskId={task.Id} ，已被调度。");
                }
            }
            catch (Exception e) // 通知失败
            {
                await SchedulerLock.ClearLock(task.Id);
                if (e.InnerException != null) e = e.InnerException;

                var statusCode = StatusCode.Unknown;
                var msg        = e.Message;
                if (e is RpcException rpcException1)
                {
                    msg        = rpcException1.Status.Detail;
                    statusCode = rpcException1.Status.StatusCode;
                }

                // 通知失败，则把当前任务设为失败
                task.Status = EumTaskType.Fail;
                await TaskUpdate.SaveAsync(task, taskGroup);
                await RunLogAdd.AddAsync(task.TaskGroupId, task.Id, LogLevel.Error, msg);
            }
        }
    }
}