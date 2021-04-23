using System.Collections.Generic;

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
    }
}