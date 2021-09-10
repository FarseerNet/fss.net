using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FS.DI;
using FS.MQ.RedisStream;
using FSS.Abstract.Entity.MetaInfo;
using FSS.Abstract.Enum;
using FSS.Abstract.Server.MetaInfo;
using FSS.Abstract.Server.RegisterCenter;
using FSS.Abstract.Server.Scheduler;
using Microsoft.Extensions.Logging;

namespace FSS.Com.SchedulerServer.Scheduler
{
    public class WhenTaskStatusFinish : IWhenTaskStatus
    {
        public ITaskInfo       TaskInfo       { get; set; }
        public IClientRegister ClientRegister { get; set; }
        public ITaskGroupList  TaskGroupList  { get; set; }
        public ILogger         Logger         { get; set; }
        public IIocManager     IocManager     { get; set; }
        public ITaskAdd        TaskAdd        { get; set; }
        public ITaskScheduler  TaskScheduler  { get; set; }

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
                        var dicTaskGroup = await TaskGroupList.ToListByMemoryAsync();
                        var lstTask      = await TaskInfo.ToGroupListAsync();

                        // 取出状态为None的，且马上到时间要处理的
                        var lstStatusFinish = lstTask.FindAll(o =>
                                dicTaskGroup.ContainsKey(o.TaskGroupId) && ClientRegister.Exists(dicTaskGroup[o.TaskGroupId].JobName) && // 注册进来的客户端，必须是能处理的，否则退出线程
                                o.Status is EumTaskType.Fail or EumTaskType.Success or EumTaskType.ReScheduler &&                        // 状态必须是 完成的
                                (DateTime.Now - o.StartAt).TotalSeconds > 3 &&
                                dicTaskGroup[o.TaskGroupId].IsEnable) // 任务组必须是开启
                            .OrderBy(o => o.StartAt).ToList();

                        foreach (var task in lstStatusFinish)
                        {
                            var newTask = await TaskAdd.GetOrCreateAsync(task.TaskGroupId);
                            await TaskScheduler.Scheduler(dicTaskGroup[task.TaskGroupId], newTask);
                            Logger.LogDebug($"\t1、新建任务: GroupId={task.TaskGroupId} TaskId={task.Id}");
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