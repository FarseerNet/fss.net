using System;
using System.Linq;
using System.Threading.Tasks;
using FSS.Abstract.Entity.MetaInfo;
using FSS.Abstract.Entity.RegisterCenter;
using FSS.Abstract.Enum;
using FSS.Abstract.Server.MetaInfo;
using FSS.Abstract.Server.RegisterCenter;
using FSS.Com.SchedulerServer.Abstract;
using Microsoft.Extensions.Logging;

namespace FSS.Com.SchedulerServer.Scheduler
{
    public class CheckClientOffline : ICheckClientOffline
    {
        public IClientRegister ClientRegister { get; set; }
        public ITaskUpdate     TaskUpdate     { get; set; }
        public IRunLogAdd      RunLogAdd      { get; set; }
        public INodeRegister   NodeRegister   { get; set; }
        public ITaskGroupInfo  TaskGroupInfo  { get; set; }
        public ITaskInfo       TaskInfo       { get; set; }

        /// <summary>
        /// 检查客户端是否离线
        /// </summary>
        public async Task<bool> Check(TaskVO task, TaskGroupVO taskGroupVO)
        {
            // 如果 任务的运行节点是当前节点时，判断客户端是否在线
            var nodeIp = NodeRegister.GetNodeIp();
            if (task.ServerNode == nodeIp)
            {
                var client = ClientRegister.ToInfo(task.ClientHost);
                if (client == null) // 客户端掉线了
                {
                    await RunLogAdd.AddAsync(task.TaskGroupId, task.Id, LogLevel.Warning, $"任务ID：{task.Id}，客户端断开连接，强制设为失败状态");
                    task.Status = EumTaskType.Fail;
                    await TaskUpdate.SaveAsync(task, taskGroupVO);
                    return true;
                }

                // 测试客户端是否假死
                if (await CheckFeignDeath(task, client))
                {
                    await RunLogAdd.AddAsync(task.TaskGroupId, task.Id, LogLevel.Warning, $"检测到客户端的最后使用时间为：{client.UseAt:yyyy-MM-dd HH:mm:ss}，进入假死状态，强制下线客户端");
                    await ClientRegister.RemoveAsync(client.ServerHost);
                    return true;
                }

                return false;
            }

            // 判断服务器节点是否掉线
            if (!NodeRegister.IsNodeExists(task.ServerNode)) // 服务端掉线
            {
                await RunLogAdd.AddAsync(task.TaskGroupId, task.Id, LogLevel.Warning, $"任务ID：{task.Id}，服务端节点下线，强制设为失败状态");
                task.Status = EumTaskType.Fail;
                await TaskUpdate.SaveAsync(task, taskGroupVO);
                return true;
            }

            // 客户端是否掉线（客户端的注册不在当前节点）
            if ((DateTime.Now - task.StartAt).TotalMinutes >= 1 && ! await ClientRegister.IsExistsByRedis(task.ClientHost)) // 大于1分钟，才检查
            {
                await RunLogAdd.AddAsync(task.TaskGroupId, task.Id, LogLevel.Warning, $"任务ID：{task.Id}，客户端假死状态，强制设为失败状态");
                task.Status = EumTaskType.Fail;
                await TaskUpdate.SaveAsync(task, taskGroupVO);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 检查客户端是否假死（客户端2倍于平时耗时时间未使用，且该客户端关联的所有任务，全部处于调度、工作状态）
        /// </summary>
        private async Task<bool> CheckFeignDeath(TaskVO task, ClientConnectVO client)
        {
            var taskGroupVO = await TaskGroupInfo.ToInfoAsync(task.TaskGroupId);
            var timeout     = taskGroupVO.RunSpeedAvg * 2.5;
            // 如果时间小于5分钟的，则按5分钟来判定
            var minTimeout                    = TimeSpan.FromMinutes(5).TotalMilliseconds;
            if (timeout < minTimeout) timeout = minTimeout;

            // 距离上一次的调度，在超时范围内的，不作假死判断
            if ((DateTime.Now - client.UseAt).TotalMilliseconds < timeout) return false;

            // 找出当前客户端对应的所有任务、并且执行时间 已经到了
            var lstTask = await TaskInfo.ToGroupListAsync();
            lstTask = lstTask.FindAll(o => o.ClientHost == client.ServerHost && o.StartAt < DateTime.Now); //client.Jobs.Contains(o.JobName)
            if (lstTask == null || lstTask.Count == 0) return false;

            // 全部处于调度、工作状态，说明客户端已经假死了
            return lstTask.All(o => o.Status is EumTaskType.Scheduler or EumTaskType.Working);
        }
    }
}