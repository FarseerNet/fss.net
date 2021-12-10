using System.Threading.Tasks;
using FS.Cache;
using FSS.Abstract.Entity.MetaInfo;
using FSS.Abstract.Server.MetaInfo;
using FSS.Com.MetaInfoServer.TaskGroup.Dal;
using FSS.Domain.Tasks.TaskGroup.Entity;

namespace FSS.Com.MetaInfoServer.TaskGroup
{
    public class TaskGroupInfo : ITaskGroupInfo
    {
        public TaskGroupAgent TaskGroupAgent { get; set; }
        public TaskGroupCache TaskGroupCache { get; set; }

        /// <summary>
        /// 获取任务信息
        /// </summary>
        public Task<TaskGroupDO> ToInfoAsync(int id) => TaskGroupCache.ToEntityAsync(EumCacheStoreType.Redis, id);

    }
}