using System.Threading.Tasks;
using FS.Core;
using FSS.Abstract.Entity.MetaInfo;
using FSS.Domain.Log.TaskLog.Entity;
using Microsoft.Extensions.Logging;

namespace FSS.Application.Log.Interface
{
    public interface ILogAddApp
    {
        /// <summary>
        /// 添加日志记录
        /// </summary>
        Task AddAsync(TaskGroupVO groupInfo, LogLevel logLevel, string content);
        /// <summary>
        /// 获取日志
        /// </summary>
        DataSplitList<RunLogDO> GetList(string jobName, LogLevel? logLevel, int pageSize, int pageIndex);
    }
}