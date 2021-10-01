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
        Task<TaskGroupVO> ToInfoAsync(int id);

        /// <summary>
        /// 从数据库中取出并保存
        /// </summary>
        Task<TaskGroupVO> ToInfoByDbAsync(int id);
    }
}