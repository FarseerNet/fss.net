using System.Threading.Tasks;
using FS.DI;
using FSS.Abstract.Entity.MetaInfo;

namespace FSS.Abstract.Server.MetaInfo
{
    public interface ITaskAdd: ITransientDependency
    {
        /// <summary>
        /// 创建Task，并更新到缓存
        /// </summary>
        Task<TaskVO> GetOrCreateAsync(int taskGroupId);

        /// <summary>
        /// 创建Task，并更新到缓存
        /// </summary>
        Task<TaskVO> GetOrCreateAsync(TaskGroupVO taskGroup);
    }
}