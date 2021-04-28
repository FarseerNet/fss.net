using System.Threading.Tasks;
using FS.DI;
using FSS.Abstract.Entity.MetaInfo;
using FSS.Abstract.Entity.RegisterCenter;

namespace FSS.Abstract.Server.RemoteCall
{
    public interface IClientResponse: ITransientDependency
    {
        /// <summary>
        /// 打印文本
        /// </summary>
        /// <param name="responseStream">响应流</param>
        /// <param name="message">打印内容</param>
        Task PrintAsync(object responseStream, string message);

        /// <summary>
        /// 通知客户端开始任务调度
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="taskGroup">任务组</param>
        /// <param name="task">任务</param>
        Task JobSchedulerAsync(ClientConnectVO client, TaskGroupVO taskGroup, TaskVO task);

        /// <summary>
        /// 打印文本
        /// </summary>
        /// <param name="message">打印内容</param>
        object Print(string message);

        /// <summary>
        /// 忽略操作
        /// </summary>
        object Ignore(string message);
    }
}