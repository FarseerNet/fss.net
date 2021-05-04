using System.Threading.Tasks;
using FSS.Abstract.Entity.MetaInfo;
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
        
        /// <summary>
        /// 检查客户端是否离线
        /// </summary>
        public async Task<bool> Check(TaskVO task)
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
                    await TaskUpdate.SaveAsync(task);
                    return true;
                }
            }
            else // 如果不是，则判断服务器节点是否掉线
            {
                if (!NodeRegister.IsNodeExists(task.ServerNode)) // 服务端掉线
                {
                    await RunLogAdd.AddAsync(task.TaskGroupId, task.Id, LogLevel.Warning, $"任务ID：{task.Id}，服务端节点下线，强制设为失败状态");
                    task.Status = EumTaskType.Fail;
                    await TaskUpdate.SaveAsync(task);
                    return true;
                }
            }

            return false;
        }
    }
}