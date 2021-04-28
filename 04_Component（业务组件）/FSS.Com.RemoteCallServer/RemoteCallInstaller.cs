using System.Reflection;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using FS.DI;
using FSS.Abstract.Server.RemoteCall;
using FSS.Com.RemoteCallServer.RemoteCommand;

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
            container.Register(Component.For<IRemoteCommand, RegisterCommand>().Named("fss_server_Register").LifestyleTransient());
            container.Register(Component.For<IRemoteCommand, JobStatusCommand>().Named("fss_server_JobStatus").LifestyleTransient());
        }
    }
}