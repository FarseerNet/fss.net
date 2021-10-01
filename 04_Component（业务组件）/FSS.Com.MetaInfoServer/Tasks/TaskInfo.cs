using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FS.Extends;
using FSS.Abstract.Entity.MetaInfo;
using FSS.Abstract.Server.MetaInfo;
using FSS.Com.MetaInfoServer.Tasks.Dal;

namespace FSS.Com.MetaInfoServer.Tasks
{
    public class TaskInfo : ITaskInfo
    {
        public TaskAgent      TaskAgent     { get; set; }
        public TaskCache      TaskCache     { get; set; }

        /// <summary>
        /// 获取任务信息
        /// </summary>
        public Task<TaskVO> ToInfoByDbAsync(int id) => TaskAgent.ToEntityAsync(id).MapAsync<TaskVO, TaskPO>();

        /// <summary>
        /// 获取当前任务组的任务
        /// </summary>
        public Task<TaskVO> ToInfoByGroupIdAsync(int taskGroupId) => TaskCache.ToEntityAsync(taskGroupId);

        /// <summary>
        /// 获取所有任务组
        /// </summary>
        public Task<List<TaskVO>> ToGroupListAsync() => TaskCache.ToListAsync();

        /// <summary>
        /// 计算任务的平均运行速度
        /// </summary>
        public async Task<long> StatAvgSpeedAsync(int taskGroupId)
        {
            var speedList = await TaskAgent.ToSpeedListAsync(taskGroupId);
            if (speedList.Count == 0) return 0;
            return speedList.Sum() / speedList.Count;
        }

        /// <summary>
        /// 今日执行失败数量
        /// </summary>
        public Task<int> TodayFailCountAsync() => TaskAgent.TodayFailCountAsync();
    }
}