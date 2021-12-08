using System.Threading.Tasks;
using FS.DI;
using Microsoft.Extensions.Logging;

namespace FSS.Abstract.Server.MetaInfo
{
    public interface IRunLogAdd : ISingletonDependency
    {
        /// <summary>
        /// 添加日志记录
        /// </summary>
        Task AddAsync(int taskGroupId, LogLevel logLevel, string content);

    }
}