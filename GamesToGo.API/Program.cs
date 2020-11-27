using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Linq;
using GamesToGo.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GamesToGo.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine(Environment.CurrentDirectory);
            Console.WriteLine(System.Reflection.Assembly.GetExecutingAssembly().Location);
            Environment.CurrentDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var host = CreateHostBuilder(args).Build();

            if (args.Any(a => a == "--database"))
            {
                using var scope = host.Services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<GamesToGoContext>();
                db.Database.Migrate();
                return;
            }

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>().UseUrls("http://*:5000");
                });
    }
}
