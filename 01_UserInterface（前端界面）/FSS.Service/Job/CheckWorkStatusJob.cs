using System;
using System.Threading.Tasks;
using FS.Job;
using FS.Job.Entity;
using FSS.Abstract.Server.MetaInfo;
using FSS.Abstract.Server.Scheduler;

namespace FSS.Service.Job
{
    /// <summary>
    /// 检测进行中状态的任务
    /// </summary>
    [FssJob(Name = "FSS.CheckWorkStatus")]
    public class CheckWorkStatusJob : IFssJob
    {
        public ITaskList           TaskList           { get; set; }
        public ICheckClientOffline CheckClientOffline { get; set; }

        public async Task<bool> Execute(ReceiveContext context)
        {
            // 取出任务组
            var lstTask = await TaskList.ToSchedulerWorkingListAsync();

            foreach (var task in lstTask)
            {
                // 检查是否离线
                await CheckClientOffline.Check(task);
                await Task.Delay(200);
            }

            context.SetNextAt(TimeSpan.FromSeconds(30));
            return true;
        }
    }

}