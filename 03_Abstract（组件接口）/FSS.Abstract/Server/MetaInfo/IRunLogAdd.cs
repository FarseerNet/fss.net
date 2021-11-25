using System.Threading.Tasks;
using FS.DI;
using FSS.Abstract.Entity.MetaInfo;
using Microsoft.Extensions.Logging;

namespace FSS.Abstract.Server.MetaInfo
{
    public interface IRunLogAdd : ISingletonDependency
    {
        /// <summary>
        /// 添加日志记录
        /// </summary>
        Task AddAsync(int taskGroupId, LogLevel logLevel, string content);

        /// <summary>
        /// 添加日志记录
        /// </summary>
        Task AddAsync(TaskGroupVO groupInfo, LogLevel logLevel, string content);
        /// <summary>
        /// 将日志写入ES或数据库
        /// </summary>
        Task<int> AddToDbAsync(int top);
    }
}