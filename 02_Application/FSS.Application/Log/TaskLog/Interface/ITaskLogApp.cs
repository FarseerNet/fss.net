using System.Threading.Tasks;
using FS.Core;
using FSS.Application.Log.TaskLog.Entity;
using FSS.Domain.Tasks.TaskGroup.Entity;
using Microsoft.Extensions.Logging;

namespace FSS.Application.Log.TaskLog.Interface
{
    public interface ITaskLogApp
    {
        /// <summary>
        /// 获取日志
        /// </summary>
        DataSplitList<TaskLogDTO> GetList(string jobName, LogLevel? logLevel, int pageSize, int pageIndex);
        /// <summary>
        /// 添加日志记录
        /// </summary>
        Task AddAsync(int taskGroupId, string jobName, string caption, LogLevel logLevel, string content);
        /// <summary>
        /// 添加日志记录
        /// </summary>
        Task AddAsync(TaskGroupDO taskGroupDO, LogLevel logLevel, string content);
    }
}