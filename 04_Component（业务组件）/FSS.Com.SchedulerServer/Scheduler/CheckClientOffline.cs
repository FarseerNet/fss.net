using System;
using System.Linq;
using System.Threading.Tasks;
using FS.Utils.Common;
using FSS.Abstract.Entity;
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
        public ITaskGroupInfo  TaskGroupInfo  { get; set; }
        public ITaskInfo       TaskInfo       { get; set; }

        /// <summary>
        /// 检查客户端是否离线
        /// </summary>
        public async Task<bool> Check(int groupId)
        {
            var taskGroup = await TaskGroupInfo.ToInfoAsync(groupId);
            var task      = await TaskInfo.ToInfoByGroupIdAsync(groupId);
            // 客户端
            var client = await ClientRegister.ToInfo(task.ClientId) ?? new ClientVO();

            // 测试客户端是否假死
            if (await CheckFeignDeath(client, taskGroup))
            {
                await RunLogAdd.AddAsync(taskGroup, task.Id, LogLevel.Warning, $"检测到客户端的最后使用时间为：{client.ActivateAt:yyyy-MM-dd HH:mm:ss}，进入假死状态，强制下线客户端");
                await ClientRegister.RemoveAsync(client.Id);
                return true;
            }


            // 任务组活动时间大于1分钟、同时客户端活动时间大于1分钟，判定为客户端下线
            if ((DateTime.Now - taskGroup.ActivateAt).TotalMinutes >= 1 && (client == null || client.Id == 0 || (DateTime.Now - client.ActivateAt).TotalMinutes >= 1)) // 大于1分钟，才检查
            {
                await RunLogAdd.AddAsync(taskGroup, task.Id, LogLevel.Warning, $"任务ID：{task.Id}，客户端假死状态{taskGroup.ActivateAt:yyyy-MM-dd HH:mm:ss} {client.ActivateAt:yyyy-MM-dd HH:mm:ss}，强制设为失败状态");
                task.Status = EumTaskType.Fail;
                // 取最新的任务组（不能用本地缓存的）
                taskGroup = await TaskGroupInfo.ToInfoAsync(task.TaskGroupId);
                await TaskUpdate.SaveFinishAsync(task, taskGroup);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 检查客户端是否假死（客户端2倍于平时耗时时间未使用，且该客户端关联的所有任务，全部处于调度、工作状态）
        /// </summary>
        private async Task<bool> CheckFeignDeath(ClientVO client, TaskGroupVO taskGroup)
        {
            var timeout = taskGroup.RunSpeedAvg * 2.5;
            // 如果时间小于5分钟的，则按5分钟来判定
            var minTimeout                    = TimeSpan.FromMinutes(5).TotalMilliseconds;
            if (timeout < minTimeout) timeout = minTimeout;

            // 距离上一次的调度，在超时范围内的，不作假死判断
            if ((DateTime.Now - client.ActivateAt).TotalMilliseconds < timeout) return false;

            // 找出当前客户端对应的所有任务、并且执行时间 已经到了
            var lstTask = await TaskInfo.ToGroupListAsync();
            lstTask = lstTask.FindAll(o => o.ClientId == client.Id && o.StartAt < DateTime.Now);
            if (lstTask.Count == 0) return false;

            // 全部处于调度、工作状态，说明客户端已经假死了
            return lstTask.All(o => o.Status is EumTaskType.Scheduler or EumTaskType.Working);
        }
    }
}