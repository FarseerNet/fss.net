using System.Threading.Tasks;
using FS.Extends;
using FSS.Abstract.Entity.MetaInfo;
using FSS.Abstract.Server.MetaInfo;
using FSS.Com.MetaInfoServer.TaskGroup.Dal;
using FSS.Domain.Tasks.TaskGroup.Entity;

namespace FSS.Com.MetaInfoServer.TaskGroup
{
    public class TaskGroupUpdate : ITaskGroupUpdate
    {
        public TaskGroupCache TaskGroupCache { get; set; }
        public ITaskGroupList  TaskGroupList  { get; set; }
        public TaskGroupAgent TaskGroupAgent { get; set; }

        /// <summary>
        /// 更新TaskGroup
        /// </summary>
        public Task UpdateAsync(TaskGroupDO @do) => TaskGroupCache.SaveAsync(@do);

        /// <summary>
        /// 保存TaskGroup
        /// </summary>
        public async Task SaveAsync(TaskGroupDO @do)
        {
            await TaskGroupCache.SaveAsync(@do);
            await TaskGroupAgent.UpdateAsync(@do.Id, @do.Map<TaskGroupPO>());
        }

        /// <summary>
        /// 同步缓存到数据库
        /// </summary>
        public async Task SyncCacheToDb()
        {
            var lstTaskGroup = await TaskGroupList.ToListInCacheAsync();
            foreach (var taskGroupVO in lstTaskGroup)
            {
                await TaskGroupAgent.UpdateAsync(taskGroupVO.Id, taskGroupVO.Map<TaskGroupPO>());
            }
        }
    }
}