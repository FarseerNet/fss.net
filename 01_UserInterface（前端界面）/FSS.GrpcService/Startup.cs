using System;
using System.Threading;
using FS.Cache.Redis;
using FS.Core;
using FS.Data;
using FS.DI;
using FS.Mapper;
using FS.Modules;
using FSS.Abstract.Server.MetaInfo;
using FSS.Abstract.Server.Scheduler;
using FSS.Com.MetaInfoServer;
using FSS.Com.RegisterCenterServer;
using FSS.Com.RemoteCallServer;
using FSS.Com.SchedulerServer;
using FSS.GrpcService.Background;
using FSS.GrpcService.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FSS.GrpcService
{
    [DependsOn(
        typeof(FarseerCoreModule),
        typeof(MapperModule),
        typeof(RedisModule),
        typeof(DataModule),
        typeof(MetaInfoModule),
        typeof(SchedulerModule),
        typeof(RegisterCenterModule),
        typeof(RemoteCallModule))]
    public class Startup : FarseerModule
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGrpc(options => { options.Interceptors.Add<AuthInterceptor>(); });
            services.AddSingleton<IIocManager>(FS.DI.IocManager.Instance.Resolve<IIocManager>());
            
            // 开启任务组调度
            services.AddHostedService<PrintEndPortService>(); 
            services.AddHostedService<SyncTaskGroupAvgSpeedService>(); 
            services.AddHostedService<SyncServiceInfoService>();
            services.AddHostedService<PrintThreadCountService>();
            //services.AddHostedService<RunThreadSchedulerService>(); 
            services.AddHostedService<RunTaskSchedulerService>(); 
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                //app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<FssService>();
                endpoints.MapGet("/", async context => { await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909"); });
            });
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