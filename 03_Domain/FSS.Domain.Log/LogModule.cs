using System.Reflection;
using FS.DI;
using FS.Modules;

namespace FSS.Domain.Log
{
    public class LogModule : FarseerModule
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