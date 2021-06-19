using System;
using System.Diagnostics;
using System.Threading.Tasks;
using FS.DI;
using FS.Utils.Common;
using FSS.Abstract.Entity.MetaInfo;
using FSS.Abstract.Entity.RegisterCenter;
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
        public ISchedulerLock  SchedulerLock  { get; set; }
        public ITaskInfo       TaskInfo       { get; set; }
        ILogger                _logger;

        /// <summary>
        /// 调度
        /// </summary>
        public async Task Scheduler(TaskGroupVO taskGroup, TaskVO task)
        {
            _logger = IocManager.Logger<TaskScheduler>();
            try
            {
                // 取出空闲客户端、开始调度执行
                var clientVO = ClientSlb.Slb(taskGroup.JobName);
                if (clientVO == null)
                {
                    _logger.LogWarning($"任务：GroupId={taskGroup.Id} {taskGroup.Caption}-TaskId={task.Id} 需要在（{task.StartAt:yyyy-MM-dd HH:mm:ss}）执行，但没有找到可以调度的客户端");
                    return;
                }

                // 同一个任务，多个服务端，只能由一个节点执行调度
                if (SchedulerLock.TryLock(task.Id, clientVO.ServerHost))
                {
                    //logger.LogDebug($"任务：GroupId={taskGroup.Id} TaskId={task.Id} {taskGroup.Caption} 调度给====>{clientVO.ClientIp}");
                    await Schedule(taskGroup, task, clientVO);
                }
                else
                {
                    // 等待1S后，如果任务状态还是None，则删除锁
                    await Task.Delay(1000);
                    var newTask = await TaskInfo.ToInfoByGroupIdAsync(task.TaskGroupId);
                    if (task.Id == newTask.Id && newTask.Status == EumTaskType.None)
                    {
                        await SchedulerLock.ClearLock(newTask.Id);
                        await Schedule(taskGroup: taskGroup, task: task, clientVO: clientVO);
                        return;
                    }
                    _logger.LogWarning($"任务：GroupId={taskGroup.Id} {taskGroup.Caption}-TaskId={task.Id} ，已被调度。");
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
                await RunLogAdd.AddAsync(taskGroup, task.Id, LogLevel.Error, msg);
                await SchedulerLock.ClearLock(task.Id);
                _logger.LogError(msg);
            }
        }

        /// <summary>
        /// 更新任务信息并调度
        /// </summary>
        private async Task Schedule(TaskGroupVO taskGroup, TaskVO task, ClientConnectVO clientVO)
        {
            //Stopwatch sw = Stopwatch.StartNew();
            // 通知客户端处理JOB
            task.Status      = EumTaskType.Scheduler;
            task.ClientHost  = clientVO.ServerHost;
            task.ClientIp    = clientVO.ClientIp;
            task.ServerNode  = IpHelper.GetIp;
            task.SchedulerAt = DateTime.Now;
            await TaskUpdate.UpdateAsync(task);

            // 通知客户端开始任务调度
            await ClientResponse.JobSchedulerAsync(clientVO, taskGroup, task);
            //_logger.LogInformation($"统计：调度【{task.Caption} ({task.JobName})】耗时：{sw.ElapsedMilliseconds} ms");
        }
    }
}