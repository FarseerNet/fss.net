using System.Collections.Generic;
using FS.DI;
using FSS.Abstract.Entity.MetaInfo;

namespace FSS.Abstract.Server.MetaInfo
{
    public interface ITaskGroupCache: ITransientDependency
    {
        /// <summary>
        /// 保存任务组信息
        /// </summary>
        void Save(int taskGroupId, TaskGroupVO taskGroup);

        /// <summary>
        /// 当前任务组的列表
        /// </summary>
        List<TaskGroupVO> ToList();

        /// <summary>
        /// 获取任务组
        /// </summary>
        TaskGroupVO ToEntity(int taskGroupId);
    }
}