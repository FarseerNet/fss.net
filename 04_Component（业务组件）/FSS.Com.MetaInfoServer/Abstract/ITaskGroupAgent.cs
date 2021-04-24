using System.Collections.Generic;
using FS.DI;
using FSS.Com.MetaInfoServer.Task.Dal;
using FSS.Com.MetaInfoServer.TaskGroup.Dal;

namespace FSS.Com.MetaInfoServer.Abstract
{
    public interface ITaskGroupAgent : ITransientDependency
    {
        /// <summary>
        /// 获取所有任务列表
        /// </summary>
        List<TaskGroupPO> ToList();
        
        /// <summary>
        /// 获取任务信息
        /// </summary>
        TaskGroupPO ToEntity(int id);
    }
}