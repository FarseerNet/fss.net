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
                        var dicTaskGroup = await TaskGroupList.ToListInMemoryAsync();
                        var lstTask      = await TaskInfo.ToGroupListAsync();

                        // 取出状态为None的，且马上到时间要处理的
                        var lstStatusFinish = lstTask.FindAll(o =>
                                                              dicTaskGroup.ContainsKey(o.TaskGroupId)                                        &&
                                                              o.Status is EumTaskType.Fail or EumTaskType.Success or EumTaskType.ReScheduler && // 状态必须是 完成的
                                                              (DateTime.Now - dicTaskGroup[o.TaskGroupId].ActivateAt).TotalSeconds > 3       && // 加个时间，来限制并发
                                                              dicTaskGroup[o.TaskGroupId].IsEnable)                                             // 任务组必须是开启
                                                     .OrderBy(o => o.StartAt).ToList();

                        foreach (var task in lstStatusFinish)
                        {
                            var newTask = await TaskAdd.GetOrCreateAsync(task.TaskGroupId);
                            Logger.LogDebug($"\t1、新建任务: GroupId={newTask.TaskGroupId} TaskId={newTask.Id}");
                            await Task.Delay(10);
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