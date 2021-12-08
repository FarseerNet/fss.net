using System.Collections.Generic;
using FS.Data;
using FS.Data.Map;
using FSS.Infrastructure.Repository.Log.Entity;

namespace FSS.Infrastructure.Repository
{
    /// <summary>
    /// 元信息上下文
    /// </summary>
    public class MysqlContext : DbContext<MysqlContext>
    {
        public MysqlContext() : base("default")
        {
        }

        public TableSet<RunLogPO> RunLog { get; set; }

        protected override void CreateModelInit(Dictionary<string, SetDataMap> map)
        {
            //map["RunLog"].SetName("run_log");
            RunLog.SetName("run_log");
        }
    }
}