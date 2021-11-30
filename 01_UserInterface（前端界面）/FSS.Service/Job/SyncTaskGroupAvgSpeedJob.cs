using System.Threading.Tasks;
using FS.Core.Job;
using FS.Job;
using FS.Job.Entity;
using FSS.Abstract.Server.MetaInfo;

namespace FSS.Service.Job
{
    /// <summary>
    /// 计算任务组的平均耗时
    /// </summary>
    [FssJob(Name = "FSS.SyncTaskGroupAvgSpeed")]
    public class SyncTaskGroupAvgSpeedJob : IFssJob
    {
        public ITaskGroupList   TaskGroupList   { get; set; }
        public ITaskGroupInfo   TaskGroupInfo   { get; set; }
        public ITaskGroupUpdate TaskGroupUpdate { get; set; }
        public ITaskInfo        TaskInfo        { get; set; }

        public async Task<bool> Execute(IFssContext context)
        {
            var taskGroupVos = await TaskGroupList.ToListInCacheAsync();
            foreach (var taskGroupVo in taskGroupVos)
            {
                // 先计算在更新
                var statAvgSpeed = await TaskInfo.StatAvgSpeedAsync(taskGroupVo.Id);
                if (statAvgSpeed == 0) continue;

                var taskGroupVO = await TaskGroupInfo.ToInfoAsync(taskGroupVo.Id);
                taskGroupVO.RunSpeedAvg = statAvgSpeed;
                await TaskGroupUpdate.SaveAsync(taskGroupVO);

                await Task.Delay(1000);
            }
            return true;
        }
    }
}