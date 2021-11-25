using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FS.DI;
using FS.Extends;
using FS.Job;
using FS.Job.Entity;
using FSS.Abstract.Server.MetaInfo;
using Microsoft.Extensions.Configuration;

namespace FSS.Service.Job
{
    /// <summary>
    /// 自动清除历史任务记录
    /// </summary>
    [FssJob(Name = "FSS.ClearHisTask")]
    public class ClearHisTaskJob : IFssJob
    {
        public ITaskGroupList TaskGroupList { get; set; }
        public ITaskList      TaskList      { get; set; }

        private readonly int _reservedTaskCount;
        public ClearHisTaskJob()
        {
            _reservedTaskCount = IocManager.GetService<IConfigurationRoot>().GetSection("FSS:ReservedTaskCount").Value.ConvertType(20);
        }

        public async Task<bool> Execute(ReceiveContext context)
        {
            var lst = await TaskGroupList.ToListInCacheAsync();
            foreach (var taskGroupVO in lst)
            {
                var lstTask = await TaskList.ToSuccessListAsync(taskGroupVO.Id, _reservedTaskCount);
                if (lstTask == null || lstTask.Count == 0) continue;
                var taskId  = lstTask.Min(o => o.Id);

                // 清除历史记录
                await TaskList.ClearSuccessAsync(taskGroupVO.Id, taskId);
                Thread.Sleep(1000);
            }
            return true;
        }
    }
}