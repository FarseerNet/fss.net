using System.Collections.Generic;
using System.Threading.Tasks;
using FS.DI;
using FSS.Infrastructure.Repository.Tasks.Model;

namespace FSS.Infrastructure.Repository.Tasks.Interface
{
    public interface ITaskAgent: ISingletonDependency
    {
        /// <summary>
        /// 获取指定任务组执行成功的任务列表
        /// </summary>
        Task<List<TaskPO>> ToFinishListAsync(int groupId, int top);
        /// <summary>
        /// 清除成功的任务记录（1天前）
        /// </summary>
        Task ClearFinishAsync(int groupId, int taskId);
        /// <summary>
        /// 取前100条的运行速度
        /// </summary>
        Task<List<long>> ToSpeedListAsync(int groupId);
        /// <summary>
        /// 今日执行失败数量
        /// </summary>
        Task<int> TodayFailCountAsync();
        /// <summary>
        /// 删除当前任务组下的所有
        /// </summary>
        Task<int> DeleteAsync(int taskGroupId);
        /// <summary>
        /// 保存任务信息
        /// </summary>
        Task SaveAsync(TaskPO task);
        /// <summary>
        /// 将Task写入队列中
        /// </summary>
        Task AddQueueAsync(TaskPO task);
        /// <summary>
        /// 队列中取出已完成的任务
        /// </summary>
        Task<List<TaskPO>> GetFinishTaskListAsync(int top);
        /// <summary>
        /// 获取任务信息
        /// </summary>
        Task<TaskPO> ToEntityAsync(int taskGroupId);
        /// <summary>
        /// 获取所有任务组中的任务
        /// </summary>
        Task<List<TaskPO>> ToGroupListAsync();
        /// <summary>
        /// 将任务暂时写入redis集合，再通过job集中写入数据库
        /// </summary>
        Task<int> AddToDbAsync(List<TaskPO> lstTask);
    }
}