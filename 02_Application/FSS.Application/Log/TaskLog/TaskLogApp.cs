using System.Threading.Tasks;
using FS.Core;
using FS.DI;
using FS.Extends;
using FSS.Application.Log.TaskLog.Entity;
using FSS.Domain.Log.TaskLog.Interface;
using FSS.Domain.Log.TaskLog.Repository;
using FSS.Domain.Tasks.TaskGroup.Entity;
using Microsoft.Extensions.Logging;

namespace FSS.Application.Log.TaskLog
{
    public class TaskLogApp : ISingletonDependency
    {
        public ITaskLogService    TaskLogService    { get; set; }
        public ITaskLogRepository TaskLogRepository { get; set; }

        /// <summary>
        /// 添加日志记录
        /// </summary>
        public Task AddAsync(int taskGroupId, string jobName, string caption, LogLevel logLevel, string content)
        {
            return TaskLogService.AddAsync(taskGroupId, jobName, caption, logLevel, content);
        }
        
        /// <summary>
        /// 添加日志记录
        /// </summary>
        public Task AddAsync(TaskGroupDO taskGroupDO, LogLevel logLevel, string content)
        {
            return TaskLogService.AddAsync(taskGroupDO.Id, taskGroupDO.JobName, taskGroupDO.Caption, logLevel, content);
        }

        /// <summary>
        /// 获取日志
        /// </summary>
        public DataSplitList<TaskLogDTO> GetList(string jobName, LogLevel? logLevel, int pageSize, int pageIndex)
        {
            var lst = TaskLogRepository.GetList(jobName, logLevel, pageSize, pageIndex, out var totalCount);
            return new DataSplitList<TaskLogDTO>(lst.Map<TaskLogDTO>(), totalCount);
        }
    }
}