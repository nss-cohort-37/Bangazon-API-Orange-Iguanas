using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BangazonAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}

//____ _
//  / __ \                             (_)                                  
// | |  | |_ __ __ _ _ __   __ _  ___ _  __ _ _ _  __ _ _ __   __ _ ___ 
// | |  | | '__/ _` | '_ \ / _` |/ _ \ | |/ _` | | | |/ _` | '_ \ / _` / __|
// | |__| | | | (_| | | | | (_| |  __/ | | (_| | |_| | (_| | | | | (_| \__ \
//  \____/|_|  \__,_|_| |_|\__, |\___| |_|\__, |\__,_|\__,_|_| |_|\__,_|___/
//                          __/ |          __/ |                            
//                         |___/          |___/                            
