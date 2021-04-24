using FS.DI;

namespace FSS.Abstract.Server.RegisterCenter
{
    public interface IClientRegister: ITransientDependency
    {
        /// <summary>
        /// 注册
        /// </summary>
        bool Register(string clientId, string endpoint);
    }
}