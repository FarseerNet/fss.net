using FS.DI;

namespace FSS.Com.MetaInfoServer.RunLog.Dal
{
    public interface IRunLogAgent : ITransientDependency
    {
        /// <summary>
        /// 添加日志记录
        /// </summary>
        void Add(RunLogPO po);
    }
}