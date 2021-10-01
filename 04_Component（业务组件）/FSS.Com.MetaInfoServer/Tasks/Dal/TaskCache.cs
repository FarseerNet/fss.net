using System.Collections.Generic;
using System.Threading.Tasks;
using FS.DI;
using FS.Extends;
using FSS.Abstract.Entity.MetaInfo;
using FSS.Abstract.Server.MetaInfo;
using FSS.Com.MetaInfoServer.TaskGroup.Dal;
using FSS.Infrastructure.Repository;

namespace FSS.Com.MetaInfoServer.Tasks.Dal
{
    /// <summary>
    /// 任务缓存
    /// </summary>
    // ReSharper disable once UnusedType.Global
    public class TaskCache : ISingletonDependency
    {
        public static string FailKey(int groupId) => $"FSS_Task_Fail:{groupId}";

        public ITaskAdd       TaskAdd       { get; set; }
        public ITaskGroupList TaskGroupList { get; set; }

        /// <summary>
        /// 保存任务信息
        /// </summary>
        public Task SaveAsync(TaskVO task)
        {
            var key = CacheKeys.TaskForGroupKey;
            return RedisContext.Instance.CacheManager.SaveItemAsync(key, task, task.TaskGroupId);
        }

        /// <summary>
        /// 获取任务
        /// </summary>
        public Task<TaskVO> ToEntityAsync(int taskGroupId)
        {
            var key = CacheKeys.TaskForGroupKey;
            return RedisContext.Instance.CacheManager.GetItemAsync<TaskVO, int>(key, taskGroupId, () => TaskAdd.GetOrCreateAsync(taskGroupId), o => o.TaskGroupId);
        }

        /// <summary>
        /// 当前任务的列表
        /// </summary>
        public Task<List<TaskVO>> ToListAsync()
        {
            var key = CacheKeys.TaskForGroupKey;
            return RedisContext.Instance.CacheManager.GetListAsync(key, async () =>
            {
                var taskGroupVos = await TaskGroupList.ToListInCacheAsync();
                var lst          = new List<TaskVO>();
                foreach (var taskGroupVo in taskGroupVos)
                {
                    lst.Add(await TaskAdd.GetOrCreateAsync(taskGroupVo.Id));
                }

                return lst;
            }, o => o.TaskGroupId);
        }
    }
}