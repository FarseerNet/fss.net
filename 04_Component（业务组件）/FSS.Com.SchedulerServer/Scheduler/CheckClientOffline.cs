using System;
using System.Linq;
using System.Threading.Tasks;
using FS.DI;
using FSS.Abstract.Entity;
using FSS.Abstract.Entity.MetaInfo;
using FSS.Abstract.Enum;
using FSS.Abstract.Server.MetaInfo;
using FSS.Abstract.Server.RegisterCenter;
using FSS.Abstract.Server.Scheduler;
using FSS.Application.Log.Interface;
using Microsoft.Extensions.Logging;

namespace FSS.Com.SchedulerServer.Scheduler
{
    public class CheckClientOffline : ICheckClientOffline
    {
        public IClientRegister ClientRegister { get; set; }
        public ITaskUpdate     TaskUpdate     { get; set; }
        public ILogAddApp      LogAddApp      { get; set; }
        public ITaskGroupInfo  TaskGroupInfo  { get; set; }
        public ITaskList       TaskList       { get; set; }

        /// <summary>
        /// 检查客户端是否离线
        /// </summary>
        public async Task Check(TaskVO task)
        {
            if (task == null) return;
            var taskGroup = await TaskGroupInfo.ToInfoAsync(task.TaskGroupId);

            if (!taskGroup.IsEnable)
            {
                await LogAddApp.AddAsync(taskGroup, LogLevel.Warning, $"任务：{taskGroup.Id} {taskGroup.Caption} {taskGroup.JobName} 已被禁用，强制设为失败状态");
                task.Status = EumTaskType.Fail;
                await TaskUpdate.SaveFinishAsync(task, taskGroup);
                return;
            }


            // 客户端
            var client = await ClientRegister.ToInfoAsync(task.ClientId) ?? new ClientVO();

            // 客户端下线
            if (client.Id == 0 || (DateTime.Now - client.ActivateAt).TotalMinutes >= 1)
            {
                await LogAddApp.AddAsync(taskGroup, LogLevel.Warning, $"【客户端下线】{client.ActivateAt:yyyy-MM-dd HH:mm:ss}，任务：【{taskGroup.JobName}】 {taskGroup.Id} {taskGroup.Caption} ，强制设为失败状态");
                task.Status = EumTaskType.Fail;
                await TaskUpdate.SaveFinishAsync(task, taskGroup);
                return;
            }

            // 加个时间，来限制并发
            if (task.Status == EumTaskType.Scheduler && (DateTime.Now - task.StartAt).TotalSeconds < 5) return;
            if (task.Status == EumTaskType.Working   && (DateTime.Now - task.RunAt).TotalSeconds   < 5) return;

            // 测试客户端是否假死
            if (await CheckFeignDeath(client, taskGroup))
            {
                await LogAddApp.AddAsync(taskGroup, LogLevel.Warning, $"【客户端假死】{client.ActivateAt:yyyy-MM-dd HH:mm:ss}，强制下线客户端");
                await ClientRegister.RemoveAsync(client.Id);
                task.Status = EumTaskType.Fail;
                await TaskUpdate.SaveFinishAsync(task, taskGroup);
                return;
            }


            // 任务组活动时间大于1分钟、同时客户端活动时间大于1分钟，判定为客户端下线
            if ((DateTime.Now - taskGroup.ActivateAt).TotalMinutes >= 1) // 大于1分钟，才检查
            {
                await LogAddApp.AddAsync(taskGroup, LogLevel.Warning, $"【客户端假死】{client.ActivateAt:yyyy-MM-dd HH:mm:ss}，任务：【{taskGroup.JobName}】 {taskGroup.Id} {taskGroup.Caption} ，强制设为失败状态");
                task.Status = EumTaskType.Fail;
                await TaskUpdate.SaveFinishAsync(task, taskGroup);
                return;
            }
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
            var lstTask = await TaskList.ToGroupListAsync();
            lstTask = lstTask.FindAll(o => o.ClientId == client.Id && o.StartAt < DateTime.Now);
            if (lstTask.Count == 0) return false;

            // 全部处于调度、工作状态，说明客户端已经假死了
            return lstTask.All(o => o.Status is EumTaskType.Scheduler or EumTaskType.Working);
        }
    }
}