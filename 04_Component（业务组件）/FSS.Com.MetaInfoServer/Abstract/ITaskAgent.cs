using System.Collections.Generic;
using System.Threading.Tasks;
using FS.DI;
using FSS.Com.MetaInfoServer.Tasks.Dal;

namespace FSS.Com.MetaInfoServer.Abstract
{
    public interface ITaskAgent : ITransientDependency
    {
        /// <summary>
        /// 获取所有任务列表
        /// </summary>
        Task<List<TaskPO>> ToListAsync();
        
        /// <summary>
        /// 获取任务信息
        /// </summary>
        Task<TaskPO> ToEntityAsync(int id);

        /// <summary>
        /// 更新任务信息
        /// </summary>
        Task UpdateAsync(int id, TaskPO task);

        /// <summary>
        /// 添加任务信息
        /// </summary>
        Task AddAsync(TaskPO task);

        /// <summary>
        /// 获取未执行的任务信息
        /// </summary>
        Task<TaskPO> ToUnExecutedTaskAsync(int groupId);

        /// <summary>
        /// 取前100条的运行速度
        /// </summary>
        Task<List<int>> ToSpeedListAsync(int groundId);

        /// <summary>
        /// 获取指定任务组执行成功的任务列表
        /// </summary>
        Task<List<TaskPO>> ToSuccessListAsync(int groupId, int top);

        /// <summary>
        /// 清除成功的任务记录（1天前）
        /// </summary>
        Task ClearSuccessAsync(int groupId, int taskId);

        /// <summary>
        /// 今日执行失败数量
        /// </summary>
        Task<int> TodayFailCountAsync();
    }
}