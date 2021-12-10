using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FS.Core;
using FS.Extends;
using FSS.Infrastructure.Repository.TaskGroup.Interface;
using FSS.Infrastructure.Repository.Tasks.Enum;
using FSS.Infrastructure.Repository.Tasks.Interface;
using FSS.Infrastructure.Repository.Tasks.Model;
using Newtonsoft.Json;

namespace FSS.Infrastructure.Repository.Tasks
{
    /// <summary>
    /// 任务数据库层
    /// </summary>
    public class TaskAgent : ITaskAgent
    {
        public ITaskGroupAgent TaskGroupAgent { get; set; }
        
        /// <summary>
        /// 获取指定任务组执行成功的任务列表
        /// </summary>
        public Task<List<TaskPO>> ToFinishListAsync(int groupId, int top) => MysqlContext.Data.Task.Where(o => o.TaskGroupId == groupId && (o.Status == EumTaskType.Success || o.Status == EumTaskType.Fail)).Desc(o => o.CreateAt).ToListAsync(top);

        /// <summary>
        /// 清除成功的任务记录（1天前）
        /// </summary>
        public Task ClearFinishAsync(int groupId, int taskId) => MysqlContext.Data.Task.Where(o => o.TaskGroupId == groupId && (o.Status == EumTaskType.Success || o.Status == EumTaskType.Fail) && o.CreateAt < DateTime.Now.AddDays(-1) && o.Id < taskId).DeleteAsync();

        /// <summary>
        /// 取前100条的运行速度
        /// </summary>
        public Task<List<long>> ToSpeedListAsync(int groupId) => MysqlContext.Data.Task.Where(o => o.TaskGroupId == groupId && o.Status == EumTaskType.Success).Desc(o => o.CreateAt).ToSelectListAsync(100, o => o.RunSpeed.GetValueOrDefault());

        /// <summary>
        /// 今日执行失败数量
        /// </summary>
        public Task<int> TodayFailCountAsync() => MysqlContext.Data.Task.Where(o => o.Status == EumTaskType.Fail && o.CreateAt >= DateTime.Now.Date).CountAsync();

        /// <summary>
        /// 删除当前任务组下的所有
        /// </summary>
        public Task<int> DeleteAsync(int taskGroupId) => MysqlContext.Data.Task.Where(o => o.TaskGroupId == taskGroupId).DeleteAsync();

        private const string FinishTaskQueueKey = "FSS_FinishTaskQueue";

        /// <summary>
        /// 保存任务信息
        /// </summary>
        public Task SaveAsync(TaskPO task)
        {
            var key = CacheKeys.TaskForGroupKey;
            return RedisContext.Instance.CacheManager.SaveItemAsync(key, task);
        }

        /// <summary>
        /// 将Task写入队列中
        /// </summary>
        public Task AddQueueAsync(TaskPO task)
        {
            return Task.WhenAll(SaveAsync(task),
                                RedisContext.Instance.Db.SortedSetAddAsync(FinishTaskQueueKey, JsonConvert.SerializeObject(task), DateTime.Now.ToTimestamps()));
        }

        /// <summary>
        /// 队列中取出已完成的任务
        /// </summary>
        public async Task<List<TaskPO>> GetFinishTaskListAsync(int top)
        {
            var sortedSetEntries = await RedisContext.Instance.Db.SortedSetPopAsync(FinishTaskQueueKey, top);
            return sortedSetEntries.Select(s => Jsons.ToObject<TaskPO>(s.Element)).ToList();
        }

        /// <summary>
        /// 获取任务信息
        /// </summary>
        public Task<TaskPO> ToEntityAsync(int taskGroupId)
        {
            var key = CacheKeys.TaskForGroupKey;
            return RedisContext.Instance.CacheManager.GetItemAsync(key, taskGroupId);
        }
        
        /// <summary>
        /// 获取所有任务组中的任务
        /// </summary>
        public Task<List<TaskPO>> ToGroupListAsync()
        {
            var key = CacheKeys.TaskForGroupKey;
            return RedisContext.Instance.CacheManager.GetListAsync(key);
        }
    }
}