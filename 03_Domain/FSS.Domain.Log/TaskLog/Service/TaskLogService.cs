using System;
using System.Threading.Tasks;
using FS.Core;
using FS.Extends;
using FSS.Abstract.Entity.MetaInfo;
using FSS.Domain.Log.TaskLog.Entity;
using FSS.Domain.Log.TaskLog.Interface;
using FSS.Infrastructure.Repository.Log.Interface;
using Microsoft.Extensions.Logging;

namespace FSS.Domain.Log.TaskLog.Service
{
    public class TaskLogService : ITaskLogService
    {
        public ILogQueue LogQueue { get; set; }
        public ILogAgent LogAgent { get; set; }

        /// <summary>
        /// 添加日志记录
        /// </summary>
        public Task AddAsync(TaskGroupVO groupInfo, LogLevel logLevel, string content)
        {
            return new RunLogDO
            {
                TaskGroupId = groupInfo.Id,
                Caption     = groupInfo.Caption ?? "",
                JobName     = groupInfo.JobName ?? "",
                LogLevel    = logLevel,
                Content     = content,
                CreateAt    = DateTime.Now
            }.AddAsync();
        }

        /// <summary>
        /// 将日志从队列中保存到ES数据库
        /// </summary>
        public async Task<int> SaveAsync(int saveCount)
        {
            var lstLog = await LogQueue.GetQueueAsync(saveCount);
            if (lstLog.Count == 0) return 0;

            return await LogAgent.AddAsync(lstLog);
        }

        /// <summary>
        /// 获取日志
        /// </summary>
        public DataSplitList<RunLogDO> GetList(string jobName, LogLevel? logLevel, int pageSize, int pageIndex)
        {
            var lst = LogAgent.GetList(jobName, logLevel, pageSize, pageIndex);
            return new DataSplitList<RunLogDO>(lst.List.Map<RunLogDO>(), lst.TotalCount);
        }
    }
}