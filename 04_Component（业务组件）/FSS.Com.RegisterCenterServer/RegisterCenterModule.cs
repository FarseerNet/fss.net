using FS.Modules;

namespace FSS.Com.RegisterCenterServer
{
    /// <summary>
    ///     注册中心模块
    /// </summary>
    [DependsOn]
    public class RegisterCenterModule : FarseerModule
    {
        public override void PreInitialize()
        {
        }

        public override void PostInitialize()
        {
            IocManager.Container.Install(new RegisterCenterInstaller(IocManager));
        }
    }
}