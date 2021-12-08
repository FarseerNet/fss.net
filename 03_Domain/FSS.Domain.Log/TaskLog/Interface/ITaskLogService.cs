using System.Threading.Tasks;
using FS.Core;
using FS.DI;
using FSS.Abstract.Entity.MetaInfo;
using FSS.Domain.Log.TaskLog.Entity;
using Microsoft.Extensions.Logging;

namespace FSS.Domain.Log.TaskLog.Interface
{
    public interface ITaskLogService: ISingletonDependency
    {
        /// <summary>
        /// 添加日志记录
        /// </summary>
        Task AddAsync(TaskGroupVO groupInfo, LogLevel logLevel, string content);
        /// <summary>
        /// 将日志从队列中保存到ES数据库
        /// </summary>
        Task<int> SaveAsync(int saveCount);
        /// <summary>
        /// 获取日志
        /// </summary>
        DataSplitList<RunLogDO> GetList(string jobName, LogLevel? logLevel, int pageSize, int pageIndex);
    }
}