using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using TFM.Data.Models.Game;
using TFM.Services;
using TFM.Services.Mail;
using TFM.Services.Scraping;

namespace TFM.WebScraping
{
    class Program
    {
        public static async Task Main()
        {
            int exitCode = 0;
            ServiceProvider sp = null;
            
            try
            {
                IConfiguration configuration = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .Build();

                // create service collection
                var services = new ServiceCollection();
                ConfigureServices(services, configuration);

                // create service provider
                sp = services.BuildServiceProvider();

                // scrape metacritic
                var metacriticObjects = await sp.GetService<IScrapingService>().Scrape();

                // update db ranking
                var mails = await sp.GetService<IGamesService>().UpsertRanking(metacriticObjects.Select(mo => new Game(mo.Result)));

                // new added games notifications
                foreach (var mail in mails)
                {
                    await sp.GetService<IMailService>().SendMail(mail);
                }
            }
            catch (Exception ex)
            {
                exitCode = -1;

                using (var msg = new MailMessage
                {
                    Subject = "Something went wrong!",
                    Body = ex.ToString()
                })
                {
                    await sp.GetService<IMailService>().SendMail(msg);
                }

                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine(ex.ToString());
            }

            Environment.Exit(exitCode);
        }

        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {            
            // DB
            services.AddDbContext<Data.DB.TFMContext>(
                (sp, options) => options.UseSqlServer(sp.GetRequiredService<Data.Models.Configuration.AppSettings>().TFMConnectionString,
                sqlServerOptionsAction: sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 10,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
                })
            );

            // Configs
            services.AddSingleton(configuration.Get<Data.Models.Configuration.AppSettings>());

            // Services
            services.AddSingleton<IScrapingService, ScrapingService>();
            services.AddSingleton<IGamesService, GamesService>();
            services.AddSingleton<IMailService, MailService>();

            // HttpClient
            services.AddHttpClient<IScrapingService, ScrapingService>(
                (sp, c) =>
                {
                    c.DefaultRequestHeaders.Add("x-rapidapi-host", sp.GetRequiredService<Data.Models.Configuration.AppSettings>().MetacriticApi.Host);
                    c.DefaultRequestHeaders.Add("x-rapidapi-key", sp.GetRequiredService<Data.Models.Configuration.AppSettings>().MetacriticApi.Key);
                });
        }
    }
}
