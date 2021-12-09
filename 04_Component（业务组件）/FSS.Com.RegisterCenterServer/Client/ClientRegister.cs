using System.Threading.Tasks;
using FSS.Abstract.Server.MetaInfo;
using FSS.Abstract.Server.RegisterCenter;

namespace FSS.Com.RegisterCenterServer.Client
{
    /// <summary>
    /// 客户端注册
    /// </summary>
    // ReSharper disable once UnusedType.Global
    public class ClientRegister : IClientRegister
    {
        public ITaskList      TaskList      { get; set; }
        public ITaskUpdate    TaskUpdate    { get; set; }
        public ITaskGroupInfo TaskGroupInfo { get; set; }

        /// <summary>
        /// 移除客户端
        /// </summary>
        public async Task RemoveAsync(long clientId)
        {
        }
    }
}