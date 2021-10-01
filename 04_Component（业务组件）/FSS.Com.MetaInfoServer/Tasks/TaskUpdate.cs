using System;
using System.Threading.Tasks;
using FS.Cache.Redis;
using FS.Extends;
using FS.Utils.Component;
using FSS.Abstract.Entity.MetaInfo;
using FSS.Abstract.Server.MetaInfo;
using FSS.Com.MetaInfoServer.Tasks.Dal;
using FSS.Infrastructure.Repository;

namespace FSS.Com.MetaInfoServer.Tasks
{
    public class TaskUpdate : ITaskUpdate
    {
        public TaskAgent        TaskAgent       { get; set; }
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
        /// 移除缓存
        /// </summary>
        public async Task ClearCacheAsync()
        {
            await CacheKeys.TaskForGroupClear();
            await TaskInfo.ToGroupListAsync();
        }

        /// <summary>
        /// 保存Task
        /// </summary>
        public async Task SaveAsync(TaskVO task)
        {
            await UpdateAsync(task);
            await TaskAgent.UpdateAsync(task.Id, task.Map<TaskPO>());
        }

        /// <summary>
        /// 保存Task（taskGroup必须是最新的）
        /// </summary>
        public async Task SaveFinishAsync(TaskVO task)
        {
            var taskGroup = await TaskGroupInfo.ToInfoAsync(task.TaskGroupId);
            await SaveFinishAsync(task, taskGroup);
        }

        /// <summary>
        /// 保存Task（taskGroup必须是最新的）
        /// </summary>
        public async Task SaveFinishAsync(TaskVO task, TaskGroupVO taskGroup)
        {
            // 要先保存状态
            await SaveAsync(task);

            // 说明上一次任务，没有设置下一次的时间（动态设置）
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

            await TaskGroupUpdate.UpdateAsync(taskGroup);

            if (taskGroup.IsEnable)
            {
                // 完成后，立即生成一个新的任务
                await TaskAdd.GetOrCreateAsync(taskGroup);
            }
        }
    }
}