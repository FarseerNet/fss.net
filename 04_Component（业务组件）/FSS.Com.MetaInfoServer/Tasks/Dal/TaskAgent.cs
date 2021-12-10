using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FS.DI;
using FSS.Infrastructure.Repository.Tasks.Enum;

namespace FSS.Com.MetaInfoServer.Tasks.Dal
{
    /// <summary>
    /// 任务数据库层
    /// </summary>
    public class TaskAgent : ISingletonDependency
    {
        /// <summary>
        /// 获取指定任务组执行成功的任务列表
        /// </summary>
        public Task<List<TaskPO>> ToFinishListAsync(int groupId, int top) => MetaInfoContext.Data.Task.Where(o => o.TaskGroupId == groupId && (o.Status == EumTaskType.Success ||o.Status == EumTaskType.Fail)).Desc(o => o.CreateAt).ToListAsync(top);

        /// <summary>
        /// 清除成功的任务记录（1天前）
        /// </summary>
        public Task ClearFinishAsync(int groupId, int taskId) => MetaInfoContext.Data.Task.Where(o => o.TaskGroupId == groupId && (o.Status == EumTaskType.Success ||o.Status == EumTaskType.Fail) && o.CreateAt < DateTime.Now.AddDays(-1) && o.Id < taskId).DeleteAsync();

        /// <summary>
        /// 取前100条的运行速度
        /// </summary>
        public Task<List<long>> ToSpeedListAsync(int groupId) => MetaInfoContext.Data.Task.Where(o => o.TaskGroupId == groupId && o.Status == EumTaskType.Success).Desc(o => o.CreateAt).ToSelectListAsync(100, o => o.RunSpeed.GetValueOrDefault());

        /// <summary>
        /// 今日执行失败数量
        /// </summary>
        public Task<int> TodayFailCountAsync() => MetaInfoContext.Data.Task.Where(o => o.Status == EumTaskType.Fail && o.CreateAt >= DateTime.Now.Date).CountAsync();

    }
}