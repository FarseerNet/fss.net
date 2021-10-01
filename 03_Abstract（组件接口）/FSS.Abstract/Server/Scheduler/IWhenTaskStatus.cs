using System.Threading.Tasks;
using FS.DI;

namespace FSS.Abstract.Server.Scheduler
{
    public interface IWhenTaskStatus: ISingletonDependency
    {
        /// <summary>
        /// 运行检查指定状态的任务
        /// </summary>
        Task Run();
    }
}