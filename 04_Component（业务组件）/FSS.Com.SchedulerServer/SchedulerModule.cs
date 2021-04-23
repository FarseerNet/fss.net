using FS.Modules;

namespace FSS.Com.SchedulerServer
{
    /// <summary>
    ///     元信息模块
    /// </summary>
    [DependsOn]
    public class SchedulerModule : FarseerModule
    {
        public override void PreInitialize()
        {
        }

        public override void PostInitialize()
        {
            IocManager.Container.Install(new SchedulerInstaller(IocManager));
        }
    }
}