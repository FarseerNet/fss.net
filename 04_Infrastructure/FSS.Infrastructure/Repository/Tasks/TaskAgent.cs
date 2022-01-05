using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FS.Core;
using FS.DI;
using FSS.Domain.Tasks.TaskGroup.Enum;
using FSS.Infrastructure.Repository.Tasks.Model;

namespace FSS.Infrastructure.Repository.Tasks
{
    /// <summary>
    /// 任务数据库层
    /// </summary>
    public class TaskAgent: ISingletonDependency
    {
        /// <summary>
        /// 将任务暂时写入redis集合，再通过job集中写入数据库
        /// </summary>
        public async Task<int> AddToDbAsync(List<TaskPO> lstTask)
        {
            using (var db = new MysqlContext())
            {
                foreach (var taskPO in lstTask)
                {
                    taskPO.Id = null;
                    await db.Task.InsertAsync(taskPO);
                }
                db.SaveChanges();
            }

            return lstTask.Count;
        }

        /// <summary>
        /// 获取指定任务组执行成功的任务列表
        /// </summary>
        public Task<List<TaskPO>> ToFinishListAsync(int groupId, int top) => MysqlContext.Data.Task.Where(o => o.TaskGroupId == groupId && (o.Status == EumTaskType.Success || o.Status == EumTaskType.Fail)).Desc(o => o.CreateAt).ToListAsync(top);

        /// <summary>
        /// 获取已完成的任务列表
        /// </summary>
        public Task<PageList<TaskPO>> ToFinishPageListAsync(int pageSize, int pageIndex)
        {
            return MysqlContext.Data.Task.Where(o => (o.Status == EumTaskType.Fail || o.Status == EumTaskType.Success) && o.CreateAt >= DateTime.Now.AddDays(-1))
                               .Select(o => new { o.Id, o.Caption, o.Progress, o.Status, o.StartAt, o.CreateAt, o.ClientIp, o.RunSpeed, o.RunAt, o.JobName })
                               .Desc(o => o.RunAt).ToPageListAsync(pageSize, pageIndex);
        }
        
        /// <summary>
        /// 清除成功的任务记录（1天前）
        /// </summary>
        public Task ClearFinishAsync(int groupId, int taskId) => MysqlContext.Data.Task.Where(o => o.TaskGroupId == groupId && (o.Status == EumTaskType.Success || o.Status == EumTaskType.Fail) && o.CreateAt < DateTime.Now.AddDays(-1) && o.Id < taskId).DeleteAsync();

        /// <summary>
        /// 取前100条的运行速度
        /// </summary>
        public Task<List<long>> ToSpeedListAsync(int taskGroupId) => MysqlContext.Data.Task.Where(o => o.TaskGroupId == taskGroupId && o.Status == EumTaskType.Success).Desc(o => o.CreateAt).ToSelectListAsync(100, o => o.RunSpeed.GetValueOrDefault());

        /// <summary>
        /// 今日执行失败数量
        /// </summary>
        public Task<int> TodayFailCountAsync() => MysqlContext.Data.Task.Where(o => o.Status == EumTaskType.Fail && o.CreateAt >= DateTime.Now.Date).CountAsync();

        /// <summary>
        /// 删除当前任务组下的所有
        /// </summary>
        public Task<int> DeleteAsync(int taskGroupId) => MysqlContext.Data.Task.Where(o => o.TaskGroupId == taskGroupId).DeleteAsync();

        /// <summary>
        /// 获取指定任务组的任务列表（FOPS）
        /// </summary>
        public Task<PageList<TaskPO>> ToListAsync(int groupId, int pageSize, int pageIndex)
        {
            return MysqlContext.Data.Task.Where(o => o.TaskGroupId == groupId)
                                  .Select(o => new { o.Id, o.Caption, o.Progress, o.Status, o.StartAt, o.CreateAt, o.ClientIp, o.RunSpeed, o.RunAt })
                                  .Desc(o => o.CreateAt).ToPageListAsync(pageSize, pageIndex);
        }

    }
}