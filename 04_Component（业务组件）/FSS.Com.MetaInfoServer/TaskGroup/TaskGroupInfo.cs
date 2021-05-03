using System.Threading.Tasks;
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
        public Task<TaskGroupVO> ToInfoAsync(int id)
        {
            return RedisCacheManager.CacheManager.ToEntityAsync<TaskGroupVO>(TaskGroupCache.Key,
                id.ToString(),
                _ => TaskGroupAgent.ToEntityAsync(id).MapAsync<TaskGroupVO,TaskGroupPO>(),
                o => o.Id);
        }
    }
}