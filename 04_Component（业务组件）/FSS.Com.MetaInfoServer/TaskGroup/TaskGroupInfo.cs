using FS.Cache;
using FS.Extends;
using FSS.Abstract.Entity.MetaInfo;
using FSS.Abstract.Server.MetaInfo;
using FSS.Com.MetaInfoServer.Abstract;
using FSS.Com.MetaInfoServer.TaskGroup.Dal;

namespace FSS.Com.MetaInfoServer.TaskGroup
{
    public class TaskGroupInfo : ITaskGroupInfo
    {
        public ITaskGroupAgent TaskGroupAgent { get; set; }
        public ICacheManager   CacheManager   { get; set; }

        /// <summary>
        /// 获取任务信息
        /// </summary>
        public TaskGroupVO ToInfo(int id)
        {
            return CacheManager.ToEntity(TaskGroupCache.Key, 
                id.ToString(), 
                o=>TaskGroupAgent.ToEntity(id).Map<TaskGroupVO>(),
                o=>o.Id);
        }
    }
}