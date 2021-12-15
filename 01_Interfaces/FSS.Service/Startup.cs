using System.Threading;
using FS;
using FS.Cache.Redis;
using FS.Core;
using FS.Data;
using FS.ElasticSearch;
using FS.EventBus;
using FS.Job;
using FS.LinkTrack;
using FS.Mapper;
using FS.Modules;
using FSS.Application.Tasks.TaskGroup.Job;
using FSS.Infrastructure;
using FSS.Service.Background;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace FSS.Service
{
    [DependsOn(
                  typeof(FarseerCoreModule),
                  typeof(MapperModule),
                  typeof(RedisModule),
                  typeof(DataModule),
                  typeof(ElasticSearchModule),
                  typeof(LinkTrackModule),
                  typeof(EventBusModule),
                  typeof(JobModule),
                  typeof(InfrastructureModule)
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
            services.AddHostedService<InitSysTaskService>();
            services.AddHostedService<CheckFinishStatusService>();
            services.AddHostedService<CheckWorkStatusService>();
            services.AddLogging(o =>
                                o.AddConsole());

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