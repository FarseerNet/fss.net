using FS.DI;
using Microsoft.Extensions.Logging;

namespace FSS.Abstract.Server.MetaInfo
{
    public interface IRunLogAdd : ITransientDependency
    {
        /// <summary>
        /// 添加日志记录
        /// </summary>
        void Add(int taskGroupId, int taskId, LogLevel logLevel, string content);
    }
}