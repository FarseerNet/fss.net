using System.Collections.Generic;
using FS.DI;
using FS.Extends;
using FSS.Abstract.Entity.MetaInfo;
using FSS.Abstract.Server.MetaInfo;
using FSS.Com.MetaInfoServer.Task.Dal;

namespace FSS.Com.MetaInfoServer.Task
{
    /// <summary>
    /// 任务列表
    /// </summary>
    public class TaskList : ITaskList
    {
        public ITaskAgent TaskAgent { get; set; }

        /// <summary>
        /// 获取全部任务列表
        /// </summary>
        public List<TaskVO> ToList() => TaskAgent.ToList().Map<TaskVO>();
    }
}