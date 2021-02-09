using System;
using System.Collections.Generic;
using System.Text;

namespace XLoad.Helpers
{
    public static class LogHelper
    {
        public static void ConditionalLog(string data, bool log)
        {
            if (log)
            {
                Console.WriteLine(data);
            }
        }
    }
}
