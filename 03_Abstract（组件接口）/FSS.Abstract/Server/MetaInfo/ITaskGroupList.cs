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
        Task<List<TaskGroupVO>> ToListInCacheAsync(EumCacheStoreType cacheStoreType = EumCacheStoreType.Redis);

        /// <summary>
        /// 获取全部任务列表
        /// </summary>
        Task<List<TaskGroupVO>> ToListAndSaveAsync();

        /// <summary>
        /// 本地缓存获取任务组
        /// </summary>
        Task<Dictionary<int, TaskGroupVO>> ToListInMemoryAsync();

        /// <summary>
        /// 获取全部任务列表（数据库）
        /// </summary>
        Task<List<TaskGroupVO>> ToListInDbAsync();
        /// <summary>
        /// 获取任务组数量
        /// </summary>
        Task<long> Count();
        /// <summary>
        /// 获取未执行的任务数量
        /// </summary>
        Task<int> ToUnRunCountAsync();
    }
}