using System.Threading.Tasks;
using FS.Core.Job;
using FS.Job;
using FSS.Abstract.Server.MetaInfo;

namespace FSS.Service.Job
{
    /// <summary>
    /// 同步任务组信息数据库与缓存
    /// </summary>
    [FssJob(Name = "FSS.SyncTaskGroup")]
    public class SyncTaskGroupJob : IFssJob
    {
        public ITaskGroupList TaskGroupList { get; set; }
        public ITaskGroupInfo TaskGroupInfo { get; set; }

        public async Task<bool> Execute(IFssContext context)
        {
            // 数据库同步到缓存
            var lstGroupByDb = await TaskGroupList.ToListInDbAsync();
            foreach (var taskGroupVo in lstGroupByDb)
            {
                // 强制从缓存中再读一次，可以实现当缓存丢失时，可以重新写入该条任务组到缓存
                await TaskGroupInfo.ToInfoAsync(taskGroupVo.Id);
            }

            return true;
        }
    }
}