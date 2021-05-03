using System.Threading.Tasks;
using FSS.Com.MetaInfoServer.Abstract;

namespace FSS.Com.MetaInfoServer.RunLog.Dal
{
    /// <summary>
    /// 日志记录数据库层
    /// </summary>
    public class RunLogAgent : IRunLogAgent
    {
        /// <summary>
        /// 添加日志记录
        /// </summary>
        public Task AddAsync(RunLogPO po) => MetaInfoContext.Data.RunLog.InsertAsync(po);
    }
}