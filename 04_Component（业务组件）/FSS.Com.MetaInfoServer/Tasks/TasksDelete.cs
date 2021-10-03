using System.Threading.Tasks;
using FSS.Abstract.Server.MetaInfo;
using FSS.Com.MetaInfoServer.Tasks.Dal;

namespace FSS.Com.MetaInfoServer.Tasks
{
    public class TasksDelete : ITasksDelete
    {
        public TaskAgent TaskAgent { get; set; }

        /// <summary>
        /// 删除任务组
        /// </summary>
        public Task DeleteAsync(int taskGroupId) => TaskAgent.DeleteAsync(taskGroupId);
    }
}