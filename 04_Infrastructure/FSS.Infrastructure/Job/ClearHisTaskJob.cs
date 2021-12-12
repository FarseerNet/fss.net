using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FS.Cache;
using FS.Core.Job;
using FS.DI;
using FS.Extends;
using FS.Job;
using FSS.Infrastructure.Repository.TaskGroup;
using FSS.Infrastructure.Repository.Tasks;
using Microsoft.Extensions.Configuration;

namespace FSS.Infrastructure.Job
{
    /// <summary>
    /// 自动清除历史任务记录
    /// </summary>
    [FssJob(Name = "FSS.ClearHisTask")]
    public class ClearHisTaskJob : IFssJob
    {
        public TaskAgent      TaskAgent      { get; set; }
        public TaskGroupCache TaskGroupCache { get; set; }

        private readonly int _reservedTaskCount;
        public ClearHisTaskJob()
        {
            _reservedTaskCount = IocManager.GetService<IConfigurationRoot>().GetSection("FSS:ReservedTaskCount").Value.ConvertType(20);
        }

        public async Task<bool> Execute(IFssContext context)
        {
            var lst      = await TaskGroupCache.ToListAsync(EumCacheStoreType.Redis);
            var curIndex = 0;
            foreach (var taskGroupVO in lst)
            {
                curIndex++;
                var lstTask = await TaskAgent.ToFinishListAsync(taskGroupVO.Id.GetValueOrDefault(), _reservedTaskCount);
                if (lstTask == null || lstTask.Count == 0) continue;
                var taskId = lstTask.Min(o => o.Id.GetValueOrDefault());

                // 清除历史记录
                await TaskAgent.ClearFinishAsync(taskGroupVO.Id.GetValueOrDefault(), taskId);
                await context.SetProgressAsync(curIndex / lst.Count * 100);
                Thread.Sleep(100);
            }
            return true;
        }
    }
}