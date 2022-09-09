using Redis.OM.Modeling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WatchLogger.Helpers
{
    [Document(StorageType = StorageType.Json, Prefixes = new[] { "WLog" })]
    public class WLog
    {
        [RedisIdField]
        [Indexed]
        public string Id { get; set; }
    
        [Indexed]
        public string ResponseBody { get; set; }
        [Indexed]
        public int ResponseStatus { get; set; }
     
        [Indexed]
        public string RequestBody { get; set; }
      
        [Indexed]
        public string QueryString { get; set; }
        
        [Indexed]
        public string Path { get; set; }
       
        [Indexed]
        public string RequestHeaders { get; set; }
        [Indexed]
        public string ResponseHeaders { get; set; }
        [Indexed]
        public string Method { get; set; }
        [Indexed]
        public string Host { get; set; }
        
        [Indexed]
        public string IpAddress { get; set; }
        [Indexed]
        public string TimeSpent { get; set; }
     
        [Indexed]
        public DateTime StartTime { get; set; }
        [Indexed]
        public DateTime EndTime { get; set; }
    }
}
