using System.Collections.Generic;
using System.Threading.Tasks;
using FS.Core;
using FS.DI;
using FS.Extends;
using FSS.Infrastructure.Repository.Log.Model;
using Microsoft.Extensions.Logging;

namespace FSS.Infrastructure.Repository.Log
{
    public class LogAgent : ISingletonDependency
    {
        /// <summary>
        /// 将日志写入ES或数据库
        /// </summary>
        public async Task<int> AddAsync(List<TaskLogPO> lstLog)
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
        public List<TaskLogPO> GetList(string jobName, LogLevel? logLevel, int pageSize, int pageIndex, out long recordCount)
        {
            List<TaskLogPO> lst;
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
            return lst;
        }
    }
}