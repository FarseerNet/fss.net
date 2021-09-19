using System.Threading;
using FS;
using FS.Cache.Redis;
using FS.Core;
using FS.Data;
using FS.ElasticSearch;
using FS.LinkTrack;
using FS.Mapper;
using FS.Modules;
using FS.MQ.RedisStream;
using FSS.Com.MetaInfoServer;
using FSS.Com.RegisterCenterServer;
using FSS.Com.SchedulerServer;
using FSS.Service.Background;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace FSS.Service
{
    [DependsOn(
        typeof(FarseerCoreModule),
        typeof(MapperModule),
        typeof(RedisModule),
        typeof(RedisStreamModule),
        typeof(DataModule),
        typeof(ElasticSearchModule),
        typeof(MetaInfoModule),
        typeof(SchedulerModule),
        typeof(RegisterCenterModule),
        typeof(LinkTrackModule)
    )]
    public class Startup : FarseerModule
    {
        public Startup()
        {
        }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebApplication", Version = "v1" }); });
            services.AddFarseerControllers();
            
            // 开启任务组调度
            services.AddHostedService<PrintSysInfoService>();
            services.AddHostedService<SyncTaskGroupAvgSpeedService>();
            services.AddHostedService<SyncTaskGroupService>();
            services.AddHostedService<AutoClearHisTaskRecordService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebApplication v1"));
            }
            
            app.UseMiddleware<LinkTrackMiddleware>();
            app.UseMiddleware<CorsMiddleware>();
            app.UseMiddleware<ExceptionMiddleware>();

            app.UseRouting();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }

        public override void PreInitialize()
        {
            ThreadPool.SetMinThreads(200, 200);
        }

        public override void PostInitialize()
        {
            IocManager.RegisterAssemblyByConvention(GetType());
        }
    }
}