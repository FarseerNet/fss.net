using FS.DI;
using FSS.Abstract.Entity.MetaInfo;

namespace FSS.Abstract.Server.MetaInfo
{
    public interface ITaskGroupUpdate: ITransientDependency
    {
        /// <summary>
        /// 更新TaskGroup
        /// </summary>
        void Update(TaskGroupVO taskGroup);
    }
}