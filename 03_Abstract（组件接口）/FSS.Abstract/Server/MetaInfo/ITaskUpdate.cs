using FS.DI;
using FSS.Abstract.Entity.MetaInfo;

namespace FSS.Abstract.Server.MetaInfo
{
    public interface ITaskUpdate: ITransientDependency
    {
        /// <summary>
        /// 更新Task
        /// </summary>
        void Update(TaskVO task);
        
        /// <summary>
        /// 保存Task
        /// </summary>
        void Save(TaskVO task);
    }
}