using System.Collections.Generic;
using System.Linq;
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

        public const string Key = "TaskGroup";

        /// <summary>
        /// 保存任务组信息
        /// </summary>
        public void Save(int taskGroupId, TaskGroupVO taskGroup)
        {
            RedisCacheManager.Db.HashSet(Key, taskGroupId, JsonConvert.SerializeObject(taskGroup));
        }

        /// <summary>
        /// 当前任务组的列表
        /// </summary>
        public List<TaskGroupVO> ToList()
        {
            return RedisCacheManager.Db.HashGetAll(Key).Select(o => JsonConvert.DeserializeObject<TaskGroupVO>(o.Value)).ToList();
        }

        /// <summary>
        /// 获取任务组
        /// </summary>
        public TaskGroupVO ToEntity(int taskGroupId)
        {
            var redisValue = RedisCacheManager.Db.HashGet(Key, taskGroupId);
            return !redisValue.HasValue ? null : JsonConvert.DeserializeObject<TaskGroupVO>(redisValue.ToString());
        }
    }
}