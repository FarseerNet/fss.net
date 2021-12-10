using System.Threading.Tasks;
using FS.DI;
using FSS.Application.Tasks.TaskGroup.Entity;

namespace FSS.Application.Tasks.TaskGroup.Interface
{
    public interface ITaskGroupApp: ISingletonDependency
    {
        /// <summary>
        /// 添加任务组信息
        /// </summary>
        Task<int> AddAsync(TaskGroupDTO dto);
        /// <summary>
        /// 取消任务
        /// </summary>
        Task CancelTask(int groupId);
    }
}