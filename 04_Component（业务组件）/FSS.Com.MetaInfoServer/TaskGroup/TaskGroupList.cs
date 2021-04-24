using System.Collections.Generic;
using FS.Extends;
using FSS.Abstract.Entity.MetaInfo;
using FSS.Abstract.Server.MetaInfo;
using FSS.Com.MetaInfoServer.Abstract;

namespace FSS.Com.MetaInfoServer.TaskGroup
{
    /// <summary>
    /// 任务列表
    /// </summary>
    public class TaskGroupList : ITaskGroupList
    {
        public ITaskGroupAgent TaskGroupAgent { get; set; }

        /// <summary>
        /// 获取全部任务列表
        /// </summary>
        public List<TaskGroupVO> ToList() => TaskGroupAgent.ToList().Map<TaskGroupVO>();
    }
}