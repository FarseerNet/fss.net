using System;
using System.Linq;
using System.Threading.Tasks;
using FS.DI;
using FSS.Abstract.Entity;
using FSS.Abstract.Entity.MetaInfo;
using FSS.Abstract.Enum;
using FSS.Abstract.Server.MetaInfo;
using FSS.Abstract.Server.RegisterCenter;
using Microsoft.Extensions.Logging;

namespace FSS.Com.SchedulerServer.Scheduler
{
    public class CheckClientOffline : ISingletonDependency
    {
        public IClientRegister ClientRegister { get; set; }
        public ITaskUpdate     TaskUpdate     { get; set; }
        public IRunLogAdd      RunLogAdd      { get; set; }
        public ITaskGroupInfo  TaskGroupInfo  { get; set; }

        /// <summary>
        /// 检查客户端是否离线
        /// </summary>
        public async Task Check(TaskVO task)
        {
            var taskGroup = await TaskGroupInfo.ToInfoAsync(task.TaskGroupId);

            // 客户端
            var client = await ClientRegister.ToInfoAsync(task.ClientId) ?? new ClientVO();

            // 客户端下线
            if (client.Id == 0 || (DateTime.Now - client.ActivateAt).TotalMinutes >= 1)
            {
                await RunLogAdd.AddAsync(taskGroup, task.Id, LogLevel.Warning, $"任务ID：{task.Id}，客户端下线{taskGroup.ActivateAt:yyyy-MM-dd HH:mm:ss} {client.ActivateAt:yyyy-MM-dd HH:mm:ss}，强制设为失败状态");
                task.Status = EumTaskType.Fail;
                await TaskUpdate.SaveFinishAsync(task, taskGroup);
                return;
            }

            // 测试客户端是否假死
            //if (await CheckFeignDeath(client, taskGroup))
            //{
            //    await RunLogAdd.AddAsync(taskGroup, task.Id, LogLevel.Warning, $"检测到客户端的最后使用时间为：{client.ActivateAt:yyyy-MM-dd HH:mm:ss}，进入假死状态，强制下线客户端");
            //    await ClientRegister.RemoveAsync(client.Id);
            //    task.Status = EumTaskType.Fail;
            //    await TaskUpdate.SaveFinishAsync(task, taskGroup);
            //    return;
            //}


            // 任务组活动时间大于1分钟、同时客户端活动时间大于1分钟，判定为客户端下线
            if ((DateTime.Now - taskGroup.ActivateAt).TotalMinutes >= 1) // 大于1分钟，才检查
            {
                await RunLogAdd.AddAsync(taskGroup, task.Id, LogLevel.Warning, $"任务ID：{task.Id}，客户端假死状态{taskGroup.ActivateAt:yyyy-MM-dd HH:mm:ss} {client.ActivateAt:yyyy-MM-dd HH:mm:ss}，强制设为失败状态");
                task.Status = EumTaskType.Fail;
                await TaskUpdate.SaveFinishAsync(task, taskGroup);
                return;
            }

        }
    }
}