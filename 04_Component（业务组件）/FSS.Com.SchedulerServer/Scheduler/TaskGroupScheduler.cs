using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FS.DI;
using FS.Extends;
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
    public class TaskGroupScheduler : ITaskGroupScheduler
    {
        public IIocManager     IocManager     { get; set; }
        public ITaskInfo       TaskInfo       { get; set; }
        public ITaskGroupInfo  TaskGroupInfo  { get; set; }
        public ITaskUpdate     TaskUpdate     { get; set; }
        public IClientSlb      ClientSlb      { get; set; }
        public IClientRegister ClientRegister { get; set; }
        public ITaskAdd        TaskAdd        { get; set; }
        public IRunLogAdd      RunLogAdd      { get; set; }
        public IClientResponse ClientResponse { get; set; }
        public INodeRegister   NodeRegister   { get; set; }
        public ISchedulerLock  SchedulerLock  { get; set; }

        // 当前任务组是否有任务在运行
        private static readonly Dictionary<int, TaskVO> dicTaskGroupIsRun = new();

        /// <summary>
        /// 根据任务组ID，进行任务调度
        /// </summary>
        /// <param name="taskGroupId">任务组ID</param>
        public void SchedulerTaskGroup(int taskGroupId)
        {
            // 如果已有该任务组的时间器，才不需要再执行
            if (dicTaskGroupIsRun.ContainsKey(taskGroupId)) return;
            var logger = IocManager.Logger<TaskGroupScheduler>();

            // 开启单个任务组线程，判断是否要创建任务
            ThreadPool.QueueUserWorkItem(async taskGroupIdState =>
            {
                var tGroupId = (int) taskGroupIdState;
                // 取出任务组
                var taskGroup = TaskGroupInfo.ToInfo(tGroupId);

                var nextSeconds  = (taskGroup.NextAt - DateTime.Now).TotalSeconds.ConvertType(0);
                var nextTimeDesc = nextSeconds > 0 ? nextSeconds.ToString() + " 秒" : $"立即";
                logger.LogInformation($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} 开始运行调度线程：任务组：GroupId={taskGroup.Id} Caption={taskGroup.Caption} 下次执行时间：{nextTimeDesc}");

                while (true)
                {
                    try
                    {
                        // 当前没有客户端数量
                        if (ClientRegister.Count(taskGroup.JobTypeName) == 0)
                        {
                            logger.LogDebug($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} 任务组：GroupId={taskGroup.Id} {taskGroup.Caption} 当前没有客户端连接，调度休眠...");
                            Thread.Sleep(30 * 1000);
                            continue;
                        }

                        if (taskGroup.IsEnable is false)
                        {
                            logger.LogDebug($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} 任务组：GroupId={taskGroup.Id} {taskGroup.Caption} 没有启用，调度休眠...");
                            await Task.Delay(30 * 1000);
                        }

                        // 取出当前任务组的Task
                        dicTaskGroupIsRun[tGroupId] = TaskInfo.ToGroupTask(tGroupId);

                        logger.LogDebug($"1、GroupId={taskGroup.Id} {taskGroup.Caption} TaskId={dicTaskGroupIsRun[tGroupId].Id}  Status={dicTaskGroupIsRun[tGroupId].Status}");

                        // 执行调度
                        if (dicTaskGroupIsRun[tGroupId].Status is EumTaskType.None)
                        {
                            await SchedulerTask(taskGroup, dicTaskGroupIsRun[tGroupId]);
                            Thread.Sleep(100);
                        }

                        // 处于调度状态
                        if (dicTaskGroupIsRun[tGroupId].Status is EumTaskType.Scheduler)
                        {
                            CheckSchedulerStatus(taskGroup);
                        }

                        // 工作状态
                        if (dicTaskGroupIsRun[tGroupId].Status is EumTaskType.Working)
                        {
                            // 如果 任务的运行节点是当前节点时，判断客户端是否在线
                            var nodeIp = NodeRegister.GetNodeIp();
                            if (dicTaskGroupIsRun[tGroupId].ServerNode == nodeIp)
                            {
                                var client = ClientRegister.ToInfo(dicTaskGroupIsRun[tGroupId].ClientHost);
                                if (client == null) // 客户端掉线了
                                {
                                    RunLogAdd.Add(tGroupId, dicTaskGroupIsRun[tGroupId].Id, LogLevel.Warning, $"任务ID：{dicTaskGroupIsRun[tGroupId].Id}，客户端断开连接，强制设为失败状态");
                                    dicTaskGroupIsRun[tGroupId].Status = EumTaskType.Fail;
                                    TaskUpdate.Save(dicTaskGroupIsRun[tGroupId]);
                                }
                            }
                            else // 如果不是，则判断服务器节点是否掉线
                            {
                                if (!NodeRegister.IsNodeExists(dicTaskGroupIsRun[tGroupId].ServerNode)) // 服务端掉线
                                {
                                    RunLogAdd.Add(tGroupId, dicTaskGroupIsRun[tGroupId].Id, LogLevel.Warning, $"任务ID：{dicTaskGroupIsRun[tGroupId].Id}，服务端节点下线，强制设为失败状态");
                                    dicTaskGroupIsRun[tGroupId].Status = EumTaskType.Fail;
                                    TaskUpdate.Save(dicTaskGroupIsRun[tGroupId]);
                                }
                            }

                            Thread.Sleep(50);
                        }

                        switch (dicTaskGroupIsRun[tGroupId].Status)
                        {
                            case EumTaskType.Fail:
                            case EumTaskType.Success:
                            case EumTaskType.ReScheduler:
                            {
                                dicTaskGroupIsRun[tGroupId] = TaskAdd.GetOrCreate(tGroupId);
                                logger.LogDebug($"\t1、新建任务: GroupId={taskGroup.Id} {taskGroup.Caption} TaskId={dicTaskGroupIsRun[tGroupId].Id}");
                                break;
                            }
                        }

                        Thread.Sleep(10);
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e.Message);
                    }
                }
            }, taskGroupId);
        }

        /// <summary>
        /// 一直处于调度状态时，要注意是否客户端断开链接、或同步JOB状态时有异常
        /// </summary>
        private void CheckSchedulerStatus(TaskGroupVO taskGroup)
        {
            var logger = IocManager.Logger<TaskGroupScheduler>();
            while (dicTaskGroupIsRun[taskGroup.Id].Status is EumTaskType.Scheduler)
            {
                logger.LogDebug($"2、GroupId={taskGroup.Id} {taskGroup.Caption}、{dicTaskGroupIsRun[taskGroup.Id].Id}、{dicTaskGroupIsRun[taskGroup.Id].Status}");
                var newTask = TaskInfo.ToGroupTask(taskGroup.Id);

                // 不相等，说明已经执行了新的Task
                if (dicTaskGroupIsRun[taskGroup.Id].Id != newTask.Id) break;
                dicTaskGroupIsRun[taskGroup.Id] = newTask;

                // 说明已调度成功
                if (dicTaskGroupIsRun[taskGroup.Id].Status != EumTaskType.Scheduler) break;

                // 处于Scheduler状态，如果时间>2S，认为客户端无法处理当前JOB，重新调度
                var taskTimeSpan = DateTime.Now - dicTaskGroupIsRun[taskGroup.Id].SchedulerAt;
                if (taskTimeSpan.TotalMilliseconds > 2000)
                {
                    RunLogAdd.Add(taskGroup.Id, newTask.Id, LogLevel.Warning, $"任务ID：{dicTaskGroupIsRun[taskGroup.Id].Id}，已调度，{(int) taskTimeSpan.TotalMilliseconds} ms未执行，重新调度");

                    // 标记为重新调度
                    newTask.Status = EumTaskType.ReScheduler;
                    TaskUpdate.Save(newTask);
                    break;
                }

                Thread.Sleep(200);
            }
        }

        /// <summary>
        /// 执行调度
        /// </summary>
        private async Task SchedulerTask(TaskGroupVO taskGroup, TaskVO task)
        {
            var logger = IocManager.Logger<TaskGroupScheduler>();
            var taskVO = task;

            var timeSpan = task.StartAt - DateTime.Now;

            // 任务没开始，休眠
            if (timeSpan.TotalMilliseconds > 0)
            {
                logger.LogDebug($"执行任务：GroupId={taskGroup.Id} {taskGroup.Caption} taskId={task.Id}，还需要等待 {timeSpan.TotalMilliseconds} ms，休眠中...");
                Thread.Sleep((int) timeSpan.TotalMilliseconds);
            }

            try
            {
                // 取出空闲客户端、开始调度执行
                var clientVO = ClientSlb.Slb(taskGroup.JobTypeName);
                // 当前没有客户端注册进来
                if (clientVO == null)
                {
                    IocManager.Logger<TaskGroupScheduler>().LogWarning($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} 任务：GroupId={taskGroup.Id} {taskGroup.Caption}-TaskId={taskVO.Id} 需要在（{taskVO.StartAt:yyyy-MM-dd HH:mm:ss}）执行，但没有找到可以调度的客户端");
                    Thread.Sleep(5000);
                    return;
                }

                IocManager.Logger<TaskGroupScheduler>().LogInformation($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} 任务：GroupId={taskGroup.Id} {taskGroup.Caption}-TaskId={taskVO.Id} 调度给====>{clientVO.ServerHost}");

                if (SchedulerLock.TryLock(task.Id, clientVO.ServerHost))
                {
                    // 通知客户端处理JOB
                    task.Status     = EumTaskType.Scheduler;
                    task.ClientHost = clientVO.ServerHost;
                    task.ClientIp   = clientVO.ClientIp;
                    task.ServerNode = NodeRegister.GetNodeIp();
                    TaskUpdate.Update(task);

                    // 通知客户端开始任务调度
                    await ClientResponse.JobSchedulerAsync(clientVO, taskGroup, task);

                    // 更新调度时间
                    task.SchedulerAt = DateTime.Now;
                    TaskUpdate.Update(task);
                }
                else
                {
                    // 被其他节点锁定了
                    Thread.Sleep(50);
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

                IocManager.Logger<TaskGroupScheduler>().LogError(msg);
                Thread.Sleep(3000);
            }
        }
    }
}