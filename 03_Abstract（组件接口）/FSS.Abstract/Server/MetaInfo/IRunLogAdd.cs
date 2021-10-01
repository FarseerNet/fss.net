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
        Task AddAsync(int taskGroupId, int taskId, LogLevel logLevel, string content);

        /// <summary>
        /// 添加日志记录
        /// </summary>
        Task AddAsync(TaskGroupVO groupInfo, int taskId, LogLevel logLevel, string content);
    }
}