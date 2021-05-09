using System;
using System.Collections.Generic;
using FS.ElasticSearch;
using FS.ElasticSearch.Map;
using FSS.Com.MetaInfoServer.RunLog.Dal;

namespace FSS.Com.MetaInfoServer
{
    /// <summary>
    /// ES日志上下文
    /// </summary>
    public class LogContext : EsContext<LogContext>
    {
        public LogContext() : base("default")
        {
        }

        protected override void CreateModelInit(Dictionary<string, SetDataMap> map)
        {
            map["RunLog"].SetName($"FssLog_{DateTime.Now:yyyy_MM}", 2, 0, "FssLog");
        }

        /// <summary>
        /// 用户索引
        /// </summary>
        public IndexSet<RunLogPO> RunLog { get; set; }
    }
}