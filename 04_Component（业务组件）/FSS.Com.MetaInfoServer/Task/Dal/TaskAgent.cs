using System.Collections.Generic;
using FSS.Abstract.Enum;
using FSS.Com.MetaInfoServer.Abstract;

namespace FSS.Com.MetaInfoServer.Task.Dal
{
    /// <summary>
    /// 任务数据库层
    /// </summary>
    public class TaskAgent : ITaskAgent
    {
        /// <summary>
        /// 获取所有任务列表
        /// </summary>
        public List<TaskPO> ToList() => MetaInfoContext.Data.Task.ToList();

        /// <summary>
        /// 获取任务信息
        /// </summary>
        public TaskPO ToEntity(int id) => MetaInfoContext.Data.Task.Where(o => o.Id == id).ToEntity();

        /// <summary>
        /// 更新任务信息
        /// </summary>
        public void Update(int id, TaskPO task) => MetaInfoContext.Data.Task.Where(o => o.Id == id).Update(task);

        /// <summary>
        /// 添加任务信息
        /// </summary>
        public void Add(TaskPO task, out int id) => MetaInfoContext.Data.Task.Insert(task, out id);

        /// <summary>
        /// 获取未执行的任务信息
        /// </summary>
        public TaskPO ToUnExecutedTask(int groupId) => MetaInfoContext.Data.Task.Where(o => o.TaskGroupId == groupId && (o.Status == EumTaskType.None || o.Status == EumTaskType.Scheduler)).ToEntity();
    }
}