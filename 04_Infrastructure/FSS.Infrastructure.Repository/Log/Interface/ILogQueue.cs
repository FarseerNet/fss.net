using System.Collections.Generic;
using System.Threading.Tasks;
using FS.DI;
using FSS.Infrastructure.Repository.Log.Entity;

namespace FSS.Infrastructure.Repository.Log.Interface
{
    public interface ILogQueue: ISingletonDependency
    {
        /// <summary>
        /// 将日志写入队列中
        /// </summary>
        Task AddQueueAsync(RunLogPO log);
        /// <summary>
        /// 队列中取出已完成的任务
        /// </summary>
        Task<List<RunLogPO>> GetQueueAsync(int top);
    }
}