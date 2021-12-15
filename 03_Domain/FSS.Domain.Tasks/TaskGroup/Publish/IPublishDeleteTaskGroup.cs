namespace FSS.Domain.Tasks.TaskGroup.Publish
{
    public interface IPublishDeleteTaskGroup
    {
        /// <summary>
        /// 发布删除任务组事件
        /// </summary>
        void Publish(object sender, int taskGroupId);
    }
}