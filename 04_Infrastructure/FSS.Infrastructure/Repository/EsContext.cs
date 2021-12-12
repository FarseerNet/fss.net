using System;
using System.Collections.Generic;
using System.Linq;
using FS.DI;
using FS.ElasticSearch;
using FS.ElasticSearch.Map;
using FSS.Infrastructure.Repository.Log.Model;
using Microsoft.Extensions.Configuration;

namespace FSS.Infrastructure.Repository
{
    /// <summary>
    /// ES日志上下文
    /// </summary>
    public class EsContext : EsContext<EsContext>
    {
        /// <summary>
        /// ES索引日期格式化
        /// </summary>
        private static readonly string ElasticIndexFormat;
        public static readonly bool UseEs;
        
        static EsContext()
        {
            UseEs              = FS.DI.IocManager.Instance.Resolve<IConfigurationRoot>().GetSection("ElasticSearch").GetChildren().Any(o => o.Key == "es");
            ElasticIndexFormat = IocManager.Instance.Resolve<IConfigurationRoot>().GetSection("FSS:ElasticIndexFormat").Value;
            if (string.IsNullOrWhiteSpace(ElasticIndexFormat)) ElasticIndexFormat = "yyyy_MM";
        }

        public EsContext() : base("es")
        {
        }

        protected override void CreateModelInit(Dictionary<string, SetDataMap> map)
        {
            //map["RunLog"].SetName($"FssLog_{DateTime.Now.ToString(ElasticIndexFormat)}", 2, 0, 1, "FssLog");
            RunLog.SetName($"FssLog_{DateTime.Now.ToString(ElasticIndexFormat)}", 2, 0, 1, "FssLog");
        }

        /// <summary>
        /// 用户索引
        /// </summary>
        public IndexSet<TaskLogPO> RunLog { get; set; }
    }
}