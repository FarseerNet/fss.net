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
        public const string Key = "Task";
    }
}