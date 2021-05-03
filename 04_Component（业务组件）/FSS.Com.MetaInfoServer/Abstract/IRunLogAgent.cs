using System.Threading.Tasks;
using FS.DI;
using FSS.Com.MetaInfoServer.RunLog.Dal;

namespace FSS.Com.MetaInfoServer.Abstract
{
    public interface IRunLogAgent : ITransientDependency
    {
        /// <summary>
        /// 添加日志记录
        /// </summary>
        Task AddAsync(RunLogPO po);
    }
}