using FS.DI;
using FSS.Domain.Tasks.TaskGroup.Entity;

namespace FSS.Domain.Tasks.TaskGroup.Publish
{
    public interface ITaskFinishPublish: ISingletonDependency
    {

        /// <summary>
        /// 发布任务组完成事件
        /// </summary>
        void Publish(object sender, TaskGroupDO taskGroup);
    }
}