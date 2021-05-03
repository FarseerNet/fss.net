using FS.Cache;
using FS.Cache.Redis;
using FS.Extends;
using FSS.Abstract.Entity.MetaInfo;
using FSS.Abstract.Server.MetaInfo;
using FSS.Com.MetaInfoServer.Abstract;
using FSS.Com.MetaInfoServer.TaskGroup.Dal;

namespace FSS.Com.MetaInfoServer.TaskGroup
{
    public class TaskGroupInfo : ITaskGroupInfo
    {
        public ITaskGroupAgent    TaskGroupAgent    { get; set; }
        public IRedisCacheManager RedisCacheManager { get; set; }

        /// <summary>
        /// 获取任务信息
        /// </summary>
        public TaskGroupVO ToInfo(int id)
        {
            return RedisCacheManager.CacheManager.ToEntity(TaskGroupCache.Key, 
                id.ToString(), 
                o=>TaskGroupAgent.ToEntity(id).Map<TaskGroupVO>(),
                o=>o.Id);
        }
    }
}