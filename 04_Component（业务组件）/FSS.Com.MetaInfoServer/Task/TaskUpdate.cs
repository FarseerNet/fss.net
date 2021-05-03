using System;
using FS.Cache;
using FS.Cache.Redis;
using FS.Extends;
using FS.Utils.Component;
using FSS.Abstract.Entity.MetaInfo;
using FSS.Abstract.Enum;
using FSS.Abstract.Server.MetaInfo;
using FSS.Com.MetaInfoServer.Abstract;
using FSS.Com.MetaInfoServer.Task.Dal;

namespace FSS.Com.MetaInfoServer.Task
{
    public class TaskUpdate : ITaskUpdate
    {
        public IRedisCacheManager RedisCacheManager { get; set; }
        public ITaskAgent         TaskAgent         { get; set; }
        public ITaskGroupUpdate   TaskGroupUpdate   { get; set; }
        public ITaskGroupInfo     TaskGroupInfo     { get; set; }

        /// <summary>
        /// 更新Task（如果状态是成功、失败、重新调度，则应该调Save）
        /// </summary>
        public void Update(TaskVO task)
        {
            // 统计失败次数，按次数递增时间
            TaskGroupUpdate.StatFail(task);
            RedisCacheManager.CacheManager.Save(TaskCache.Key, task, task.TaskGroupId, new CacheOption());
        }

        /// <summary>
        /// 保存Task
        /// </summary>
        public void Save(TaskVO task)
        {
            switch (task.Status)
            {
                case EumTaskType.Fail:
                case EumTaskType.Success:
                case EumTaskType.ReScheduler:

                    var taskGroup = TaskGroupInfo.ToInfo(task.TaskGroupId);
                    // 说明上一次任务，没有设置下一次的时间（动态设置）
                    // 本次的时间策略晚，则通过时间策略计算出来
                    if (DateTime.Now > taskGroup.NextAt)
                    {
                        var cron = new Cron();
                        // 时间间隔器
                        if (taskGroup.IntervalMs > 0) taskGroup.NextAt = DateTime.Now.AddMilliseconds(taskGroup.IntervalMs);
                        else if (string.IsNullOrWhiteSpace(taskGroup.Cron) is false && cron.Parse(taskGroup.Cron))
                        {
                            taskGroup.NextAt = cron.GetNext(DateTime.Now); // 未实现
                        }
                        else // 没有找到设置下一次时间的设置，则默认30S执行一次
                        {
                            taskGroup.NextAt = DateTime.Now.AddSeconds(30);
                        }

                        TaskGroupUpdate.Save(taskGroup);
                    }

                    break;
            }

            TaskAgent.Update(task.Id, task.Map<TaskPO>());
            Update(task);
        }
    }
}