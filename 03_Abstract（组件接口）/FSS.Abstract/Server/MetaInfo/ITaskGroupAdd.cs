using System.Threading.Tasks;
using FS.DI;
using FSS.Abstract.Entity.MetaInfo;

namespace FSS.Abstract.Server.MetaInfo
{
    public interface ITaskGroupAdd: ITransientDependency
    {
        /// <summary>
        /// 添加任务信息
        /// </summary>
        Task<int> AddAsync(TaskGroupVO vo);
    }
}