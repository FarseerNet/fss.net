using FS.Cache;
using FS.Extends;
using FSS.Abstract.Entity.MetaInfo;
using FSS.Abstract.Server.MetaInfo;
using FSS.Com.MetaInfoServer.Abstract;
using FSS.Com.MetaInfoServer.Task.Dal;

namespace FSS.Com.MetaInfoServer.Task
{
    public class TaskUpdate : ITaskUpdate
    {
        public ICacheManager CacheManager { get; set; }
        public ITaskAgent    TaskAgent    { get; set; }

        /// <summary>
        /// 更新Task
        /// </summary>
        public void Update(TaskVO task)
        {
            CacheManager.Save(TaskCache.Key, task, task.Id,new CacheOption());
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