using FS.Modules;

namespace FSS.Com.RemoteCallServer
{
    /// <summary>
    ///     远程调用模块
    /// </summary>
    [DependsOn]
    public class RemoteCallModule : FarseerModule
    {
        public override void PreInitialize()
        {
        }

        public override void PostInitialize()
        {
            IocManager.Container.Install(new RemoteCallInstaller(IocManager));
        }
    }
}