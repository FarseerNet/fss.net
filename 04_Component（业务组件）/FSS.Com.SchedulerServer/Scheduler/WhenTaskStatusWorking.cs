using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FS.DI;
using FSS.Abstract.Entity.MetaInfo;
using FSS.Abstract.Enum;
using FSS.Abstract.Server.MetaInfo;
using FSS.Abstract.Server.RegisterCenter;
using FSS.Abstract.Server.Scheduler;
using FSS.Com.SchedulerServer.Abstract;
using Microsoft.Extensions.Logging;

namespace FSS.Com.SchedulerServer.Scheduler
{
    public class WhenTaskStatusWorking : IWhenTaskStatus
    {
        public                  ITaskInfo           TaskInfo           { get; set; }
        public                  IClientRegister     ClientRegister     { get; set; }
        public                  ITaskGroupList      TaskGroupList      { get; set; }
        public                  ILogger             Logger             { get; set; }
        public                  IIocManager         IocManager         { get; set; }
        public                  ICheckClientOffline CheckClientOffline { get; set; }

        /// <summary>
        /// 运行当状态为Node的任务
        /// </summary>
        public Task Run()
        {
            Logger = IocManager.Logger<WhenTaskStatusWorking>();
            ThreadPool.QueueUserWorkItem(async _ =>
            {
                while (true)
                {
                    try
                    {
                        var dicTaskGroup = await TaskGroupList.ToListByMemoryAsync();
                        var lstTask      = await TaskInfo.ToGroupListAsync();

                        // 注册进来的客户端，必须是能处理的，否则退出线程
                        var lstStatusWorking = lstTask.FindAll(o =>  dicTaskGroup.ContainsKey(o.TaskGroupId) && ClientRegister.Exists(dicTaskGroup[o.TaskGroupId].JobName));
                        if (lstStatusWorking == null || lstStatusWorking.Count == 0)
                        {
                            await Task.Delay(3000);
                            continue;
                        }

                        // 取出马上到时间要处理的
                        lstStatusWorking = lstStatusWorking.FindAll(o =>
                                (o.Status is EumTaskType.Working or EumTaskType.Scheduler) && // 状态必须是 EumTaskType.None
                                (DateTime.Now - o.SchedulerAt).TotalMilliseconds >= 10000 &&      // 执行时间在10s后的
                                dicTaskGroup[o.TaskGroupId].IsEnable)                         // 任务组必须是开启
                            .OrderBy(o => o.StartAt).ToList();

                        // 没有任务需要调度
                        if (lstStatusWorking == null || lstStatusWorking.Count == 0)
                        {
                            await Task.Delay(5000);
                            continue;
                        }

                        foreach (var taskGroupId in lstStatusWorking.Select(o => o.TaskGroupId))
                        {
                            try
                            {
                                // 重新取一遍，担心正好数据被正确处理好了
                                var task = await TaskInfo.ToInfoByGroupIdAsync(taskGroupId);

                                // 检查客户端是否离线
                                await CheckClientOffline.Check(task, dicTaskGroup[taskGroupId]);
                            }
                            catch (Exception e)
                            {
                                Logger.LogError(e, e.Message);
                            }
                            finally
                            {
                                // 休眠下，防止CPU过高
                                await Task.Delay(100);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.LogError(e, e.Message);
                    }

                    // 休眠下，防止CPU过高
                    await Task.Delay(5000);
                }
            });
            return Task.FromResult(0);
        }
    }
}