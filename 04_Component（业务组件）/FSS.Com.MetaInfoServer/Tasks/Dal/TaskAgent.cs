using System.Collections.Generic;
using System.Threading.Tasks;
using FSS.Abstract.Enum;
using FSS.Com.MetaInfoServer.Abstract;

namespace FSS.Com.MetaInfoServer.Tasks.Dal
{
    /// <summary>
    /// 任务数据库层
    /// </summary>
    public class TaskAgent : ITaskAgent
    {
        /// <summary>
        /// 获取所有任务列表
        /// </summary>
        public Task<List<TaskPO>> ToListAsync() => MetaInfoContext.Data.Task.ToListAsync();

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
        public Task<List<int>> ToSpeedListAsync(int groupId) => MetaInfoContext.Data.Task.Where(o => o.TaskGroupId == groupId && o.Status == EumTaskType.Success).Desc(o => o.Id).ToSelectListAsync(100, o => o.RunSpeed.GetValueOrDefault());
    }
}