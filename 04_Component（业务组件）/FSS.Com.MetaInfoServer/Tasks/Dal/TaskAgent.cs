using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FS.DI;
using FSS.Abstract.Enum;

namespace FSS.Com.MetaInfoServer.Tasks.Dal
{
    /// <summary>
    /// 任务数据库层
    /// </summary>
    public class TaskAgent : ISingletonDependency
    {
        /// <summary>
        /// 获取所有任务列表
        /// </summary>
        public Task<List<TaskPO>> ToTopListAsync(int top) => MetaInfoContext.Data.Task.Where(o => o.Status != EumTaskType.Success && o.Status != EumTaskType.Fail).Select(o => new { o.Id, o.Caption, o.Progress, o.Status, o.StartAt, o.CreateAt, o.ClientIp }).Desc(o => o.CreateAt).ToListAsync(top);

        /// <summary>
        /// 获取指定任务组执行成功的任务列表
        /// </summary>
        public Task<List<TaskPO>> ToSuccessListAsync(int groupId, int top) => MetaInfoContext.Data.Task.Where(o => o.TaskGroupId == groupId && o.Status == EumTaskType.Success).Desc(o => o.CreateAt).ToListAsync(top);

        /// <summary>
        /// 获取未执行的任务列表
        /// </summary>
        public Task<List<TaskPO>> ToNoneListAsync() => MetaInfoContext.Data.Task.Where(o => o.Status == EumTaskType.None).Asc(o => o.CreateAt).ToListAsync();

        /// <summary>
        /// 拉取3分钟内要执行的任务
        /// </summary>
        public Task<List<TaskPO>> ToNoneListAsync(int top, string[] clientJobs)
        {
            var minutes = DateTime.Now.AddMinutes(3);
            return MetaInfoContext.Data.Task.Where(o => o.Status == EumTaskType.None && clientJobs.ToList().Contains(o.JobName) && o.StartAt < minutes).Asc(o => o.StartAt).ToListAsync(top);
        }

        /// <summary>
        /// 清除成功的任务记录（1天前）
        /// </summary>
        public Task ClearSuccessAsync(int groupId, int taskId) => MetaInfoContext.Data.Task.Where(o => o.TaskGroupId == groupId && o.Status == EumTaskType.Success && o.CreateAt < DateTime.Now.AddDays(-1) && o.Id < taskId).DeleteAsync();

        /// <summary>
        /// 获取任务信息
        /// </summary>
        public Task<TaskPO> ToEntityAsync(int id) => MetaInfoContext.Data.Task.Where(o => o.Id == id).ToEntityAsync();

        /// <summary>
        /// 更新任务信息
        /// </summary>
        public Task UpdateAsync(int id, TaskPO task) => MetaInfoContext.Data.Task.Where(o => o.Id == id).UpdateAsync(task);

        /// <summary>
        /// 添加任务信息
        /// </summary>
        public Task AddAsync(TaskPO task) => MetaInfoContext.Data.Task.InsertAsync(task, true);

        /// <summary>
        /// 获取未执行的任务信息
        /// </summary>
        public Task<TaskPO> ToUnExecutedTaskAsync(int groupId) => MetaInfoContext.Data.Task.Where(o => o.TaskGroupId == groupId && (o.Status == EumTaskType.None || o.Status == EumTaskType.Scheduler)).ToEntityAsync();

        /// <summary>
        /// 取前100条的运行速度
        /// </summary>
        public Task<List<long>> ToSpeedListAsync(int groupId) => MetaInfoContext.Data.Task.Where(o => o.TaskGroupId == groupId && o.Status == EumTaskType.Success).Desc(o => o.CreateAt).ToSelectListAsync(100, o => o.RunSpeed.GetValueOrDefault());

        /// <summary>
        /// 今日执行失败数量
        /// </summary>
        public Task<int> TodayFailCountAsync() => MetaInfoContext.Data.Task.Where(o => o.Status == EumTaskType.Fail && o.CreateAt >= DateTime.Now.Date).CountAsync();
        
        /// <summary>
        /// 删除当前任务组下的所有
        /// </summary>
        public Task<int> DeleteAsync(int taskGroupId) => MetaInfoContext.Data.Task.Where(o => o.TaskGroupId == taskGroupId).DeleteAsync();
    }
}