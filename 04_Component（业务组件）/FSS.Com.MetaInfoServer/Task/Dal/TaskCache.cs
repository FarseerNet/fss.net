using System.Collections.Generic;
using System.Linq;
using FS.Cache.Redis;
using FSS.Abstract.Entity.MetaInfo;
using FSS.Abstract.Entity.RegisterCenter;
using FSS.Abstract.Server.MetaInfo;
using Newtonsoft.Json;

namespace FSS.Com.MetaInfoServer.Task.Dal
{
    /// <summary>
    /// 任务缓存
    /// </summary>
    // ReSharper disable once UnusedType.Global
    public class TaskCache : ITaskCache
    {
        public IRedisCacheManager RedisCacheManager { get; set; }

        private string key = "Task";

        /// <summary>
        /// 保存任务信息
        /// </summary>
        public void Save(int taskId, TaskVO task)
        {
            RedisCacheManager.Db.HashSet(key, taskId, JsonConvert.SerializeObject(task));
        }

        /// <summary>
        /// 当前任务的列表
        /// </summary>
        public List<TaskVO> ToList()
        {
            return RedisCacheManager.Db.HashGetAll(key).Select(o => JsonConvert.DeserializeObject<TaskVO>(o.Value)).ToList();
        }

        /// <summary>
        /// 获取任务
        /// </summary>
        public TaskVO ToEntity(int taskId)
        {
            var redisValue = RedisCacheManager.Db.HashGet(key, taskId);
            return !redisValue.HasValue ? null : JsonConvert.DeserializeObject<TaskVO>(redisValue.ToString());
        }
    }
}