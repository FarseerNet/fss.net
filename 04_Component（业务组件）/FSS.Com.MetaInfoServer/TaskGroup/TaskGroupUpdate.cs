using System.Threading.Tasks;
using FS.Extends;
using FSS.Abstract.Entity.MetaInfo;
using FSS.Abstract.Server.MetaInfo;
using FSS.Com.MetaInfoServer.TaskGroup.Dal;

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
        public Task UpdateAsync(TaskGroupVO vo) => TaskGroupCache.SaveAsync(vo);

        /// <summary>
        /// 保存TaskGroup
        /// </summary>
        public async Task SaveAsync(TaskGroupVO vo)
        {
            await UpdateAsync(vo);
            await TaskGroupAgent.UpdateAsync(vo.Id, vo.Map<TaskGroupPO>());
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