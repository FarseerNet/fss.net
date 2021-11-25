using System;
using System.Threading.Tasks;
using FS.Cache.Redis;
using FS.Extends;
using FS.Utils.Component;
using FSS.Abstract.Entity.MetaInfo;
using FSS.Abstract.Enum;
using FSS.Abstract.Server.MetaInfo;
using FSS.Com.MetaInfoServer.Tasks.Dal;

namespace FSS.Com.MetaInfoServer.Tasks
{
    public class TaskUpdate : ITaskUpdate
    {
        public ITaskAdd         TaskAdd         { get; set; }
        public ITaskGroupUpdate TaskGroupUpdate { get; set; }
        public ITaskGroupInfo   TaskGroupInfo   { get; set; }
        public ITaskInfo        TaskInfo        { get; set; }
        public TaskCache        TaskCache       { get; set; }

        /// <summary>
        /// 更新Task（如果状态是成功、失败、重新调度，则应该调Save）
        /// </summary>
        public Task UpdateAsync(TaskVO task) => TaskCache.SaveAsync(task);

        /// <summary>
        /// 取消任务
        /// </summary>
        public async Task CancelTask(int groupId)
        {
            var task                      = await TaskInfo.ToInfoByGroupIdAsync(groupId);
            if (task != null) task.Status = EumTaskType.Fail;

            var taskGroup = await TaskGroupInfo.ToInfoAsync(groupId);
            // 这里不设置的话，下次执行时间，有可能还是将来的，导致如果设置错了时间的话，会一直等待原来设置错的时间
            taskGroup.NextAt = taskGroup.LastRunAt;

            await SaveFinishAsync(task, taskGroup);
        }

        /// <summary>
        /// 保存Task（taskGroup必须是最新的）
        /// </summary>
        public async Task SaveFinishAsync(TaskVO task, TaskGroupVO taskGroup)
        {
            // 本次的时间策略晚，则通过时间策略计算出来
            if (DateTime.Now > taskGroup.NextAt)
            {
                var cron = new Cron();
                // 时间间隔器
                if (taskGroup.IntervalMs > 0) taskGroup.NextAt = DateTime.Now.AddMilliseconds(taskGroup.IntervalMs);
                else if (!string.IsNullOrWhiteSpace(taskGroup.Cron) && cron.Parse(taskGroup.Cron))
                {
                    taskGroup.NextAt = cron.GetNext(DateTime.Now);
                }
                else // 没有找到设置下一次时间的设置，则默认30S执行一次
                {
                    taskGroup.NextAt = DateTime.Now.AddSeconds(30);
                }
            }

            // 将当前的Task写入Redis队列
            if (task != null) await TaskAdd.AddToQueueAsync(task);
            await TaskGroupUpdate.UpdateAsync(taskGroup);

            // 完成后，立即生成一个新的任务
            if (taskGroup.IsEnable)
            {
                await TaskAdd.GetOrCreateAsync(taskGroup);
            }
        }
    }
}