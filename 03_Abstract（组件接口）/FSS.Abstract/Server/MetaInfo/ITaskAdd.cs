using System.Threading.Tasks;
using FS.DI;
using FSS.Abstract.Entity.MetaInfo;

namespace FSS.Abstract.Server.MetaInfo
{
    public interface ITaskAdd: ISingletonDependency
    {
        /// <summary>
        /// 创建Task，并更新到缓存
        /// </summary>
        Task<TaskVO> GetOrCreateAsync(int taskGroupId);

        /// <summary>
        /// 创建Task，并更新到缓存
        /// </summary>
        Task<TaskVO> GetOrCreateAsync(TaskGroupVO taskGroup);
        /// <summary>
        /// 将任务暂时写入redis集合，再通过job集中写入数据库
        /// </summary>
        Task AddToQueueAsync(TaskVO task);
        /// <summary>
        /// 将任务暂时写入redis集合，再通过job集中写入数据库
        /// </summary>
        Task<int> AddToDbAsync(int top);
    }
}