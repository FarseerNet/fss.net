using FS.Extends;
using FSS.Abstract.Entity.MetaInfo;
using FSS.Abstract.Server.MetaInfo;
using FSS.Com.MetaInfoServer.Abstract;
using FSS.Com.MetaInfoServer.TaskGroup.Dal;

namespace FSS.Com.MetaInfoServer.TaskGroup
{
    public class TaskGroupUpdate : ITaskGroupUpdate
    {
        public ITaskGroupCache TaskGroupCache { get; set; }
        public ITaskGroupAgent TaskGroupAgent { get; set; }

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
            TaskGroupAgent.Update(taskGroup.Id, taskGroup.Map<TaskGroupPO>());
        }
    }
}