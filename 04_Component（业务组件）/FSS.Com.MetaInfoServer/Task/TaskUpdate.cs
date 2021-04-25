using FS.Extends;
using FSS.Abstract.Entity.MetaInfo;
using FSS.Abstract.Server.MetaInfo;
using FSS.Com.MetaInfoServer.Abstract;
using FSS.Com.MetaInfoServer.Task.Dal;

namespace FSS.Com.MetaInfoServer.Task
{
    public class TaskUpdate : ITaskUpdate
    {
        public ITaskCache TaskCache { get; set; }
        public ITaskAgent  TaskAgent { get; set; }

        /// <summary>
        /// 更新Task
        /// </summary>
        public void Update(TaskVO task)
        {
            TaskCache.Save(task.Id, task);
        }

        /// <summary>
        /// 保存Task
        /// </summary>
        public void Save(TaskVO task)
        {
            Update(task);
            TaskAgent.Update(task.Id, task.Map<TaskPO>());
        }
    }
}