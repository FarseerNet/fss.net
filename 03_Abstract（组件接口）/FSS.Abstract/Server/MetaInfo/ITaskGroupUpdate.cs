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
        Task UpdateAsync(TaskGroupVO taskGroup);

        /// <summary>
        /// 保存TaskGroup
        /// </summary>
        Task SaveAsync(TaskGroupVO taskGroup);

    }
}