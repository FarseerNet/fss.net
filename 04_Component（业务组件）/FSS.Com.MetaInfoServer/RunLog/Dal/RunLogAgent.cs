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
        public void Add(RunLogPO po) => MetaInfoContext.Data.RunLog.Insert(po);
    }
}