using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TFM.Data.Models.Configuration;
using TFM.Services.JobServices.Ping;
using TFM.Services.JobServices.Ranking;
using TFM.Services.Mail;
using TFM.Services.Ranking;
using TFM.Services.Scraping;

namespace TFM.WebApi
{
    public class Startup
    {
        private readonly string MyAllowAllOrigins = "_myAllowAllOrigins";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Register CORS
            services.AddCors(o => o.AddPolicy(MyAllowAllOrigins, builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            }));

            // Register Controllers
            services.AddControllers();

            // Register DB
            services.AddDbContext<Data.DB.TFMContext>((sp, options) => options.UseSqlServer(sp.GetRequiredService<AppSettings>().TFMConnectionString,
                sqlServerOptionsAction: sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 10,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
                })
            );

            // Register Configs
            services.AddSingleton(Configuration.Get<AppSettings>());

            // Register HttpClient
            services.AddHttpClient();

            // Register Services
            services.AddTransient<IRankingService, RankingService>();
            services.AddTransient<IMailService, MailService>();
            services.AddTransient<IScrapingService, ScrapingService>();

            // Register Hosted Services
            services.AddHostedService<PingJobService>();
            services.AddHostedService<LoadRankingJobService>();

            // Register Swagger
            services.AddSwaggerGen(options =>
            {
                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors(MyAllowAllOrigins);

            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Games API");
                c.RoutePrefix = string.Empty;
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
