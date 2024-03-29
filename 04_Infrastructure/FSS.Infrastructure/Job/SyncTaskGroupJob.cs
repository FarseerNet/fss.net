using System.Threading.Tasks;
using FS.Core.Abstract.Fss;
using FS.Fss;
using FSS.Domain.Tasks.TaskGroup.Repository;
using FSS.Infrastructure.Repository.TaskGroup;

namespace FSS.Infrastructure.Job;

/// <summary>
///     同步任务组信息数据库与缓存
/// </summary>
[FssJob(Name = "FSS.SyncTaskGroup")]
public class SyncTaskGroupJob : IFssJob
{
    public ITaskGroupRepository TaskGroupRepository { get; set; }
    public TaskGroupAgent       TaskGroupAgent      { get; set; }

    public async Task<bool> Execute(IFssContext context)
    {
        // 数据库同步到缓存
        using var lstGroupByDb = await TaskGroupAgent.ToListAsync();
        var curIndex     = 0;
        foreach (var taskGroupVo in lstGroupByDb)
        {
            curIndex++;
            // 强制从缓存中再读一次，可以实现当缓存丢失时，可以重新写入该条任务组到缓存
            var po = await TaskGroupRepository.ToEntityAsync(taskGroupId: taskGroupVo.Id.GetValueOrDefault());
            await TaskGroupAgent.UpdateAsync(id: po.Id, taskGroup: po);
            context.SetProgress(rate: curIndex / lstGroupByDb.Count * 100);
        }
        return true;
    }
}