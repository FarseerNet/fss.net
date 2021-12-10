using System.Collections.Generic;
using System.Threading.Tasks;
using FS.DI;
using FSS.Abstract.Entity.MetaInfo;

namespace FSS.Abstract.Server.MetaInfo
{
    public interface ITaskInfo: ISingletonDependency
    {
        /// <summary>
        /// 获取当前任务组的任务
        /// </summary>
        Task<TaskVO> ToInfoByGroupIdAsync(int taskGroupId);

    }
}