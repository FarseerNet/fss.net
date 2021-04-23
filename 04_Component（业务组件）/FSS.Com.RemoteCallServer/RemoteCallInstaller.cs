using System.Reflection;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using FS.DI;

namespace FSS.Com.RemoteCallServer
{
    public class RemoteCallInstaller : IWindsorInstaller
    {
        /// <summary>
        ///     依赖获取接口
        /// </summary>
        private readonly IIocResolver _iocResolver;

        /// <summary>
        ///     构造函数
        /// </summary>
        public RemoteCallInstaller(IIocResolver iocResolver)
        {
            _iocResolver = iocResolver;
        }

        /// <summary>
        ///     注册依赖
        /// </summary>
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            IocManager.Instance.RegisterAssemblyByConvention(Assembly.GetExecutingAssembly());
        }
    }
}