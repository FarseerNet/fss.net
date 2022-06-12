using System.Collections.Generic;
using System.Threading.Tasks;
using Collections.Pooled;
using FS.DI;
using FSS.Infrastructure.Repository.Context;
using FSS.Infrastructure.Repository.TaskGroup.Model;

namespace FSS.Infrastructure.Repository.TaskGroup;

/// <summary>
///     任务组数据库层
/// </summary>
public class TaskGroupAgent : ISingletonDependency
{
    /// <summary>
    ///     获取所有任务组列表
    /// </summary>
    public Task<PooledList<TaskGroupPO>> ToListAsync() => MysqlContext.Data.TaskGroup.ToListAsync();
    /// <summary>
    ///     获取所有任务组列表
    /// </summary>
    public PooledList<TaskGroupPO> ToList() => MysqlContext.Data.TaskGroup.ToList();

    /// <summary>
    ///     获取任务组信息
    /// </summary>
    public Task<TaskGroupPO> ToEntityAsync(int id) => MysqlContext.Data.TaskGroup.Where(where: o => o.Id == id).ToEntityAsync();

    /// <summary>
    ///     更新任务组信息
    /// </summary>
    public Task UpdateAsync(int id, TaskGroupPO taskGroup) => MysqlContext.Data.TaskGroup.Where(where: o => o.Id == id).UpdateAsync(entity: taskGroup);

    /// <summary>
    ///     添加任务组
    /// </summary>
    public async Task<int> AddAsync(TaskGroupPO po)
    {
        await MysqlContext.Data.TaskGroup.InsertAsync(entity: po, isReturnLastId: true);
        return po.Id.GetValueOrDefault();
    }

    /// <summary>
    ///     删除当前任务组下的所有
    /// </summary>
    public async Task DeleteAsync(int taskGroupId)
    {
        await MysqlContext.Data.TaskGroup.Where(where: o => o.Id == taskGroupId).DeleteAsync();
    }

    /// <summary>
    ///     删除当前任务组下的所有
    /// </summary>
    public Task<int> Count() => MysqlContext.Data.TaskGroup.CountAsync();
}