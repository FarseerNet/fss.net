using System.Collections.Generic;
using System.Threading.Tasks;
using FS.DI;
using FSS.Abstract.Entity;
using FSS.Abstract.Entity.MetaInfo;

namespace FSS.Abstract.Server.MetaInfo
{
    public interface ITaskList: ISingletonDependency
    {
        /// <summary>
        /// 获取指定任务组执行成功的任务列表
        /// </summary>
        Task<List<TaskVO>> ToFinishListAsync(int groupId, int top);

        /// <summary>
        /// 清除成功的任务记录（1天前）
        /// </summary>
        Task ClearFinishAsync(int groupId, int taskId);

        /// <summary>
        /// 拉取指定数量的任务，并将任务设为已调度状态
        /// </summary>
        Task<List<TaskVO>> PullTaskAsync(ClientVO client, int requestTaskCount);

        /// <summary>
        /// 获取执行中的任务
        /// </summary>
        Task<List<TaskVO>> ToSchedulerWorkingListAsync();
        /// <summary>
        /// 获取指定任务组的任务列表（FOPS）
        /// </summary>
        Task<List<TaskVO>> ToListAsync(int groupId, int pageSize, int pageIndex, out int totalCount);
        /// <summary>
        /// 获取已完成的任务列表
        /// </summary>
        Task<List<TaskVO>> ToFinishListAsync(int pageSize, int pageIndex, out int totalCount);
        /// <summary>
        /// 获取未执行的任务数量
        /// </summary>
        Task<int> ToUnRunCountAsync();
    }
}