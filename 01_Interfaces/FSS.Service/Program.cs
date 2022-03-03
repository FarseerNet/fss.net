using FS;
using FS.Job;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace FSS.Service;

[Fss]
public class Program
{
    public static void Main(string[] args)
    {
        FarseerApplication.Run<Startup>(appName: "FSS.Service").Initialize();
        CreateHostBuilder(args: args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args: args).UseWindsorContainerServiceProvider().ConfigureWebHostDefaults(configure: webBuilder =>
        {
            webBuilder.UseKestrel().UseStartup<Startup>();
        });
    }
}