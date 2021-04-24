using System.Collections.Generic;
using FS.DI;
using FSS.Com.MetaInfoServer.Task.Dal;

namespace FSS.Com.MetaInfoServer.Abstract
{
    public interface ITaskAgent : ITransientDependency
    {
        /// <summary>
        /// 获取所有任务列表
        /// </summary>
        List<TaskPO> ToList();
        
        /// <summary>
        /// 获取任务信息
        /// </summary>
        TaskPO ToEntity(int id);
    }
}