using System.Collections.Generic;
using System.Threading.Tasks;
using FS.DI;
using FSS.Application.Tasks.TaskGroup.Entity;
using FSS.Domain.Tasks.TaskGroup.Entity;

namespace FSS.Application.Tasks.TaskGroup.Interface
{
    public interface ITaskGroupApp: ISingletonDependency
    {
        /// <summary>
        /// 添加任务组信息
        /// </summary>
        Task<int> AddAsync(TaskGroupDTO dto);
        /// <summary>
        /// 取消任务
        /// </summary>
        Task CancelTask(int groupId);
        /// <summary>
        /// 今日执行失败数量
        /// </summary>
        Task<int> TodayFailCountAsync();
        /// <summary>
        /// 计算任务的平均运行速度
        /// </summary>
        Task<long> StatAvgSpeedAsync(int taskGroupId);
        /// <summary>
        /// 获取所有任务组中的任务
        /// </summary>
        Task<List<TaskGroupDO>> ToListAsync();
    }
}