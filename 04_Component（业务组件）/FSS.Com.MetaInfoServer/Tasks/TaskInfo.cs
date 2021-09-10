using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FS.Cache.Redis;
using FS.Core;
using FS.DI;
using FS.Extends;
using FSS.Abstract.Entity.MetaInfo;
using FSS.Abstract.Server.MetaInfo;
using FSS.Com.MetaInfoServer.Abstract;
using FSS.Com.MetaInfoServer.Tasks.Dal;
using Newtonsoft.Json;

namespace FSS.Com.MetaInfoServer.Tasks
{
    public class TaskInfo : ITaskInfo
    {
        public  ITaskAgent         TaskAgent         { get; set; }
        private IRedisCacheManager RedisCacheManager => IocManager.Instance.Resolve<IRedisCacheManager>();
        public  ITaskAdd           TaskAdd           { get; set; }
        public  ITaskGroupList     TaskGroupList     { get; set; }

        /// <summary>
        /// 获取任务信息
        /// </summary>
        public Task<TaskVO> ToInfoByDbAsync(int id) => TaskAgent.ToEntityAsync(id).MapAsync<TaskVO, TaskPO>();

        /// <summary>
        /// 获取当前任务组的任务
        /// </summary>
        public Task<TaskVO> ToInfoByGroupIdAsync(int taskGroupId)
        {
            return RedisCacheManager.CacheManager.ToEntityAsync(TaskCache.Key,
                taskGroupId.ToString(),
                o => TaskAdd.GetOrCreateAsync(taskGroupId),
                o => o.TaskGroupId);
        }

        /// <summary>
        /// 获取所有任务组
        /// </summary>
        public Task<List<TaskVO>> ToGroupListAsync()
        {
            return RedisCacheManager.CacheManager.GetListAsync(TaskCache.Key,
                async _ =>
                {
                    var taskGroupVos = await TaskGroupList.ToListAsync();
                    var lst          = new List<TaskVO>();
                    foreach (var taskGroupVo in taskGroupVos)
                    {
                        lst.Add(await TaskAdd.GetOrCreateAsync(taskGroupVo.Id));
                    }

                    return lst;
                }, o => o.TaskGroupId);
        }

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