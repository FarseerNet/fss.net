using System.Threading.Tasks;
using FSS.Abstract.Entity.MetaInfo;
using FSS.Abstract.Server.MetaInfo;
using FSS.Com.MetaInfoServer.Tasks.Dal;

namespace FSS.Com.MetaInfoServer.Tasks
{
    public class TaskUpdate : ITaskUpdate
    {
        public TaskCache TaskCache { get; set; }

        /// <summary>
        /// 更新Task（如果状态是成功、失败、重新调度，则应该调Save）
        /// </summary>
        public Task UpdateAsync(TaskVO task) => TaskCache.SaveAsync(task);
    }
}