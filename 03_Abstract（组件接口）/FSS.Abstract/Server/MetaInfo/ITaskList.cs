using System.Collections.Generic;
using FS.DI;
using FSS.Abstract.Entity.MetaInfo;

namespace FSS.Abstract.Server.MetaInfo
{
    public interface ITaskList: ITransientDependency
    {
        /// <summary>
        /// 获取全部任务列表
        /// </summary>
        List<TaskVO> ToList();
    }
}