using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace TFM.WebApi.Middlewares
{
    public class LoggerMiddleware : IMiddleware
    {
        //private readonly Models.Security.UserInformation userInfo = null;
        private readonly ILogger<LoggerMiddleware> _logger = null;

        public LoggerMiddleware(ILogger<LoggerMiddleware> logger)//Models.Security.UserInformation userInfo, )
        {
            //this.userInfo = userInfo;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            DateTime startOn = DateTime.Now;
            var sw = Stopwatch.StartNew();

            await next(context);

            sw.Stop();
            DateTime endOn = DateTime.Now;

            using (_logger.BeginScope(new KeyValuePair<string, object>[]
            {
                //new KeyValuePair<string, object>("UserID", userInfo.UserID),
                new KeyValuePair<string, object>("Started", startOn),
                new KeyValuePair<string, object>("Ended", endOn),
                new KeyValuePair<string, object>("TotalTimeMs", sw.ElapsedMilliseconds)
            }))
            {
                _logger.LogInformation("Logging request");
            }
        }
    }
}
