using System.Reflection;
using FS.Cache;
using FS.DI;
using FS.Modules;
using FSS.Application;
using FSS.Domain.Client;
using FSS.Domain.Log;
using FSS.Domain.Tasks;

namespace FSS.Infrastructure
{
    [DependsOn(typeof(ClientModule),typeof(LogModule),typeof(TasksModule),typeof(ApplicationModule))]
    public class InfrastructureModule : FarseerModule
    {
        /// <summary>
        ///     初始化之前
        /// </summary>
        public override void PreInitialize()
        {
        }

        /// <summary>
        ///     初始化
        /// </summary>
        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(GetType());
        }
    }
}