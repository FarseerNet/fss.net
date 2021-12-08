using System.Collections.Generic;
using System.Threading.Tasks;
using FS.Core;
using FS.Extends;
using FSS.Infrastructure.Repository.Log.Entity;
using FSS.Infrastructure.Repository.Log.Interface;
using Microsoft.Extensions.Logging;

namespace FSS.Infrastructure.Repository.Log
{
    public class LogAgent : ILogAgent
    {
        /// <summary>
        /// 将日志写入ES或数据库
        /// </summary>
        public async Task<int> AddAsync(List<RunLogPO> lstLog)
        {
            if (EsContext.UseEs)
            {
                await EsContext.Data.RunLog.InsertAsync(lstLog);
            }
            else
            {
                await MysqlContext.Data.RunLog.InsertAsync(lstLog);
            }

            return lstLog.Count;
        }

        /// <summary>
        /// 获取日志
        /// </summary>
        public DataSplitList<RunLogPO> GetList(string jobName, LogLevel? logLevel, int pageSize, int pageIndex)
        {
            List<RunLogPO> lst;
            long           recordCount;

            if (EsContext.UseEs)
            {
                lst = EsContext.Data.RunLog.Where(o => o.JobName == jobName && o.LogLevel == logLevel).Desc(o => o.CreateAt).ToList(pageSize, pageIndex, out recordCount);
            }
            else
            {
                var set = MysqlContext.Data.RunLog.Desc(o => o.CreateAt);
                if (!string.IsNullOrWhiteSpace(jobName))
                {
                    set.Where(o => o.JobName == jobName);
                }

                if (logLevel.HasValue)
                {
                    set.Where(o => o.LogLevel == logLevel);
                }

                lst = set.ToList(pageSize, pageIndex, out recordCount);
            }

            return new DataSplitList<RunLogPO>(lst, recordCount.ConvertType(0));
        }
    }
}