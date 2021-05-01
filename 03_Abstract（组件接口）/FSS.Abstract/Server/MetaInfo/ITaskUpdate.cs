using FS.DI;
using FSS.Abstract.Entity.MetaInfo;

namespace FSS.Abstract.Server.MetaInfo
{
    public interface ITaskUpdate: ITransientDependency
    {
        /// <summary>
        /// 更新Task（如果状态是成功、失败、重新调度，则应该调Save）
        /// </summary>
        void Update(TaskVO task);
        
        /// <summary>
        /// 保存Task
        /// </summary>
        void Save(TaskVO task);
    }
}