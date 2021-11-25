using System.Collections.Generic;
using FS.Core;
using FS.Extends;
using FSS.Abstract.Entity;
using FSS.Abstract.Server.MetaInfo;
using FSS.Com.MetaInfoServer.RunLog.Dal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FSS.Com.MetaInfoServer.RunLog
{
    public class RunLogList : IRunLogList
    {

        static readonly bool UseEs;

        static RunLogList()
        {
            var configurationSection = FS.DI.IocManager.Instance.Resolve<IConfigurationRoot>().GetSection("ElasticSearch:0:Server").Value;
            UseEs = !string.IsNullOrWhiteSpace(configurationSection);
        }

        /// <summary>
        /// 获取日志
        /// </summary>
        public DataSplitList<RunLogVO> GetList(string jobName, LogLevel? logLevel, int pageSize, int pageIndex)
        {
            List<RunLogPO> lst;
            long           recordCount;

            if (UseEs)
            {
                lst = LogContext.Data.RunLog.Where(o => o.JobName == jobName && o.LogLevel == logLevel).Desc(o => o.CreateAt).ToList(pageSize, pageIndex, out recordCount);
            }
            else
            {
                var set = MetaInfoContext.Data.RunLog.Desc(o => o.CreateAt);
                if (!jobName.IsNullOrEmpty())
                {
                    set.Where(o => o.JobName == jobName);
                }
                
                if (logLevel.HasValue)
                {
                    set.Where(o => o.LogLevel == logLevel);
                }

                lst         = set.ToList(pageSize, pageIndex, out var recordCount2);
                recordCount = recordCount2;
            }

            return new DataSplitList<RunLogVO>(lst.Map<RunLogVO>(), recordCount.ConvertType(0));

        }
    }
}