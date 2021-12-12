using System.Collections.Generic;
using System.Threading.Tasks;
using FS.Core;
using FS.DI;
using FSS.Application.Clients.Dto;
using FSS.Application.Tasks.TaskGroup.Entity;
using FSS.Domain.Tasks.TaskGroup.Entity;
using FSS.Domain.Tasks.TaskGroup.Enum;

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
        Task CancelTask(int taskGroupId);
        /// <summary>
        /// 今日执行失败数量
        /// </summary>
        Task<int> TodayFailCountAsync();
        /// <summary>
        /// 计算任务的平均运行速度
        /// </summary>
        Task UpdateAvgSpeed(int taskGroupId);
        /// <summary>
        /// 获取所有任务组中的任务
        /// </summary>
        Task<List<TaskGroupDO>> ToListAsync();
        /// <summary>
        /// 获取任务组数量
        /// </summary>
        Task<long> GetTaskGroupCount();
        /// <summary>
        /// 获取任务组信息
        /// </summary>
        Task<TaskGroupDO> ToEntityAsync(int taskGroupId);
        /// <summary>
        /// 创建Task
        /// </summary>
        Task<TaskDO> GetOrCreateAsync(int taskGroupId);
        /// <summary>
        /// 同步数据
        /// </summary>
        Task SyncTaskGroup();
        /// <summary>
        /// 保存任务组
        /// </summary>
        Task Save(TaskGroupDTO dto);
        /// <summary>
        /// 任务调度
        /// </summary>
        Task<List<TaskDTO>> TaskSchedulerAsync(ClientDTO client, int requestTaskCount);
        /// <summary>
        /// 设置任务组状态
        /// </summary>
        Task SetEnable(int taskGroupId, bool enable);
        /// <summary>
        /// 获取指定任务组的任务列表（FOPS）
        /// </summary>
        Task<DataSplitList<TaskDTO>> ToListAsync(int groupId, int pageSize, int pageIndex);
        /// <summary>
        /// 获取指定任务组执行成功的任务列表
        /// </summary>
        Task<List<TaskDTO>> ToTaskGroupFinishListAsync(int taskGroupId, int top);
        /// <summary>
        /// 获取已完成的任务列表
        /// </summary>
        Task<DataSplitList<TaskDTO>> ToFinishListAsync(int pageSize, int pageIndex);
        /// <summary>
        /// 获取执行中的任务
        /// </summary>
        Task<List<TaskGroupDO>> ToSchedulerWorkingListAsync();
        /// <summary>
        /// 复制任务组
        /// </summary>
        Task<int> CopyTaskGroup(int taskGroupId);
        /// <summary>
        /// 删除任务组
        /// </summary>
        Task DeleteAsync(int taskGroupId);
        
        /// <summary>
        /// 获取未执行的任务数量
        /// </summary>
        Task<int> ToUnRunCountAsync();
        /// <summary>
        /// 获取进行中的任务
        /// </summary>
        Task<List<TaskDTO>> GetTaskUnFinishList(int top);
        /// <summary>
        /// 获取在用的任务组
        /// </summary>
        DataSplitList<TaskDTO> GetEnableTaskList(EumTaskType? status, int pageSize, int pageIndex);
    }
}