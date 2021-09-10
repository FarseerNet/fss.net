using System.Collections.Generic;
using System.Threading.Tasks;
using FS.DI;
using FSS.Abstract.Entity;
using FSS.Abstract.Entity.MetaInfo;

namespace FSS.Abstract.Server.MetaInfo
{
    public interface ITaskList: ITransientDependency
    {
        /// <summary>
        /// 获取指定任务组执行成功的任务列表
        /// </summary>
        Task<List<TaskVO>> ToSuccessListAsync(int groupId, int top);

        /// <summary>
        /// 清除成功的任务记录（1天前）
        /// </summary>
        Task ClearSuccessAsync(int groupId, int taskId);

        /// <summary>
        /// 获取未执行的任务列表
        /// </summary>
        Task<List<TaskVO>> ToNoneListAsync();

        /// <summary>
        /// 拉取指定数量的任务，并将任务设为已调度状态
        /// </summary>
        Task<List<TaskVO>> PullTaskAsync(ClientVO client);
    }
}