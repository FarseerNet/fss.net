using FS.Cache;
using FS.Cache.Redis;
using FS.Core;
using FS.Data;
using FS.ElasticSearch;
using FS.EventBus;
using FS.Fss;
using FS.LinkTrack;
using FS.Mapper;
using FS.Modules;
using FS.MQ.Queue;
using FSS.Application.Tasks.TaskGroup.Entity;
using FSS.Domain.Client.Clients;
using FSS.Domain.Tasks.TaskGroup;
using FSS.Domain.Tasks.TaskGroup.Entity;
using FSS.Infrastructure.Repository.Tasks.Model;

namespace FSS.Infrastructure;

[DependsOn(typeof(MapperModule),
              typeof(QueueModule),
              typeof(RedisModule),
              typeof(DataModule),
              typeof(ElasticSearchModule),
              typeof(LinkTrackModule),
              typeof(EventBusModule),
              typeof(FssModule),
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
        IocManager.RegisterAssemblyByConvention(type: GetType());
        IocManager.Resolve<ICacheServices>().SetProfilesInRedis<TaskGroupDO>("FSS_TaskGroup","default");
        IocManager.Resolve<ICacheServices>().SetProfilesInRedis<ClientDO>("FSS_ClientList","default");
        
        MapConfig<TaskPO, TaskDO>.Set();
        MapConfig<TaskDO, TaskDTO>.Set();
    }
}