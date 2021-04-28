using FS.DI;
using FSS.Abstract.Entity.MetaInfo;

namespace FSS.Abstract.Server.MetaInfo
{
    public interface ITaskAdd: ITransientDependency
    {
        /// <summary>
        /// 创建Task，并更新到缓存
        /// </summary>
        TaskVO GetOrCreate(TaskGroupVO taskGroup);

        /// <summary>
        /// 创建Task，并更新到缓存
        /// </summary>
        TaskVO GetOrCreate(int taskGroupId);
    }
}