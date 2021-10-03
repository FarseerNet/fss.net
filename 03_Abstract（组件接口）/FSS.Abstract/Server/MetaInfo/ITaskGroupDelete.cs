using System.Threading.Tasks;
using FS.DI;

namespace FSS.Abstract.Server.MetaInfo
{
    public interface ITaskGroupDelete: ISingletonDependency
    {
        /// <summary>
        /// 删除任务组
        /// </summary>
        Task DeleteAsync(int taskGroupId);
    }
}