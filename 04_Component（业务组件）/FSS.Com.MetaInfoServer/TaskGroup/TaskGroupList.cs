using System.Collections.Generic;
using FS.Cache;
using FS.Extends;
using FSS.Abstract.Entity.MetaInfo;
using FSS.Abstract.Server.MetaInfo;
using FSS.Com.MetaInfoServer.Abstract;

namespace FSS.Com.MetaInfoServer.TaskGroup
{
    /// <summary>
    /// 任务列表
    /// </summary>
    public class TaskGroupList : ITaskGroupList
    {
        public ITaskGroupAgent TaskGroupAgent { get; set; }
        public ITaskGroupCache TaskGroupCache { get; set; }
        public ICacheManager   CacheManager   { get; set; }

        /// <summary>
        /// 获取全部任务列表
        /// </summary>
        public List<TaskGroupVO> ToList()
        {
            return CacheManager.GetList("TaskGroup",
                opt => TaskGroupAgent.ToList().Map<TaskGroupVO>()
                , o => o.Id);
        }
    }
}