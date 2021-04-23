using FS.DI;
using FSS.Abstract.Entity.MetaInfo;

namespace FSS.Abstract.Server.MetaInfo
{
    public interface ITaskInfo: ITransientDependency
    {
        /// <summary>
        /// 获取任务信息
        /// </summary>
        TaskVO ToInfo(int id);
    }
}