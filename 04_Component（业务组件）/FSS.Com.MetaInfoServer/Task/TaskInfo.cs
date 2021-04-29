using System;
using System.Linq;
using FS.Cache;
using FS.Extends;
using FSS.Abstract.Entity.MetaInfo;
using FSS.Abstract.Enum;
using FSS.Abstract.Server.MetaInfo;
using FSS.Com.MetaInfoServer.Abstract;
using FSS.Com.MetaInfoServer.Task.Dal;

namespace FSS.Com.MetaInfoServer.Task
{
    public class TaskInfo : ITaskInfo
    {
        public ITaskAgent    TaskAgent    { get; set; }
        public ICacheManager CacheManager { get; set; }
        public ITaskAdd      TaskAdd     { get; set; }

        /// <summary>
        /// 获取任务信息
        /// </summary>
        public TaskVO ToInfo(int id) => TaskAgent.ToEntity(id).Map<TaskVO>();

        /// <summary>
        /// 获取当前任务组的任务
        /// </summary>
        public TaskVO ToGroupTask(int taskGroupId)
        {
            return CacheManager.ToEntity(TaskCache.Key,
                taskGroupId.ToString(),
                o => TaskAdd.GetOrCreate(taskGroupId),
                o => o.TaskGroupId);
        }

        /// <summary>
        /// 计算任务的平均运行速度
        /// </summary>
        public int StatAvgSpeed(int taskGroupId)
        {
            var speedList = TaskAgent.ToSpeedList(taskGroupId);
            if (speedList.Count == 0) return 0;
            return speedList.Sum() / speedList.Count;
        }
    }
}