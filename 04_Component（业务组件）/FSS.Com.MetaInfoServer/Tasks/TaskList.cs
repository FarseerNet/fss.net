using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FS.Core;
using FS.DI;
using FS.Extends;
using FSS.Abstract.Entity;
using FSS.Abstract.Entity.MetaInfo;
using FSS.Abstract.Enum;
using FSS.Abstract.Server.MetaInfo;
using FSS.Com.MetaInfoServer.TaskGroup;
using FSS.Com.MetaInfoServer.Tasks.Dal;
using FSS.Infrastructure.Repository;
using Microsoft.Extensions.Logging;

namespace FSS.Com.MetaInfoServer.Tasks
{
    /// <summary>
    /// 任务列表
    /// </summary>
    public class TaskList : ITaskList
    {
        public TaskAgent      TaskAgent     { get; set; }
        public ITaskGroupInfo TaskGroupInfo { get; set; }
        public ITaskUpdate    TaskUpdate    { get; set; }
        public ITaskGroupList TaskGroupList { get; set; }
        public ITaskAdd       TaskAdd       { get; set; }


        /// <summary>
        /// 获取所有任务组中的任务
        /// </summary>
        public Task<List<TaskVO>> ToGroupListAsync()
        {
            var key = CacheKeys.TaskForGroupKey;
            return RedisContext.Instance.CacheManager.GetListAsync(key, async () =>
            {
                var taskGroupVos = await TaskGroupList.ToListInCacheAsync();
                var lst          = new List<TaskVO>();
                foreach (var taskGroupVo in taskGroupVos)
                {
                    lst.Add(await TaskAdd.GetOrCreateAsync(taskGroupVo.Id));
                }

                return lst;
            });
        }

        /// <summary>
        /// 获取指定任务组的任务列表（FOPS）
        /// </summary>
        public Task<List<TaskVO>> ToListAsync(int groupId, int pageSize, int pageIndex, out int totalCount)
        {
            return MetaInfoContext.Data.Task.Where(o => o.TaskGroupId == groupId)
                                  .Select(o => new { o.Id, o.Caption, o.Progress, o.Status, o.StartAt, o.CreateAt, o.ClientIp, o.RunSpeed, o.RunAt })
                                  .Desc(o => o.CreateAt).ToListAsync(pageSize, pageIndex, out totalCount).MapAsync<TaskVO, TaskPO>();
        }

        /// <summary>
        /// 获取已完成的任务列表
        /// </summary>
        public Task<List<TaskVO>> ToFinishListAsync(int pageSize, int pageIndex, out int totalCount)
        {
            return MetaInfoContext.Data.Task.Where(o => (o.Status == EumTaskType.Fail || o.Status == EumTaskType.Success) && o.CreateAt >= DateTime.Now.AddDays(-1))
                                  .Select(o => new { o.Id, o.Caption, o.Progress, o.Status, o.StartAt, o.CreateAt, o.ClientIp, o.RunSpeed, o.RunAt, o.JobName })
                                  .Desc(o => o.RunAt).ToListAsync(pageSize, pageIndex, out totalCount).MapAsync<TaskVO, TaskPO>();
        }

        /// <summary>
        /// 获取指定任务组执行成功的任务列表
        /// </summary>
        public Task<List<TaskVO>> ToSuccessListAsync(int groupId, int top) => TaskAgent.ToSuccessListAsync(groupId, top).MapAsync<TaskVO, TaskPO>();

        /// <summary>
        /// 清除成功的任务记录（1天前）
        /// </summary>
        public Task ClearSuccessAsync(int groupId, int taskId) => TaskAgent.ClearSuccessAsync(groupId, taskId);


        /// <summary>
        /// 获取未执行的任务数量
        /// </summary>
        public async Task<int> ToUnRunCountAsync()
        {
            try
            {
                var lst = await ToGroupListAsync();
                return lst.Count(o => o.StartAt < DateTime.Now && (o.Status == EumTaskType.None || o.Status == EumTaskType.Scheduler));
            }
            catch (Exception e)
            {
                IocManager.Instance.Logger<TaskGroupList>().LogError(e, e.Message);
                return 0;
            }
        }

        /// <summary>
        /// 获取执行中的任务
        /// </summary>
        public async Task<List<TaskVO>> ToSchedulerWorkingListAsync()
        {
            var lst = await ToGroupListAsync();
            return lst.Where(o => o.Status == EumTaskType.Scheduler || o.Status == EumTaskType.Working).ToList();
        }

        /// <summary>
        /// 拉取指定数量的任务，并将任务设为已调度状态
        /// </summary>
        public async Task<List<TaskVO>> PullTaskAsync(ClientVO client, int requestTaskCount)
        {
            if (requestTaskCount == 0)
            {
                requestTaskCount = 3;
            }

            var lstTask = await ToGroupListAsync();
            lstTask = lstTask.Where(o => o.Status == EumTaskType.None && client.Jobs.Contains(o.JobName) && o.StartAt < DateTime.Now.AddMinutes(3)).OrderBy(o => o.StartAt).Take(requestTaskCount).ToList();
            if (lstTask == null || !lstTask.Any()) return null;

            // 更新任务状态
            // 更新任务为已调度
            foreach (var task in lstTask)
            {
                var taskGroup = await TaskGroupInfo.ToInfoAsync(task.TaskGroupId);
                task.Status      = EumTaskType.Scheduler;
                task.ClientIp    = client.ClientIp;
                task.ClientName  = client.ClientName;
                task.ClientId    = client.Id;
                task.SchedulerAt = DateTime.Now;
                task.Data        = Jsons.ToObject<Dictionary<string, string>>(taskGroup.Data);
                await TaskUpdate.UpdateAsync(task);
            }

            return lstTask;
        }
    }
}