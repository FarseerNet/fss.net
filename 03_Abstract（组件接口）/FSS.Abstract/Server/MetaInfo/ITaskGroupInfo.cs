using System.Threading.Tasks;
using FS.DI;
using FSS.Abstract.Entity.MetaInfo;

namespace FSS.Abstract.Server.MetaInfo
{
    public interface ITaskGroupInfo: ISingletonDependency
    {
        /// <summary>
        /// 获取任务信息
        /// </summary>
        Task<TaskGroupDO> ToInfoAsync(int id);

    }
}