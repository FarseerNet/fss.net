using Collections.Pooled;
using FS.Core;
using FS.Core.Abstract.Data;
using FS.DI;
using FSS.Domain.Tasks.TaskGroup.Entity;
using FSS.Domain.Tasks.TaskGroup.Enum;

namespace FSS.Domain.Tasks.TaskGroup.Repository;

public interface ITaskGroupRepository : ISingletonDependency
{
    /// <summary>
    ///     获取任务组信息
    /// </summary>
    Task<TaskGroupDO> ToEntityAsync(int taskGroupId);

    /// <summary>
    ///     今日执行失败数量
    /// </summary>
    Task<int> TodayFailCountAsync();
    /// <summary>
    ///     当前任务组下所有任务的执行速度
    /// </summary>
    Task<PooledList<long>> ToTaskSpeedListAsync(int taskGroupId);

    /// <summary>
    ///     获取所有任务组中的任务
    /// </summary>
    Task<PooledList<TaskGroupDO>> ToListAsync();

    /// <summary>
    ///     获取任务组数量
    /// </summary>
    Task<int> GetTaskGroupCountAsync();

    /// <summary>
    ///     获取指定任务组执行成功的任务列表
    /// </summary>
    Task<PooledList<TaskDO>> ToFinishListAsync(int taskGroupId, int top);
    /// <summary>
    ///     创建任务
    /// </summary>
    void AddTask(TaskDO taskDO);

    /// <summary>
    ///     添加任务组
    /// </summary>
    Task<TaskGroupDO> AddAsync(TaskGroupDO taskGroupDO);
    /// <summary>
    ///     保存任务组信息
    /// </summary>
    TaskGroupDO Save(TaskGroupDO taskGroupDO);
    /// <summary>
    ///     删除任务组
    /// </summary>
    Task DeleteAsync(int taskGroupId);
    /// <summary>
    ///     同步数据
    /// </summary>
    Task SyncToData();
    /// <summary>
    ///     获取所有任务组中的任务
    /// </summary>
    Task<PooledList<TaskDO>> GetCanSchedulerTaskGroup(string[] jobsName, TimeSpan ts, int count, ClientVO client);
    /// <summary>
    ///     获取未执行的任务数量
    /// </summary>
    Task<int> ToUnRunCountAsync();
    /// <summary>
    ///     获取执行中的任务
    /// </summary>
    Task<PooledList<TaskGroupDO>> ToSchedulerWorkingListAsync();
    /// <summary>
    ///     获取指定任务组的任务列表（FOPS）
    /// </summary>
    Task<PageList<TaskDO>> ToListAsync(int groupId, int pageSize, int pageIndex);
    /// <summary>
    ///     获取已完成的任务列表
    /// </summary>
    Task<PageList<TaskDO>> ToFinishPageListAsync(int pageSize, int pageIndex);
    Task<PooledList<TaskGroupDO>> ToListAsync(long clientId);
    /// <summary>
    ///     获取进行中的任务
    /// </summary>
    Task<PooledList<TaskGroupDO>> GetTaskUnFinishList(IEnumerable<string> jobsName, int top);
    /// <summary>
    ///     获取在用的任务组
    /// </summary>
    PageList<TaskDO> GetEnableTaskList(EumTaskType? status, int pageSize, int pageIndex);
}