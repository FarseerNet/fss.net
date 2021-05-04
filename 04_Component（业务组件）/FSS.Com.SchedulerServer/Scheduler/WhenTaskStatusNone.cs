using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FS.DI;
using FSS.Abstract.Enum;
using FSS.Abstract.Server.MetaInfo;
using FSS.Abstract.Server.RegisterCenter;
using FSS.Abstract.Server.RemoteCall;
using FSS.Abstract.Server.Scheduler;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace FSS.Com.SchedulerServer.Scheduler
{
    public class WhenTaskStatusNone : IWhenTaskStatus
    {
        public static           bool            IsRun;
        public                  ITaskInfo       TaskInfo       { get; set; }
        public                  IClientRegister ClientRegister { get; set; }
        public                  ITaskGroupList  TaskGroupList  { get; set; }
        public                  ILogger         Logger         { get; set; }
        public                  IIocManager     IocManager     { get; set; }
        public                  ITaskUpdate     TaskUpdate     { get; set; }
        public                  IClientSlb      ClientSlb      { get; set; }
        public                  IRunLogAdd      RunLogAdd      { get; set; }
        public                  IClientResponse ClientResponse { get; set; }
        public                  INodeRegister   NodeRegister   { get; set; }
        public                  ISchedulerLock  SchedulerLock  { get; set; }
        private static readonly object          ObjLock = new();
        /// <summary>
        /// 运行当状态为Node的任务
        /// </summary>
        public Task Run()
        {
            Logger = IocManager.Logger<WhenTaskStatusNone>();

            // 当前没有客户端连接时，休眠
            if (ClientRegister.Count() == 0)
            {
                Logger.LogDebug($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} 当前没有客户端连接，None休眠...");
                return Task.FromResult(0);
            }

            if (IsRun) return Task.FromResult(0);
            lock (ObjLock)
            {
                if (IsRun) return Task.FromResult(0);
                IsRun = true;
            }

            IocManager.Resolve<IWhenTaskStatus>("Scheduler").Run();
            IocManager.Resolve<IWhenTaskStatus>("Working").Run();
            IocManager.Resolve<IWhenTaskStatus>("Finish").Run();
            ThreadPool.QueueUserWorkItem(async _ =>
            {
                while (ClientRegister.Count() > 0)
                {
                    IsRun = true;
                    var dicTaskGroup = (await TaskGroupList.ToListAndSaveAsync()).ToDictionary(o => o.Id, o => o);
                    var lstTask      = await TaskInfo.ToGroupListAsync();

                    // 注册进来的客户端，必须是能处理的，否则退出线程
                    var lstStatusNone = lstTask.FindAll(o => ClientRegister.Count(dicTaskGroup[o.TaskGroupId].JobTypeName) > 0);
                    if (lstStatusNone == null || lstStatusNone.Count == 0) return;

                    // 取出状态为None的，且马上到时间要处理的
                    lstStatusNone = lstStatusNone.FindAll(o =>
                            o.Status == EumTaskType.None &&                       // 状态必须是 EumTaskType.None
                            (o.StartAt - DateTime.Now).TotalMilliseconds <= 50 && // 执行时间在50ms内
                            dicTaskGroup[o.TaskGroupId].IsEnable)                 // 任务组必须是开启
                        .OrderBy(o => o.StartAt).ToList();

                    // 没有任务需要调度
                    if (lstStatusNone == null || lstStatusNone.Count == 0)
                    {
                        await Task.Delay(500);
                        continue;
                    }

                    foreach (var task in lstStatusNone)
                    {
                        var taskGroup = dicTaskGroup[task.TaskGroupId];
                        var timeSpan  = task.StartAt - DateTime.Now;
                        // 任务没开始，休眠
                        if (timeSpan.TotalMilliseconds > 0)
                        {
                            Logger.LogDebug($"执行任务：GroupId={taskGroup.Id} {taskGroup.Caption} taskId={task.Id}，还需要等待 {timeSpan.TotalMilliseconds} ms，休眠中...");
                            await Task.Delay((int) timeSpan.TotalMilliseconds);
                        }

                        try
                        {
                            // 取出空闲客户端、开始调度执行
                            var clientVO = ClientSlb.Slb(taskGroup.JobTypeName);

                            // 当前没有客户端注册进来
                            if (clientVO == null)
                            {
                                IocManager.Logger<TaskGroupScheduler>().LogWarning($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} 任务：GroupId={taskGroup.Id} {taskGroup.Caption}-TaskId={task.Id} 需要在（{task.StartAt:yyyy-MM-dd HH:mm:ss}）执行，但没有找到可以调度的客户端");
                                continue;
                            }

                            // 同一个任务，多个服务端，只能由一个节点执行调度
                            if (SchedulerLock.TryLock(task.Id, clientVO.ServerHost))
                            {
                                IocManager.Logger<TaskGroupScheduler>().LogInformation($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} 任务：GroupId={taskGroup.Id} TaskId={task.Id} {taskGroup.Caption} 调度给====>{clientVO.ClientIp}");

                                // 通知客户端处理JOB
                                task.Status      = EumTaskType.Scheduler;
                                task.ClientHost  = clientVO.ServerHost;
                                task.ClientIp    = clientVO.ClientIp;
                                task.ServerNode  = NodeRegister.GetNodeIp();
                                task.SchedulerAt = DateTime.Now;
                                await TaskUpdate.UpdateAsync(task);

                                // 通知客户端开始任务调度
                                await ClientResponse.JobSchedulerAsync(clientVO, taskGroup, task);
                            }
                        }
                        catch (Exception e) // 通知失败
                        {
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
                            await TaskUpdate.SaveAsync(task);
                            await RunLogAdd.AddAsync(task.TaskGroupId, task.Id, LogLevel.Error, msg);
                            Thread.Sleep(3000);
                        }

                        // 休眠下，防止CPU过高
                        await Task.Delay(10);
                    }

                    // 休眠下，防止CPU过高
                    await Task.Delay(100);
                }

                IsRun = false;
            });
            return Task.FromResult(0);
        }
    }
}