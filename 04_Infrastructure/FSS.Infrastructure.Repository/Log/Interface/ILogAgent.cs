using System.Collections.Generic;
using System.Threading.Tasks;
using FS.Core;
using FS.DI;
using FSS.Infrastructure.Repository.Log.Model;
using Microsoft.Extensions.Logging;

namespace FSS.Infrastructure.Repository.Log.Interface
{
    public interface ILogAgent: ISingletonDependency
    {
        /// <summary>
        /// 将日志写入ES或数据库
        /// </summary>
        Task<int> AddAsync(List<RunLogPO> lstLog);
        
        /// <summary>
        /// 获取日志
        /// </summary>
        DataSplitList<RunLogPO> GetList(string jobName, LogLevel? logLevel, int pageSize, int pageIndex);
    }
}