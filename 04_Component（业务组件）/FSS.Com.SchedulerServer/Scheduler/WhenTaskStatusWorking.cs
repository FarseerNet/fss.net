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
using Microsoft.Extensions.Logging;

namespace FSS.Com.SchedulerServer.Scheduler
{
    public class WhenTaskStatusWorking : IWhenTaskStatus
    {
        public static           bool            IsRun;
        public                  ITaskInfo       TaskInfo       { get; set; }
        public                  IClientRegister ClientRegister { get; set; }
        public                  ITaskGroupList  TaskGroupList  { get; set; }
        public                  ILogger         Logger         { get; set; }
        public                  IIocManager     IocManager     { get; set; }
        public                  ITaskUpdate     TaskUpdate     { get; set; }
        public                  IRunLogAdd      RunLogAdd      { get; set; }
        public                  INodeRegister   NodeRegister   { get; set; }
        private static readonly object          ObjLock = new();

        /// <summary>
        /// 运行当状态为Node的任务
        /// </summary>
        public Task Run()
        {
            Logger = IocManager.Logger<WhenTaskStatusWorking>();

            // 当前没有客户端连接时，休眠
            if (ClientRegister.Count() == 0)
            {
                Logger.LogDebug($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} 当前没有客户端连接，Working休眠...");
                return Task.FromResult(0);
            }

            if (IsRun) return Task.FromResult(0);
            lock (ObjLock)
            {
                if (IsRun) return Task.FromResult(0);
                IsRun = true;
            }

            ThreadPool.QueueUserWorkItem(async _ =>
            {
                while (ClientRegister.Count() > 0)
                {
                    IsRun = true;
                    try
                    {
                        var dicTaskGroup = (await TaskGroupList.ToListAndSaveAsync()).ToDictionary(o => o.Id, o => o);
                        var lstTask      = await TaskInfo.ToGroupListAsync();

                        // 注册进来的客户端，必须是能处理的，否则退出线程
                        var lstStatusWorking = lstTask.FindAll(o => ClientRegister.Count(dicTaskGroup[o.TaskGroupId].JobTypeName) > 0);
                        if (lstStatusWorking.Count == 0) return;

                        // 取出状态为None的，且马上到时间要处理的
                        lstStatusWorking = lstStatusWorking.FindAll(o =>
                                o.Status == EumTaskType.Working &&                       // 状态必须是 EumTaskType.None
                                (o.StartAt - DateTime.Now).TotalMilliseconds >= 10000 && // 执行时间在10s后的
                                dicTaskGroup[o.TaskGroupId].IsEnable)                    // 任务组必须是开启
                            .OrderBy(o => o.StartAt).ToList();

                        // 没有任务需要调度
                        if (lstStatusWorking.Count == 0)
                        {
                            await Task.Delay(5000);
                            continue;
                        }

                        foreach (var taskId in lstStatusWorking.Select(o => o.Id))
                        {
                            // 重新取一遍，担心正好数据被正确处理好了
                            var task = await TaskInfo.ToInfoAsync(taskId);

                            // 如果 任务的运行节点是当前节点时，判断客户端是否在线
                            var nodeIp = NodeRegister.GetNodeIp();
                            if (task.ServerNode == nodeIp)
                            {
                                var client = ClientRegister.ToInfo(task.ClientHost);
                                if (client == null) // 客户端掉线了
                                {
                                    await RunLogAdd.AddAsync(task.TaskGroupId, task.Id, LogLevel.Warning, $"任务ID：{task.Id}，客户端断开连接，强制设为失败状态");
                                    task.Status = EumTaskType.Fail;
                                    await TaskUpdate.SaveAsync(task);
                                    continue;
                                }
                            }
                            else // 如果不是，则判断服务器节点是否掉线
                            {
                                if (!NodeRegister.IsNodeExists(task.ServerNode)) // 服务端掉线
                                {
                                    await RunLogAdd.AddAsync(task.TaskGroupId, task.Id, LogLevel.Warning, $"任务ID：{task.Id}，服务端节点下线，强制设为失败状态");
                                    task.Status = EumTaskType.Fail;
                                    await TaskUpdate.SaveAsync(task);
                                    continue;
                                }
                            }

                            await Task.Delay(100);
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.LogError(e, e.Message);
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