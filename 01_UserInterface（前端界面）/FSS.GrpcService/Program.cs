using System.Net;
using FS;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Hosting;

namespace FSS.GrpcService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            FarseerApplication.Run<Startup>("FSS.GrpcService").Initialize();
            CreateHostBuilder(args).Build().Run();
        }

        // Additional configuration is required to successfully run gRPC on macOS.
        // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682


        // Additional configuration is required to successfully run gRPC on macOS.
        // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(webBuilder =>
            {
                //Setup a HTTP/2 endpoint without TLS.
                webBuilder.ConfigureKestrel(options => options.Listen(IPAddress.Any, 80)).UseStartup<Startup>();
            });
        }
    }
}