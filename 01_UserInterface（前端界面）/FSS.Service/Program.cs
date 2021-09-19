using System.Net;
using FS;
using FS.DI;
using FS.Extends;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FSS.Service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            FarseerApplication.Run<Startup>("FSS.Service").Initialize(Env.IsPro ? null : _ => _.AddConsole());
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args).UseWindsorContainerServiceProvider().ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseKestrel().UseStartup<Startup>();
            });
        }
    }
}