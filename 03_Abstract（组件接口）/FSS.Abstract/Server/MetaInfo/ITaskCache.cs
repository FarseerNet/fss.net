using System.Collections.Generic;
using FS.DI;
using FSS.Abstract.Entity.MetaInfo;

namespace FSS.Abstract.Server.MetaInfo
{
    public interface ITaskCache: ITransientDependency
    {
        /// <summary>
        /// 保存任务信息
        /// </summary>
        void Save(int taskId, TaskVO task);

        /// <summary>
        /// 当前任务的列表
        /// </summary>
        List<TaskVO> ToList();

        /// <summary>
        /// 获取任务
        /// </summary>
        TaskVO ToEntity(int taskId);
    }
}