using System.Collections.Generic;
using FS.DI;
using FSS.Abstract.Entity.MetaInfo;

namespace FSS.Abstract.Server.MetaInfo
{
    public interface ITaskGroupList: ITransientDependency
    {
        /// <summary>
        /// 获取全部任务列表
        /// </summary>
        List<TaskGroupVO> ToList();

        /// <summary>
        /// 删除整个缓存
        /// </summary>
        void Clear();

        /// <summary>
        /// 获取全部任务列表
        /// </summary>
        List<TaskGroupVO> ToListAndSave();
    }
}