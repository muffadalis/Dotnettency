using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace MvcWeb
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var builder = new DotnettencyServiceProviderFactory<Tenant>();

            return Host.CreateDefaultBuilder(args)
                .UseServiceProviderFactory(builder)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
        }
    }
}
