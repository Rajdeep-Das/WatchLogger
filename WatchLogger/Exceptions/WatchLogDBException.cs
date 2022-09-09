using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WatchLogger.Exceptions
{
    internal class WatchLogDBException : Exception
    {
        public WatchLogDBException()
        {

        }
        internal WatchLogDBException(string message)
            : base(String.Format("WatchDog Database Exception: {0} Ensure you have passed the right Redis Connection String  at .AddWatchDogServices() as well as all required parameters for the database connection string", message))
        {

        }
    }
}
