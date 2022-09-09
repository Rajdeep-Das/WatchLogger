using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WatchLogger.Models;

namespace WatchLogger
{
    internal class WatchLoggerExceptionLogger
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;

        public WatchLoggerExceptionLogger(RequestDelegate next, ILoggerFactory logFactory)
        {
            _next = next;
            _logger = logFactory.CreateLogger("WatchLogger-Exception-Logger"); ;
            _recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var requestLog = WatchLogger.RequestLog;
                LogException(ex, requestLog);
                throw;
            }
        }
        public void LogException(Exception ex, RequestModel requestModel)
        {
            Debug.WriteLine("The following exception is logged: " + ex.Message);
            var watchExceptionLog = new WatchExceptionLog();
            watchExceptionLog.EncounteredAt = DateTime.Now;
            watchExceptionLog.Message = ex.Message;
            watchExceptionLog.StackTrace = ex.StackTrace;
            watchExceptionLog.Source = ex.Source;
            watchExceptionLog.TypeOf = ex.GetType().ToString();
            watchExceptionLog.Path = requestModel?.Path;
            watchExceptionLog.Method = requestModel?.Method;
            watchExceptionLog.QueryString = requestModel?.QueryString;
            watchExceptionLog.RequestBody = requestModel?.RequestBody;

            //Insert Log To DB
            _logger.LogInformation(watchExceptionLog.Method + watchExceptionLog.RequestBody + watchExceptionLog.Path);

        }
    }
}
