using System.Threading.Tasks;
using FS.Cache;
using FS.Core.Job;
using FS.Job;
using FSS.Infrastructure.Repository.TaskGroup;

namespace FSS.Infrastructure.Job
{
    /// <summary>
    /// 同步任务组信息数据库与缓存
    /// </summary>
    [FssJob(Name = "FSS.SyncTaskGroup")]
    public class SyncTaskGroupJob : IFssJob
    {
        public TaskGroupCache TaskGroupCache { get; set; }
        public TaskGroupAgent TaskGroupAgent { get; set; }

        public async Task<bool> Execute(IFssContext context)
        {
            // 数据库同步到缓存
            var lstGroupByDb = await TaskGroupAgent.ToListAsync();
            var curIndex     = 0;
            foreach (var taskGroupVo in lstGroupByDb)
            {
                curIndex++;
                // 强制从缓存中再读一次，可以实现当缓存丢失时，可以重新写入该条任务组到缓存
                var po = await TaskGroupCache.ToEntityAsync(taskGroupVo.Id.GetValueOrDefault());
                await TaskGroupAgent.UpdateAsync(po.Id, po);
                context.SetProgress(curIndex / lstGroupByDb.Count * 100);
            }
            return true;
        }
    }
}