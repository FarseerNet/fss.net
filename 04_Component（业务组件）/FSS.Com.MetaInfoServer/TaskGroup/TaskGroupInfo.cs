using System.Threading.Tasks;
using FS.Cache;
using FS.Extends;
using FSS.Abstract.Entity.MetaInfo;
using FSS.Abstract.Server.MetaInfo;
using FSS.Com.MetaInfoServer.TaskGroup.Dal;

namespace FSS.Com.MetaInfoServer.TaskGroup
{
    public class TaskGroupInfo : ITaskGroupInfo
    {
        public TaskGroupAgent TaskGroupAgent { get; set; }
        public TaskGroupCache TaskGroupCache { get; set; }

        /// <summary>
        /// 获取任务信息
        /// </summary>
        public Task<TaskGroupVO> ToInfoAsync(int id) => TaskGroupCache.ToEntityAsync(EumCacheStoreType.Redis, id);

        /// <summary>
        /// 从数据库中取出并保存
        /// </summary>
        public async Task<TaskGroupVO> ToInfoByDbAsync(int id)
        {
            var entity = await TaskGroupAgent.ToEntityAsync(id).MapAsync<TaskGroupVO, TaskGroupPO>();
            await TaskGroupCache.SaveAsync(entity);
            return entity;
        }
    }
}