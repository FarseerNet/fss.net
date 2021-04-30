using System;
using FS.Cache;
using FS.Cache.Redis;
using FS.Extends;
using FSS.Abstract.Entity.MetaInfo;
using FSS.Abstract.Enum;
using FSS.Abstract.Server.MetaInfo;
using FSS.Com.MetaInfoServer.Abstract;
using FSS.Com.MetaInfoServer.Task.Dal;
using Newtonsoft.Json;

namespace FSS.Com.MetaInfoServer.Task
{
    public class TaskUpdate : ITaskUpdate
    {
        public ICacheManager    CacheManager    { get; set; }
        public ITaskAgent       TaskAgent       { get; set; }
        public ITaskGroupUpdate TaskGroupUpdate { get; set; }

        /// <summary>
        /// 更新Task
        /// </summary>
        public void Update(TaskVO task)
        {
            // 统计失败次数，按次数递增时间
            TaskGroupUpdate.StatFail(task);
            CacheManager.Save(TaskCache.Key, task, task.TaskGroupId, new CacheOption());
        }

        /// <summary>
        /// 保存Task
        /// </summary>
        public void Save(TaskVO task)
        {
            TaskAgent.Update(task.Id, task.Map<TaskPO>());
            Update(task);
        }
    }
}