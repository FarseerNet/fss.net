namespace FSS.Abstract.Server.RegisterCenter
{
    public interface IClientRegister
    {
        /// <summary>
        /// 注册
        /// </summary>
        bool Register(string clientId, string endpoint);
    }
}