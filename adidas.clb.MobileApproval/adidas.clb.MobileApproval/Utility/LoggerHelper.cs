using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace adidas.clb.MobileApproval.Utility
{
    /// <summary>
    /// The class which implements logging.
    /// </summary>
    public class LoggerHelper
    {
        private static readonly ILog log = LogManager.GetLogger("aiAppender");

        /// <summary>
        /// WriteToLog method to handle logs by log4Net based on priority and category
        /// </summary>
        /// <param name="message"></param>
        /// <param name="priority"></param>
        /// <param name="category"></param>
        public static void WriteToLog(string message, string priority, string category)
        {
            Log4NetLogger.Info(string.Concat(System.DateTime.Now.ToString(), " - Category : ", category, " - Priority : ", priority));

            // Log4Net Switch case for Exception category
            switch (category)
            {
                case "General":
                    Log4NetLogger.Info(message);
                    break;
                case "Trace":
                    Log4NetLogger.Info(message);
                    break;
                case "Error":
                    Log4NetLogger.Error(message);
                    break;
            }
        }

        /// <summary>
        /// Log4Net Logger Class to log the exception and Error/Information
        /// </summary>
        public static class Log4NetLogger
        {
            private static log4net.ILog Log { get; set; }

            // Log4Net to get the Log path
            static Log4NetLogger()
            {
                Log = log4net.LogManager.GetLogger(typeof(Log4NetLogger));
            }
            // Log4Net Error method only input as message
            public static void Error(string msg)
            {
                Log.Error(msg);
            }

            // Log4Net Error method input as message and exception
            public static void Error(string msg, Exception ex)
            {
                Log.Error(msg, ex);
            }

            // Log4Net Information method input as message
            public static void Info(object msg)
            {
                Log.Info(msg);
            }

            // Log4Net Error method input as message and params
            public static void Error(string message, params object[] parameters)
            {
                Error(message, null, parameters);
            }

            // Log4Net Error method input as exception
            public static void Error(Exception exception)
            {
                if (exception == null)
                    return;
                Error(exception.ToString(), exception);
            }
        }
    }
}