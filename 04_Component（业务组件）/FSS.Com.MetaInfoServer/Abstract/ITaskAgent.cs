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

        /// <summary>
        /// 更新任务信息
        /// </summary>
        void Update(int id, TaskPO task);

        /// <summary>
        /// 添加任务信息
        /// </summary>
        void Add(TaskPO task, out int id);

        /// <summary>
        /// 获取未执行的任务信息
        /// </summary>
        TaskPO ToUnExecutedTask(int groupId);
    }
}