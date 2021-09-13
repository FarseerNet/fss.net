using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FS.Core;
using FS.Extends;
using FSS.Abstract.Entity;
using FSS.Abstract.Entity.MetaInfo;
using FSS.Abstract.Enum;
using FSS.Abstract.Server.MetaInfo;
using FSS.Com.MetaInfoServer.Abstract;
using FSS.Com.MetaInfoServer.Tasks.Dal;

namespace FSS.Com.MetaInfoServer.Tasks
{
    /// <summary>
    /// 任务列表
    /// </summary>
    public class TaskList : ITaskList
    {
        public ITaskAgent       TaskAgent       { get; set; }
        public ITaskGroupInfo   TaskGroupInfo   { get; set; }
        public ITaskUpdate      TaskUpdate      { get; set; }
        public ITaskGroupUpdate TaskGroupUpdate { get; set; }

        /// <summary>
        /// 获取指定任务组执行成功的任务列表
        /// </summary>
        public Task<List<TaskVO>> ToSuccessListAsync(int groupId, int top) => TaskAgent.ToSuccessListAsync(groupId, top).MapAsync<TaskVO, TaskPO>();

        /// <summary>
        /// 清除成功的任务记录（1天前）
        /// </summary>
        public Task ClearSuccessAsync(int groupId, int taskId) => TaskAgent.ClearSuccessAsync(groupId, taskId);

        /// <summary>
        /// 获取未执行的任务列表
        /// </summary>
        public Task<List<TaskVO>> ToNoneListAsync() => TaskAgent.ToNoneListAsync().MapAsync<TaskVO, TaskPO>();

        /// <summary>
        /// 获取执行中的任务
        /// </summary>
        public Task<List<TaskVO>> ToSchedulerWorkingListAsync()
        {
            return MetaInfoContext.Data.Task.Where(o => (o.Status == EumTaskType.Scheduler || o.Status == EumTaskType.Working)).ToListAsync().MapAsync<TaskVO, TaskPO>();
        }

        /// <summary>
        /// 拉取指定数量的任务，并将任务设为已调度状态
        /// </summary>
        public async Task<List<TaskVO>> PullTaskAsync(ClientVO client)
        {
            // 计算本次要拉取任务的数量
            var taskCount = 3;
            var lstPo     = await TaskAgent.ToNoneListAsync(taskCount, client.Jobs);
            if (lstPo.Count == 0) return null;

            // 更新任务状态
            using (var db = new MetaInfoContext())
            {
                // 更新任务为已调度
                foreach (var task in lstPo)
                {
                    task.Status      = EumTaskType.Scheduler;
                    task.ClientIp    = client.ClientIp;
                    task.ClientName  = client.ClientName;
                    task.ClientId    = client.Id;
                    task.SchedulerAt = DateTime.Now;
                    // 更新任务状态
                    var result = await db.Task.Where(o => o.Id == task.Id && o.Status == EumTaskType.None).UpdateAsync(task);
                    // 被其它客户端抢了任务，回滚本次任务
                    if (result == 0)
                    {
                        db.Rollback();
                        return null;
                    }
                }

                db.SaveChanges();
            }

            var lstVo = lstPo.Map<TaskVO>();
            foreach (var task in lstVo)
            {
                var taskGroup = await TaskGroupInfo.ToInfoAsync(task.TaskGroupId);
                task.Data        = Jsons.ToObject<Dictionary<string, string>>(taskGroup.Data);
                taskGroup.TaskId = task.Id;
                await Task.WhenAll(TaskUpdate.UpdateAsync(task), TaskGroupUpdate.UpdateAsync(taskGroup));
            }

            return lstVo;
        }
    }
}