using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FS.Cache.Redis;
using FSS.Abstract.Entity.MetaInfo;
using FSS.Abstract.Server.MetaInfo;
using Newtonsoft.Json;

namespace FSS.Com.MetaInfoServer.TaskGroup.Dal
{
    /// <summary>
    /// 任务组缓存
    /// </summary>
    // ReSharper disable once UnusedType.Global
    public class TaskGroupCache : ITaskGroupCache
    {
        public IRedisCacheManager RedisCacheManager { get; set; }

        public const string Key = "FSS_TaskGroup";

        /// <summary>
        /// 保存任务组信息
        /// </summary>
        public Task SaveAsync(int taskGroupId, TaskGroupVO taskGroup)
        {
            return RedisCacheManager.Db.HashSetAsync(Key, taskGroupId, JsonConvert.SerializeObject(taskGroup));
        }

        /// <summary>
        /// 当前任务组的列表
        /// </summary>
        public async Task<List<TaskGroupVO>> ToListAsync()
        {
            var hashGetAllAsync = await RedisCacheManager.Db.HashGetAllAsync(Key);
            return hashGetAllAsync.Select(o => JsonConvert.DeserializeObject<TaskGroupVO>(o.Value)).ToList();
        }

        /// <summary>
        /// 获取任务组
        /// </summary>
        public async Task<TaskGroupVO> ToEntityAsync(int taskGroupId)
        {
            var redisValue = await RedisCacheManager.Db.HashGetAsync(Key, taskGroupId);
            return !redisValue.HasValue ? null : JsonConvert.DeserializeObject<TaskGroupVO>(redisValue.ToString());
        }
    }
}