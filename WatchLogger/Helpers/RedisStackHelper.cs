using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Redis.OM;
using Redis.OM.Searching;

using WatchLogger.Models;

namespace WatchLogger.Helpers
{
    public class RedisStackHelper
    {
         public static RedisConnectionProvider _provider;
         public static RedisCollection<WLog> _redisWatchLog;

        public RedisStackHelper()
        {
             _provider = new RedisConnectionProvider(WatchLogExternalDbConfig.ConnectionString);
            _redisWatchLog = (RedisCollection<WLog>)_provider.RedisCollection<WLog>();
        }
        public async Task<string> InsertWatchLog(WatchLog log)
        {
            var destination = CustomMapper.Mapper.Map<WLog>(log);
            return await _redisWatchLog.InsertAsync(destination);
        }

        public IEnumerable<WatchLog> GETAllWatchLog()
        {
            var logs = _redisWatchLog.ToList();
            List<WatchLog> logList = CustomMapper.Mapper.Map<List<WLog>, List<WatchLog>>((List<WLog>)logs);
            return logList;
        }

        public  WatchLog GetWatchLogById(string id)
        {
            var log = _redisWatchLog.FindById(id);
            var destination = CustomMapper.Mapper.Map<WatchLog>(log);
            return destination;
        }
        public void DeleteWatchLog(string id)
        {
             _provider.Connection.Unlink($"WLog:{id}");
        }
        public void ClearWatchLog()
        {
            var keys = (_provider.Connection.Execute("KEYS *")).ToArray().Select(x => x.ToString());
            var keysList = keys.ToList();
            keysList.ForEach(id =>
            {
                _provider.Connection.Unlink($"WLog:{id}");
            });
        }

        public IEnumerable<WatchLog> FilterByMethod(string method)
        {
            var logs = _redisWatchLog.Where(x => x.Method == method).ToList();
            List<WatchLog> logList = CustomMapper.Mapper.Map<List<WLog>, List<WatchLog>>(logs);
            return logList;
                
        }
        public IEnumerable<WatchLog> FilterByPath(string path)
        {
            var logs = _redisWatchLog.Where(x => x.Path.Contains(path)).ToList();
            List<WatchLog> logList = CustomMapper.Mapper.Map<List<WLog>, List<WatchLog>>(logs);
            return logList;

        }

        public IEnumerable<WatchLog> FilterByStatusCode(int code)
        {
            var logs = _redisWatchLog.Where(x => x.ResponseStatus > 100).ToList();
            List<WatchLog> logList = CustomMapper.Mapper.Map<List<WLog>, List<WatchLog>>(logs);
            return logList;

        }
    }
}
