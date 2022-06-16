using FS;
using FS.Fss;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace FSS.Service;

[Fss]
public class Program
{
    public static void Main(string[] args)
    {
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