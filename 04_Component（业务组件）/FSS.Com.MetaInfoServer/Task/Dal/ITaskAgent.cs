using System.Collections.Generic;
using FS.DI;

namespace FSS.Com.MetaInfoServer.Task.Dal
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