using System.Threading.Tasks;
using FS.DI;
using Microsoft.Extensions.Logging;

namespace FSS.Domain.Log.TaskLog.Interface
{
    public interface ITaskLogService: ISingletonDependency
    {
        /// <summary>
        /// 添加日志记录
        /// </summary>
        Task AddAsync(int taskGroupId, string jobName, string caption, LogLevel logLevel, string content);
    }
}