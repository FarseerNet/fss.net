using FS.Cache.Redis;
using FS.Core;
using FS.Data;
using FS.ElasticSearch;
using FS.EventBus;
using FS.Job;
using FS.LinkTrack;
using FS.Mapper;
using FS.Modules;

namespace FSS.Infrastructure
{
    [DependsOn(typeof(MapperModule),
                  typeof(RedisModule),
                  typeof(DataModule),
                  typeof(ElasticSearchModule),
                  typeof(LinkTrackModule),
                  typeof(EventBusModule),
                  typeof(JobModule),
                  typeof(FarseerCoreModule))]
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