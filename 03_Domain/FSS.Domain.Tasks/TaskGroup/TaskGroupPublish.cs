using FS.DI;
using FS.EventBus;
using FSS.Domain.Tasks.TaskGroup.Entity;

namespace FSS.Domain.Tasks.TaskGroup
{
    public class TaskGroupPublish : ISingletonDependency
    {
        /// <summary>
        /// 发布删除任务组事件
        /// </summary>
        public void DeleteTaskGroup(object sender, int taskGroupId)
        {
            IocManager.GetService<IEventProduct>("DeleteTaskGroup").SendSync(sender, taskGroupId);
        }
        
        /// <summary>
        /// 发布添加任务组事件
        /// </summary>
        public void CreateTaskGroup(object sender, int taskGroupId)
        {
            IocManager.GetService<IEventProduct>("CreateTaskGroup").SendSync(sender, taskGroupId);
        }
        
        /// <summary>
        /// 发布任务组开启事件
        /// </summary>
        public void EnableTaskGroup(object sender, int taskGroupId)
        {
            IocManager.GetService<IEventProduct>("EnableTaskGroup").SendSync(sender, taskGroupId);
        }
        
        /// <summary>
        /// 发布任务组停止事件
        /// </summary>
        public void DisEnableTaskGroup(object sender, int taskGroupId)
        {
            IocManager.GetService<IEventProduct>("DisEnableTaskGroup").SendSync(sender, taskGroupId);
        }
        
        /// <summary>
        /// 发布任务组停止事件
        /// </summary>
        public void TaskFinish(object sender, TaskGroupDO taskGroup)
        {
            IocManager.GetService<IEventProduct>("TaskFinish").SendSync(sender, taskGroup);
        }
    }
}