using FS.Modules;

namespace FSS.Com.MetaInfoServer
{
    /// <summary>
    ///     元信息模块
    /// </summary>
    [DependsOn]
    public class MetaInfoModule : FarseerModule
    {
        public override void PreInitialize()
        {
        }

        public override void PostInitialize()
        {
            IocManager.Container.Install(new MetaInfoInstaller(IocManager));
        }
    }
}