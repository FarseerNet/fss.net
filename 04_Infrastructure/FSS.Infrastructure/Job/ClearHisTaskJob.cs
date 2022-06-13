using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FS.Core.Abstract.Fss;
using FS.DI;
using FS.Extends;
using FS.Fss;
using FSS.Domain.Tasks.TaskGroup.Repository;
using FSS.Infrastructure.Repository.TaskGroup;
using FSS.Infrastructure.Repository.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FSS.Infrastructure.Job;

/// <summary>
///     自动清除历史任务记录
/// </summary>
[FssJob(Name = "FSS.ClearHisTask")]
public class ClearHisTaskJob : IFssJob
{

    private readonly int _reservedTaskCount;
    public ClearHisTaskJob()
    {
        _reservedTaskCount = IocManager.GetService<IConfigurationRoot>().GetSection(key: "FSS:ReservedTaskCount").Value.ConvertType(defValue: 20);
    }
    public TaskAgent            TaskAgent           { get; set; }
    public ITaskGroupRepository TaskGroupRepository { get; set; }

    public async Task<bool> Execute(IFssContext context)
    {
        using var lst      = await TaskGroupRepository.ToListAsync();
        var curIndex = 0;
        var result   = 0;
        foreach (var taskGroupVO in lst)
        {
            curIndex++;
            var lstTask = await TaskAgent.ToFinishListAsync(groupId: taskGroupVO.Id, top: _reservedTaskCount);
            if (lstTask == null || lstTask.Count == 0) continue;
            result += lstTask.Count;
            var taskId = lstTask.Min(selector: o => o.Id.GetValueOrDefault());

            // 清除历史记录
            await TaskAgent.ClearFinishAsync(groupId: taskGroupVO.Id, taskId: taskId);
            context.SetProgress(rate: curIndex / lst.Count * 100);
            Thread.Sleep(millisecondsTimeout: 100);
        }
        await context.LoggerAsync(logLevel: LogLevel.Information, log: $"共清除{result}条历史任务记录");
        return true;
    }
}