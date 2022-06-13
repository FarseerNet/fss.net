using System.Threading;
using FS;
using FS.Modules;
using FSS.Application;
using FSS.Application.Job;
using FSS.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace FSS.Service;

[DependsOn(
              typeof(ApplicationModule),
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
        services.AddSwaggerGen(setupAction: c => { c.SwaggerDoc(name: "v1", info: new OpenApiInfo { Title = "WebApplication", Version = "v1" }); });
        services.AddFarseerControllers();

        // 开启任务组调度
        services.AddHostedService<PrintSysInfoService>();
        services.AddHostedService<InitSysTaskService>();
        services.AddHostedService<CheckFinishStatusService>();
        services.AddHostedService<CheckWorkStatusService>();

    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(setupAction: c => c.SwaggerEndpoint(url: "/swagger/v1/swagger.json", name: "WebApplication v1"));
        }

        app.UseMiddleware<LinkTrackMiddleware>();
        app.UseMiddleware<CorsMiddleware>();
        app.UseMiddleware<ExceptionMiddleware>();

        app.UseRouting();
        app.UseEndpoints(configure: endpoints => { endpoints.MapControllers(); });
    }

    public override void PreInitialize()
    {
        ThreadPool.SetMinThreads(workerThreads: 200, completionPortThreads: 200);
    }

    public override void PostInitialize()
    {
        IocManager.RegisterAssemblyByConvention(type: GetType());
    }
}