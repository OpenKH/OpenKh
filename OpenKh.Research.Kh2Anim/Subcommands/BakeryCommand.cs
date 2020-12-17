using McMaster.Extensions.CommandLineUtils;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using OpenKh.Research.Kh2Anim.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenKh.Research.Kh2Anim.Subcommands
{
    [HelpOption]
    [Command(Description = "Launch bakery web server (powered by ASP.NET Core) to bake motion as Matrix4x4")]
    public class BakeryCommand
    {
        [Option(CommandOptionType.MultipleValue)]
        public string[] Url { get; set; }

        protected int OnExecute(CommandLineApplication app)
        {
            var args = new string[0];
            CreateHostBuilder(args).Build().Run();

            return 0;
        }

        private IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(
                    webBuilder =>
                    {
                        if (Url != null && Url.Length != 0)
                        {
                            webBuilder.UseUrls(Url);
                        }
                        webBuilder.UseStartup<Startup>();
                    }
                );
    }
}
