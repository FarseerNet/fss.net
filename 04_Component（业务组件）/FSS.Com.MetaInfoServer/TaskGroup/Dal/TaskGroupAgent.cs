using System.Collections.Generic;
using FSS.Com.MetaInfoServer.Abstract;

namespace FSS.Com.MetaInfoServer.TaskGroup.Dal
{
    /// <summary>
    /// 任务组数据库层
    /// </summary>
    public class TaskGroupAgent : ITaskGroupAgent
    {
        /// <summary>
        /// 获取所有任务组列表
        /// </summary>
        public List<TaskGroupPO> ToList()
        {
            var lst = MetaInfoContext.Data.TaskGroup.ToList();
            return lst;
        }

        /// <summary>
        /// 获取任务组信息
        /// </summary>
        public TaskGroupPO ToEntity(int id) => MetaInfoContext.Data.TaskGroup.Where(o => o.Id == id).ToEntity();

        /// <summary>
        /// 更新任务组信息
        /// </summary>
        public void Update(int id, TaskGroupPO taskGroup) => MetaInfoContext.Data.TaskGroup.Where(o => o.Id == id).Update(taskGroup);

        /// <summary>
        /// 更新任务ID
        /// </summary>
        public void UpdateTaskId(int taskGroupId, int taskId) => MetaInfoContext.Data.TaskGroup.Where(o => o.Id == taskGroupId).Update(new TaskGroupPO {TaskId = taskId});
    }
}