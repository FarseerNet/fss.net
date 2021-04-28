using FS.DI;

namespace FSS.Abstract.Server.RegisterCenter
{
    public interface INodeRegister: ITransientDependency
    {
        /// <summary>
        /// 注册当前IP到服务列表中
        /// </summary>
        void Register();
    }
}