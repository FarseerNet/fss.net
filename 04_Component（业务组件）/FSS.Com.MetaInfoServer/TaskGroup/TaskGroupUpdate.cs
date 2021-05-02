using System;
using FS.Cache.Redis;
using FS.Extends;
using FSS.Abstract.Entity.MetaInfo;
using FSS.Abstract.Enum;
using FSS.Abstract.Server.MetaInfo;
using FSS.Com.MetaInfoServer.Abstract;
using FSS.Com.MetaInfoServer.Task.Dal;
using FSS.Com.MetaInfoServer.TaskGroup.Dal;
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
        public void Update(TaskGroupVO taskGroup)
        {
            TaskGroupCache.Save(taskGroup.Id, taskGroup);
        }

        /// <summary>
        /// 保存TaskGroup
        /// </summary>
        public void Save(TaskGroupVO taskGroup)
        {
            Update(taskGroup);
            var taskGroupPO = taskGroup.Map<TaskGroupPO>();
            taskGroupPO.Cron = null;
            taskGroupPO.IntervalMs = null;
            taskGroupPO.IsEnable = null;
            TaskGroupAgent.Update(taskGroup.Id, taskGroupPO);
        }

        /// <summary>
        /// 更新任务ID
        /// </summary>
        public void UpdateTaskId(int taskGroupId, int taskId)
        {
            var taskGroupVO = TaskGroupInfo.ToInfo(taskGroupId);
            taskGroupVO.TaskId = taskId;
            Update(taskGroupVO);
            TaskGroupAgent.UpdateTaskId(taskGroupId, taskId);
        }

        /// <summary>
        /// 统计失败次数，按次数递增时间
        /// </summary>
        public void StatFail(TaskVO task)
        {
            var failKey = TaskCache.FailKey(task.TaskGroupId);

            switch (task.Status)
            {
                case EumTaskType.Fail:
                case EumTaskType.ReScheduler:
                    // 失败时，要写入失败列表
                    RedisCacheManager.Db.ListLeftPush(failKey, JsonConvert.SerializeObject(task));

                    // 如果连接3次执行失败，则要把下次执行时间改为递增方式
                    var failCount = RedisCacheManager.Db.ListLength(failKey);
                    if (failCount >= 4)
                    {
                        // 从第4次开始递增时间，每失败1次 + 5分钟
                        var count  = failCount - 3;
                        var nextAt = DateTime.Now.AddMinutes(count * 5);

                        // 当计划下次时间，比计算出来的时间要早（一般是立即就要执行），则强制更新下次时间为计算后的时间（延迟执行）
                        var taskGroupVO = TaskGroupInfo.ToInfo(task.TaskGroupId);
                        if (taskGroupVO.NextAt < nextAt)
                        {
                            taskGroupVO.NextAt = nextAt;
                            Save(taskGroupVO);
                        }
                    }

                    break;
                case EumTaskType.Success:
                    // 一旦恢复成功，则清除失败列表
                    RedisCacheManager.Db.KeyDelete(failKey);
                    break;
            }
        }
    }
}