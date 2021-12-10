using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FS.DI;
using FSS.Abstract.Entity.MetaInfo;
using FSS.Abstract.Server.MetaInfo;
using FSS.Application.Tasks.Tasks.Interface;
using FSS.Com.MetaInfoServer.Tasks.Dal;
using FSS.Infrastructure.Repository;

namespace FSS.Com.MetaInfoServer.Tasks
{
    public class TaskInfo : ITaskInfo
    {
        public TaskAgent TaskAgent { get; set; }
        public ITaskApp  TaskApp   { get; set; }

        /// <summary>
        /// 获取当前任务组的任务
        /// </summary>
        public Task<TaskVO> ToInfoByGroupIdAsync(int taskGroupId)
        {
            var key = CacheKeys.TaskForGroupKey;
            return RedisContext.Instance.CacheManager.GetItemAsync(key, taskGroupId, () => TaskApp.GetOrCreateAsync(taskGroupId));
        }

        /// <summary>
        /// 今日执行失败数量
        /// </summary>
        public Task<int> TodayFailCountAsync() => TaskAgent.TodayFailCountAsync();

        /// <summary>
        /// 计算任务的平均运行速度
        /// </summary>
        public async Task<long> StatAvgSpeedAsync(int taskGroupId)
        {
            var speedList = await TaskAgent.ToSpeedListAsync(taskGroupId);
            if (speedList.Count == 0) return 0;
            return speedList.Sum() / speedList.Count;
        }
    }
}