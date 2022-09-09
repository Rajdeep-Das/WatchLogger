
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using Redis.OM;
using Redis.OM.Searching;
using System;
using WatchLogger.Helpers;
using WatchLogger.Models;

namespace WatchLogger.Controllers
{
    [Route("watchlogger")]
    [ApiController]
    public class WatchLoggerController : ControllerBase
    {
      
        private readonly RedisStackHelper _redisStackHelper;

        public WatchLoggerController(RedisStackHelper redisStackHelper)
        {
           _redisStackHelper = redisStackHelper;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var allLogs = _redisStackHelper.GETAllWatchLog();
            return Ok(allLogs);
        }

        [HttpGet("filter")]
        public IActionResult GetAllWithFilters(string searchString = "", string verbString = "", string statusCode = "")
        {
            var logs = _redisStackHelper.GETAllWatchLog();
            if (logs != null)
            {
                if (!string.IsNullOrEmpty(searchString))
                {
                    searchString = searchString.ToLower();
                    logs = logs.Where(l => l.Path.ToLower().Contains(searchString) || l.Method.ToLower().Contains(searchString) || l.ResponseStatus.ToString().Contains(searchString) || (!String.IsNullOrEmpty(l.QueryString) && l.QueryString.ToLower().Contains(searchString)));
                }

                if (!string.IsNullOrEmpty(verbString))
                {
                    logs = logs.Where(l => l.Method.ToLower() == verbString.ToLower());
                }

                if (!string.IsNullOrEmpty(statusCode))
                {
                    logs = logs.Where(l => l.ResponseStatus.ToString() == statusCode);
                }
            }
            logs = logs.OrderByDescending(x => x.StartTime);

            return Ok(logs);
        }
    }
}
