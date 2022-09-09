using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WatchLogger.Helpers
{
    public class WatchLogConfig
    {
        public string SetExternalDbConnString { get; set; } = string.Empty;
    }

    public static class WatchLogExternalDbConfig
    {
        public static string ConnectionString { get; set; } = string.Empty;
    }
}
