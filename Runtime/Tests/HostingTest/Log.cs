using System;
using System.Collections.Generic;
using System.Text;

namespace HostingTest
{

    /// <summary>
    /// This class is a place holder for a logger class
    /// 
    /// The stubs are losely based on log4j logger class
    /// 
    /// Could use reflection to capture the file/class/method
    /// that logging is called from.
    /// </summary>
    public static class Log
    {
        public static void Debug(string msg)
        {
        }
        public static void Error(string msg)
        {
        }
        public static void Trace(string msg)
        {
        
        }
        
        public static void Warn(string msg)
        {
        
        }
        public static void Info(string msg)
        {
        
        }
        public static void Fatal(string msg)
        {
        
        }

        /// <summary>
        /// To put in context why a test failed.
        /// </summary>
        /// <param name="msg"></param>
        public static void Fail(string msg)
        {
        }

    }
}
