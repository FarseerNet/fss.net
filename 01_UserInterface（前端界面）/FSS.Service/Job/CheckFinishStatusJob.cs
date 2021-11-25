using System;
using System.Linq;
using System.Threading.Tasks;
using FS.Job;
using FS.Job.Entity;
using FSS.Abstract.Server.MetaInfo;
using Microsoft.Extensions.Logging;

namespace FSS.Service.Job
{
    /// <summary>
    /// 检测完成状态的任务
    /// </summary>
    [FssJob(Name = "FSS.CheckFinishStatus")]
    public class CheckFinishStatusJob : IFssJob
    {
        public ITaskGroupList TaskGroupList { get; set; }
        public ITaskInfo      TaskInfo      { get; set; }
        public ITaskAdd       TaskAdd       { get; set; }

        public async Task<bool> Execute(ReceiveContext context)
        {
            // 取出任务组
            var dicTaskGroup = await TaskGroupList.ToListInCacheAsync();

            // 只检测Enable状态的任务组
            foreach (var taskGroupVO in dicTaskGroup.Where(o => o.IsEnable))
            {
                var task = await TaskInfo.ToInfoByGroupIdAsync(taskGroupVO.Id);
                if (task != null)
                {
                    // 状态必须是 完成的
                    if (task.Status != FSS.Abstract.Enum.EumTaskType.Fail && task.Status != FSS.Abstract.Enum.EumTaskType.Success) continue;
                    // 加个时间，来限制并发
                    if ((DateTime.Now - task.RunAt).TotalSeconds < 3) continue;
                }

                var newTask = await TaskAdd.GetOrCreateAsync(taskGroupVO.Id);
                await context.LoggerAsync(LogLevel.Information, $"\t1、新建任务: GroupId={newTask.TaskGroupId} TaskId={newTask.Id}");
                await Task.Delay(200);
            }

            context.SetNextAt(TimeSpan.FromMinutes(1));
            return true;
        }
    }
}