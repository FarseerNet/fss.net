using System.Threading.Tasks;
using FS.Core.Job;
using FS.Job;
using FSS.Application.Tasks.TaskGroup;

namespace FSS.Service.Job
{
    /// <summary>
    /// 计算任务组的平均耗时
    /// </summary>
    [FssJob(Name = "FSS.SyncTaskGroupAvgSpeed")]
    public class SyncTaskGroupAvgSpeedJob : IFssJob
    {
        public TaskQueryApp TaskQueryApp { get; set; }
        public TaskGroupApp TaskGroupApp { get; set; }

        public async Task<bool> Execute(IFssContext context)
        {
            var taskGroupVos = await TaskQueryApp.ToListAsync();
            foreach (var taskGroupVo in taskGroupVos)
            {
                // 先计算在更新
                await TaskGroupApp.UpdateAvgSpeed(taskGroupVo.Id);
                await Task.Delay(1000);
            }
            return true;
        }
    }
}