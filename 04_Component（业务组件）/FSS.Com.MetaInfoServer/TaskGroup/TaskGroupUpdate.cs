using System;
using System.Threading.Tasks;
using FS.Cache.Redis;
using FS.Extends;
using FSS.Abstract.Entity.MetaInfo;
using FSS.Abstract.Enum;
using FSS.Abstract.Server.MetaInfo;
using FSS.Com.MetaInfoServer.Abstract;
using FSS.Com.MetaInfoServer.TaskGroup.Dal;
using FSS.Com.MetaInfoServer.Tasks.Dal;
using Newtonsoft.Json;

namespace FSS.Com.MetaInfoServer.TaskGroup
{
    public class TaskGroupUpdate : ITaskGroupUpdate
    {
        public ITaskGroupCache    TaskGroupCache    { get; set; }
        public ITaskGroupAgent    TaskGroupAgent    { get; set; }
        public ITaskGroupInfo     TaskGroupInfo     { get; set; }
        public IRedisCacheManager RedisCacheManager { get; set; }

        /// <summary>
        /// 更新TaskGroup
        /// </summary>
        public Task UpdateAsync(TaskGroupVO taskGroup) => TaskGroupCache.SaveAsync(taskGroup.Id, taskGroup);

        /// <summary>
        /// 保存TaskGroup
        /// </summary>
        public async Task SaveAsync(TaskGroupVO taskGroup)
        {
            await UpdateAsync(taskGroup);
            var taskGroupPO = taskGroup.Map<TaskGroupPO>();
            taskGroupPO.Cron       = null;
            taskGroupPO.IntervalMs = null;
            taskGroupPO.IsEnable   = null;
            await TaskGroupAgent.UpdateAsync(taskGroup.Id, taskGroupPO);
        }

        /// <summary>
        /// 更新任务ID
        /// </summary>
        public async Task UpdateTaskIdAsync(int taskGroupId, int taskId)
        {
            var taskGroupVO = await TaskGroupInfo.ToInfoAsync(taskGroupId);
            taskGroupVO.TaskId = taskId;
            await UpdateAsync(taskGroupVO);
            await TaskGroupAgent.UpdateTaskIdAsync(taskGroupId, taskId);
        }

        /// <summary>
        /// 统计失败次数，按次数递增时间
        /// </summary>
        public async Task StatFailAsync(TaskVO task)
        {
            var failKey = TaskCache.FailKey(task.TaskGroupId);

            switch (task.Status)
            {
                case EumTaskType.Fail:
                case EumTaskType.ReScheduler:
                    // 失败时，要写入失败列表
                    await RedisCacheManager.Db.ListLeftPushAsync(failKey, JsonConvert.SerializeObject(task));

                    // 如果连接3次执行失败，则要把下次执行时间改为递增方式
                    var failCount = await RedisCacheManager.Db.ListLengthAsync(failKey);
                    if (failCount >= 4)
                    {
                        // 从第4次开始递增时间，每失败1次 + 5分钟
                        var count  = failCount - 3;
                        var nextAt = DateTime.Now.AddMinutes(count * 5);

                        // 当计划下次时间，比计算出来的时间要早（一般是立即就要执行），则强制更新下次时间为计算后的时间（延迟执行）
                        var taskGroupVO = await TaskGroupInfo.ToInfoAsync(task.TaskGroupId);
                        if (taskGroupVO.NextAt < nextAt)
                        {
                            taskGroupVO.NextAt = nextAt;
                            await SaveAsync(taskGroupVO);
                        }
                    }

                    break;
                case EumTaskType.Success:
                    // 一旦恢复成功，则清除失败列表
                    await RedisCacheManager.Db.KeyDeleteAsync(failKey);
                    break;
            }
        }
    }
}