using System.Threading.Tasks;
using FS.Core.Job;
using FS.Job;
using FSS.Domain.Tasks.TaskGroup;
using FSS.Domain.Tasks.TaskGroup.Repository;

namespace FSS.Application.Job;

/// <summary>
///     计算任务组的平均耗时
/// </summary>
[FssJob(Name = "FSS.SyncTaskGroupAvgSpeed")]
public class SyncTaskGroupAvgSpeedJob : IFssJob
{
    public TaskGroupService     TaskGroupService    { get; set; }
    public ITaskGroupRepository TaskGroupRepository { get; set; }

    public async Task<bool> Execute(IFssContext context)
    {
        var taskGroupVos = await TaskGroupService.ToListAsync();
        foreach (var taskGroupVo in taskGroupVos)
        {
            // 先计算在更新
            var taskGroup = await TaskGroupRepository.ToEntityAsync(taskGroupId: taskGroupVo.Id);
            await taskGroup.UpdateAvgSpeed();
            await Task.Delay(millisecondsDelay: 1000);
        }
        return true;
    }
}