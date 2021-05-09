using System;
using System.Collections.Generic;
using FS.DI;
using FS.ElasticSearch;
using FS.ElasticSearch.Map;
using FSS.Com.MetaInfoServer.RunLog.Dal;
using Microsoft.Extensions.Configuration;

namespace FSS.Com.MetaInfoServer
{
    /// <summary>
    /// ES日志上下文
    /// </summary>
    public class LogContext : EsContext<LogContext>
    {
        /// <summary>
        /// ES索引日期格式化
        /// </summary>
        private static readonly string _elasticIndexFormat;

        static LogContext()
        {
            _elasticIndexFormat = IocManager.Instance.Resolve<IConfigurationRoot>().GetSection("FSS:ElasticIndexFormat").Value;
            if (string.IsNullOrWhiteSpace(_elasticIndexFormat)) _elasticIndexFormat = "yyyy_MM";
        }

        public LogContext() : base("es")
        {
        }

        protected override void CreateModelInit(Dictionary<string, SetDataMap> map)
        {
            map["RunLog"].SetName($"FssLog_{DateTime.Now.ToString(_elasticIndexFormat)}", 2, 0, "FssLog");
        }

        /// <summary>
        /// 用户索引
        /// </summary>
        public IndexSet<RunLogPO> RunLog { get; set; }
    }
}