using System.Collections.Generic;
using System.Threading.Tasks;
using FS.DI;
using FSS.Abstract.Entity.MetaInfo;

namespace FSS.Abstract.Server.MetaInfo
{
    public interface ITaskGroupList: ITransientDependency
    {
        /// <summary>
        /// 获取全部任务列表
        /// </summary>
        Task<List<TaskGroupVO>> ToListAsync();

        /// <summary>
        /// 删除整个缓存
        /// </summary>
        Task ClearAsync();

        /// <summary>
        /// 获取全部任务列表
        /// </summary>
        Task<List<TaskGroupVO>> ToListAndSaveAsync();

        /// <summary>
        /// 获取任务组数量
        /// </summary>
        Task<long> Count();
    }
}