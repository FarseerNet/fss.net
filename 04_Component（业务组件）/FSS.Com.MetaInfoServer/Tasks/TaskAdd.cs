using System;
using System.Threading.Tasks;
using FS.Cache.Redis;
using FS.DI;
using FS.Extends;
using FSS.Abstract.Entity.MetaInfo;
using FSS.Abstract.Enum;
using FSS.Abstract.Server.MetaInfo;
using FSS.Com.MetaInfoServer.Abstract;
using FSS.Com.MetaInfoServer.Tasks.Dal;
using Microsoft.Extensions.Logging;

namespace FSS.Com.MetaInfoServer.Tasks
{
    public class TaskAdd : ITaskAdd
    {
        public ITaskAgent         TaskAgent         { get; set; }
        public IRedisCacheManager RedisCacheManager { get; set; }
        public ITaskGroupInfo     TaskGroupInfo     { get; set; }
        public ITaskGroupUpdate   TaskGroupUpdate   { get; set; }

        private static DateTime _lastUpdateGroupAt = DateTime.Now;

        /// <summary>
        /// 创建Task，并更新到缓存
        /// </summary>
        public async Task<TaskVO> GetOrCreateAsync(int taskGroupId)
        {
            TaskGroupVO taskGroup;
            // 如果上一次的更新时间超过5分钟，则重新取数据库的任务组
            if ((DateTime.Now - _lastUpdateGroupAt).TotalMinutes >= 1)
            {
                taskGroup          = await TaskGroupInfo.ToInfoByDbAsync(taskGroupId);
                _lastUpdateGroupAt = DateTime.Now;
            }
            else
                taskGroup = await TaskGroupInfo.ToInfoAsync(taskGroupId);

            if (taskGroup == null)
            {
                IocManager.Instance.Logger<TaskAdd>().LogWarning($"taskGroupId={taskGroupId}，这里查不到taskGroup");
            }

            var task = await TaskAgent.ToUnExecutedTaskAsync(taskGroup.Id);
            if (task == null)
            {
                // 没查到时，自动创建一条对应的Task
                task = new TaskPO
                {
                    TaskGroupId = taskGroup.Id,
                    StartAt     = taskGroup.NextAt,
                    RunSpeed    = 0,
                    ClientHost  = "",
                    ClientIp    = "",
                    Progress    = 0,
                    Status      = EumTaskType.None,
                    CreateAt    = DateTime.Now,
                    ServerNode  = ""
                };
                await TaskAgent.AddAsync(task);
                await TaskGroupUpdate.UpdateTaskIdAsync(taskGroup.Id, task.Id.GetValueOrDefault());
            }

            var taskVo = task.Map<TaskVO>();
            await RedisCacheManager.CacheManager.SaveAsync(TaskCache.Key, taskVo, taskVo.TaskGroupId);

            return taskVo;
        }
    }
}