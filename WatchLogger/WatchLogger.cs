using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using Redis.OM;
using System.Runtime.InteropServices;
using WatchLogger.Exceptions;
using WatchLogger.Helpers;
using WatchLogger.HostedService;
using WatchLogger.Models;

namespace WatchLogger
{
    public class WatchLogger
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        public static RequestModel RequestLog;
        private readonly RedisStackHelper _redisStackHelper;
        private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;

        public WatchLogger(RequestDelegate next, ILoggerFactory logFactory,RedisStackHelper redisStackHelper)
        {
            _next = next;
            _logger = logFactory.CreateLogger("WatchLoggerMiddleWare");
            _recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
            _redisStackHelper = redisStackHelper;
          
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var requestLog = await LogRequest(context);
            var responseLog = await LogResponse(context);
            var timeSpent = responseLog.FinishTime.Subtract(requestLog.StartTime);
           
            //Build General WatchLog, Join from requestLog and responseLog

            var watchLog = new WatchLog
            {
               
                IpAddress = context.Connection.RemoteIpAddress.ToString(),
                ResponseStatus = responseLog.ResponseStatus,
                QueryString = requestLog.QueryString,
                Method = requestLog.Method,
                Path = requestLog.Path,
                Host = requestLog.Host,
                RequestBody = requestLog.RequestBody,
                ResponseBody = responseLog.ResponseBody,
                TimeSpent = string.Format("{0:D1} hrs {1:D1} mins {2:D1} secs {3:D1} ms", timeSpent.Hours, timeSpent.Minutes, timeSpent.Seconds, timeSpent.Milliseconds),
                RequestHeaders = requestLog.Headers,
                ResponseHeaders = responseLog.Headers,
                StartTime = requestLog.StartTime,
                EndTime = responseLog.FinishTime
            };
            
            _logger.LogInformation("----------- Request Info  ------ ");
            _logger.LogInformation(requestLog.Method);
            _logger.LogInformation(watchLog.ResponseBody.ToString());
          
            _logger.LogInformation("----------- DB Operation Start  ------ ");
            var resullt = await _redisStackHelper.InsertWatchLog(watchLog);
            _logger.LogInformation("----------- Insert DB Done  ------ ");

        }

        private async Task<RequestModel> LogRequest(HttpContext context)
        {
            var startTime = DateTime.Now;
            List<string> requestHeaders = new List<string>();

            var requestBodyDto = new RequestModel()
            {
                RequestBody = string.Empty,
                Host = context.Request.Host.ToString(),
                Path = context.Request.Path.ToString(),
                Method = context.Request.Method.ToString(),
                QueryString = context.Request.QueryString.ToString(),
                StartTime = startTime,
                Headers = context.Request.Headers.Select(x => x.ToString()).Aggregate((a, b) => a + ": " + b),
            };


            if (context.Request.ContentLength > 1)
            {
                context.Request.EnableBuffering();
                await using var requestStream = _recyclableMemoryStreamManager.GetStream();
                await context.Request.Body.CopyToAsync(requestStream);
                requestBodyDto.RequestBody = GeneralHelper.ReadStreamInChunks(requestStream);
                context.Request.Body.Position = 0;
            }
            RequestLog = requestBodyDto;
            return requestBodyDto;
        }

        private async Task<ResponseModel> LogResponse(HttpContext context)
        {
            var responseBody = string.Empty;
            using (var originalBodyStream = context.Response.Body)
            {
                try
                {
                    using (var originalResponseBody = _recyclableMemoryStreamManager.GetStream())
                    {
                        context.Response.Body = originalResponseBody;
                        await _next(context);
                        context.Response.Body.Seek(0, SeekOrigin.Begin);
                        responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
                        context.Response.Body.Seek(0, SeekOrigin.Begin);
                        var responseBodyDto = new ResponseModel
                        {
                            ResponseBody = responseBody,
                            ResponseStatus = context.Response.StatusCode,
                            FinishTime = DateTime.Now,
                            Headers = context.Response.StatusCode != 200 || context.Response.StatusCode != 201 ? "" : context.Response.Headers.Select(x => x.ToString()).Aggregate((a, b) => a + ": " + b),
                        };
                        await originalResponseBody.CopyToAsync(originalBodyStream);
                        return responseBodyDto;
                    }
                }
                finally
                {
                    context.Response.Body = originalBodyStream;
                }
            }
        }

    }

}