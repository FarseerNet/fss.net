using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FS.Cache.Redis;
using FS.DI;
using FS.MQ.RedisStream;
using FSS.Abstract.Enum;
using FSS.Abstract.Server.MetaInfo;
using FSS.Abstract.Server.RegisterCenter;
using FSS.Abstract.Server.Scheduler;
using Microsoft.Extensions.Logging;

namespace FSS.Com.SchedulerServer.Scheduler
{
    public class WhenTaskStatusNone : IWhenTaskStatus
    {
        public ITaskInfo       TaskInfo       { get; set; }
        public IClientRegister ClientRegister { get; set; }
        public ITaskGroupList  TaskGroupList  { get; set; }
        public ITaskList       TaskList       { get; set; }
        public ITaskUpdate     TaskUpdate     { get; set; }
        public IIocManager     IocManager     { get; set; }
        public IRunLogAdd      RunLogAdd      { get; set; }
        public ITaskScheduler  TaskScheduler  { get; set; }

        /// <summary>
        /// 运行当状态为Node的任务
        /// </summary>
        public Task Run()
        {
            var logger = IocManager.Logger<WhenTaskStatusNone>();

            return Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    try
                    {
                        var dicTaskGroup = await TaskGroupList.ToListByMemoryAsync();
                        var lstGroupTask = await TaskInfo.ToGroupListAsync();

                        // 找出未执行的任务列表
                        var lstNoneTask = await TaskList.ToNoneListAsync();
                        foreach (var task in lstNoneTask)
                        {
                            // 缓存中，找到相同任务组ID的任务
                            var taskVO = lstGroupTask.Find(o => o.TaskGroupId == task.TaskGroupId);

                            // 两者的任务ID不一致
                            if (taskVO.Id != task.Id)
                            {
                                task.Status = EumTaskType.Fail;
                                await RunLogAdd.AddAsync(dicTaskGroup[task.TaskGroupId], task.Id, LogLevel.Warning, $"任务ID：{task.Id}，与当前任务组正在执行的任务不一致，强制设为失败状态");
                                await TaskUpdate.SaveAsync(task);
                            }
                        }

                        // 取出状态为None的，且马上到时间要处理的
                        var lstStatusNone = lstGroupTask.FindAll(o =>
                                ClientRegister.Exists(o.JobName) &&             // 注册进来的客户端，必须是能处理的，否则退出线程
                                o.Status == EumTaskType.None &&                 // 状态必须是 EumTaskType.None
                                dicTaskGroup[o.TaskGroupId].IsEnable)           // 任务组必须是开启
                            .OrderBy(o => o.StartAt).ToList();

                        // 调度
                        if (lstStatusNone.Count > 0)
                        {
                            await Task.WhenAll(lstStatusNone.Select(o => TaskScheduler.Scheduler(dicTaskGroup[o.TaskGroupId], o)));
                        }
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, e.Message);
                    }

                    await Task.Delay(5000);
                }
            }, TaskCreationOptions.LongRunning);
        }
    }
}