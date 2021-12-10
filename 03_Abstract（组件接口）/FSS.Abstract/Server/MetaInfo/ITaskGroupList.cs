using System.Collections.Generic;
using System.Threading.Tasks;
using FS.Cache;
using FS.DI;
using FSS.Abstract.Entity.MetaInfo;

namespace FSS.Abstract.Server.MetaInfo
{
    public interface ITaskGroupList: ISingletonDependency
    {
        /// <summary>
        /// 获取全部任务列表
        /// </summary>
        Task<List<TaskGroupDO>> ToListInCacheAsync(EumCacheStoreType cacheStoreType = EumCacheStoreType.Redis);

        /// <summary>
        /// 获取全部任务列表（数据库）
        /// </summary>
        Task<List<TaskGroupDO>> ToListInDbAsync();
        /// <summary>
        /// 获取任务组数量
        /// </summary>
        Task<long> Count();
    }
}