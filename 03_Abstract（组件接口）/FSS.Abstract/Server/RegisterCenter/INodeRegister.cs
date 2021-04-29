using FS.DI;

namespace FSS.Abstract.Server.RegisterCenter
{
    public interface INodeRegister: ITransientDependency
    {
        /// <summary>
        /// 注册当前IP到服务列表中
        /// </summary>
        void Register();

        /// <summary>
        /// 获取当前节点IP
        /// </summary>
        string GetNodeIp();

        /// <summary>
        /// 检查服务节点是否在线
        /// </summary>
        bool IsNodeExists(string ip);
    }
}