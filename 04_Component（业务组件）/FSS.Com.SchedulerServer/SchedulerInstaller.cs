using System.Reflection;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using FS.DI;
using FSS.Abstract.Server.Scheduler;
using FSS.Com.SchedulerServer.Scheduler;

namespace FSS.Com.SchedulerServer
{
    public class SchedulerInstaller : IWindsorInstaller
    {
        /// <summary>
        ///     依赖获取接口
        /// </summary>
        private readonly IIocResolver _iocResolver;

        /// <summary>
        ///     构造函数
        /// </summary>
        public SchedulerInstaller(IIocResolver iocResolver)
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