using System.Threading.Tasks;
using FS.DI;
using FSS.Abstract.Entity.MetaInfo;

namespace FSS.Abstract.Server.MetaInfo
{
    public interface ITaskGroupUpdate: ISingletonDependency
    {
        /// <summary>
        /// 更新TaskGroup
        /// </summary>
        Task UpdateAsync(TaskGroupDO taskGroup);

        /// <summary>
        /// 保存TaskGroup
        /// </summary>
        Task SaveAsync(TaskGroupDO taskGroup);

        /// <summary>
        /// 同步缓存到数据库
        /// </summary>
        Task SyncCacheToDb();
    }
}