using System.Collections.Generic;
using System.Threading.Tasks;
using FS.Core;
using FS.Core.Abstract.Data;
using FS.DI;
using FSS.Infrastructure.Repository.Context;
using FSS.Infrastructure.Repository.Log.Model;
using Microsoft.Extensions.Logging;

namespace FSS.Infrastructure.Repository.Log;

public class LogAgent : ISingletonDependency
{
    /// <summary>
    ///     将日志写入ES或数据库
    /// </summary>
    public async Task AddAsync(IEnumerable<TaskLogPO> lstLog)
    {
        if (EsContext.UseEs)
            await EsContext.Data.RunLog.InsertAsync(lst: lstLog);
        else
            await MysqlContext.Data.RunLog.InsertAsync(lst: lstLog);
    }

    /// <summary>
    ///     获取日志
    /// </summary>
    public PageList<TaskLogPO> GetList(string jobName, LogLevel? logLevel, int pageSize, int pageIndex)
    {
        PageList<TaskLogPO> lst;
        if (EsContext.UseEs)
            lst = EsContext.Data.RunLog.Where(query: o => o.JobName == jobName && o.LogLevel == logLevel).Desc(desc: o => o.CreateAt).ToPageList(pageSize: pageSize, pageIndex: pageIndex);
        else
        {
            var set = MysqlContext.Data.RunLog.Desc(desc: o => o.CreateAt);
            if (!string.IsNullOrWhiteSpace(value: jobName)) set.Where(where: o => o.JobName == jobName);

            if (logLevel.HasValue) set.Where(where: o => o.LogLevel == logLevel);

            lst = set.ToPageList(pageSize: pageSize, pageIndex: pageIndex);
        }
        return lst;
    }
}