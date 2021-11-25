using FS.Core;
using FS.DI;
using FSS.Abstract.Entity;
using Microsoft.Extensions.Logging;

namespace FSS.Abstract.Server.MetaInfo
{
    public interface IRunLogList: ISingletonDependency
    {
        /// <summary>
        /// 获取日志
        /// </summary>
        DataSplitList<RunLogVO> GetList(string jobName, LogLevel? requestLogLevel, int pageSize, int pageIndex);
    }
}