using System.Collections.Generic;
using FS.Cache;
using FS.Cache.Redis;
using FS.Extends;
using FSS.Abstract.Entity.MetaInfo;
using FSS.Abstract.Server.MetaInfo;
using FSS.Com.MetaInfoServer.Abstract;
using FSS.Com.MetaInfoServer.TaskGroup.Dal;

namespace FSS.Com.MetaInfoServer.TaskGroup
{
    /// <summary>
    /// 任务列表
    /// </summary>
    public class TaskGroupList : ITaskGroupList
    {
        public ITaskGroupAgent    TaskGroupAgent    { get; set; }
        public IRedisCacheManager RedisCacheManager { get; set; }

        /// <summary>
        /// 获取全部任务列表
        /// </summary>
        public List<TaskGroupVO> ToListAndSave()
        {
            var taskGroupVos = TaskGroupAgent.ToList().Map<TaskGroupVO>();
            RedisCacheManager.CacheManager.Save(TaskGroupCache.Key, taskGroupVos, o => o.Id);
            return taskGroupVos;
        }

        /// <summary>
        /// 获取全部任务列表
        /// </summary>
        public List<TaskGroupVO> ToList()
        {
            return RedisCacheManager.CacheManager.GetList(TaskGroupCache.Key,
                opt => TaskGroupAgent.ToList().Map<TaskGroupVO>()
                , o => o.Id);
        }

        /// <summary>
        /// 删除整个缓存
        /// </summary>
        public void Clear()
        {
            RedisCacheManager.CacheManager.Remove(TaskGroupCache.Key);
        }
    }
}