using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace TFM.WebApi.Middlewares
{
    public class ExceptionLoggerFilter : ExceptionFilterAttribute
    {
        //private readonly Models.Security.UserInformation userInfo = null;
        private readonly ILogger<ExceptionLoggerFilter> _logger = null;

        public ExceptionLoggerFilter(ILogger<ExceptionLoggerFilter> logger)//, Models.Security.UserInformation userInfo)
        {
            //this.userInfo = userInfo;
            _logger = logger;
        }

        public override void OnException(ExceptionContext context)
        {
            using (_logger.BeginScope(new KeyValuePair<string, object>[]
            {
                //new KeyValuePair<string, object>("UserID", userInfo.UserID)
            }))
            {
                _logger.LogError(context.Exception, "Logging exception");

                if (context.Exception.InnerException != null)
                    _logger.LogError(context.Exception.InnerException, "Logging inner exception");
            }
        }
    }
}
