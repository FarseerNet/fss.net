using FS.Modules;
using FSS.Domain.Client;
using FSS.Domain.Log;
using FSS.Domain.Tasks;

namespace FSS.Application
{
    [DependsOn(typeof(ClientModule), typeof(LogModule), typeof(TasksModule))]
    public class ApplicationModule : FarseerModule
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