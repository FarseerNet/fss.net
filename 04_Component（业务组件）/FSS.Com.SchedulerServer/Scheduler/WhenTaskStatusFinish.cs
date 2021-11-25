using System;
using System.Linq;
using System.Threading.Tasks;
using FS.DI;
using FSS.Abstract.Enum;
using FSS.Abstract.Server.MetaInfo;
using FSS.Abstract.Server.Scheduler;
using Microsoft.Extensions.Logging;

namespace FSS.Com.SchedulerServer.Scheduler
{
    public class WhenTaskStatusFinish : IWhenTaskStatus
    {
        public ITaskInfo      TaskInfo      { get; set; }
        public ITaskGroupList TaskGroupList { get; set; }
        public ILogger        Logger        { get; set; }
        public IIocManager    IocManager    { get; set; }
        public ITaskAdd       TaskAdd       { get; set; }

        /// <summary>
        /// 运行当状态为Node的任务
        /// </summary>
        public Task Run()
        {
            Logger = IocManager.Logger<WhenTaskStatusFinish>();
            return Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    try
                    {
                        var dicTaskGroup = await TaskGroupList.ToListInCacheAsync();

                        foreach (var taskGroupVO in dicTaskGroup.Where(o=>o.IsEnable))
                        {
                            var task = await TaskInfo.ToInfoByGroupIdAsync(taskGroupVO.Id);
                            if (task != null)
                            {
                                // 状态必须是 完成的
                                if (task.Status != EumTaskType.Fail && task.Status != EumTaskType.Success) continue;
                                // 加个时间，来限制并发
                                if ((DateTime.Now - task.RunAt).TotalSeconds < 3) continue;
                            }
                                
                            var newTask = await TaskAdd.GetOrCreateAsync(taskGroupVO.Id);
                            Logger.LogDebug($"\t1、新建任务: GroupId={newTask.TaskGroupId} TaskId={newTask.Id}");
                            await Task.Delay(200);
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.LogError(e, e.Message);
                    }

                    // 休眠下，防止CPU过高
                    await Task.Delay(TimeSpan.FromMinutes(1));
                }
            }, TaskCreationOptions.LongRunning);
        }
    }
}