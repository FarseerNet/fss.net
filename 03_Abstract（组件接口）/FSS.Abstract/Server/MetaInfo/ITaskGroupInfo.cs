using System.Threading.Tasks;
using FS.DI;
using FSS.Abstract.Entity.MetaInfo;

namespace FSS.Abstract.Server.MetaInfo
{
    public interface ITaskGroupInfo: ITransientDependency
    {
        /// <summary>
        /// 获取任务信息
        /// </summary>
        Task<TaskGroupVO> ToInfoAsync(int id);
    }
}