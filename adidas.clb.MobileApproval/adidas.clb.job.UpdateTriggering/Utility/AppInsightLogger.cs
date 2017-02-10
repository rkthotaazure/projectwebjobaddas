using Microsoft.ApplicationInsights;
using Microsoft.WindowsAzure;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime;
/// <summary>
/// implements IAppInsight interface
/// </summary>

namespace adidas.clb.job.UpdateTriggering.Utility
{
    public sealed class AppInsightLogger : IAppInsight
    {
        //Create object for AppInsightLogger class
        private static readonly AppInsightLogger instance = new AppInsightLogger();
        //create telemetry client object
        private TelemetryClient client = new TelemetryClient();
        //Read instrumentationkey from app.config
        private static string InstrumentationKey = ConfigurationManager.AppSettings["APPINSIGHTS_INSTRUMENTATIONKEY"];
        //Read IsTraceEnabledForExceptions value from app.config
        bool IsTraceEnabledForExceptions = Convert.ToBoolean(ConfigurationManager.AppSettings["AppInsightTraceForExceptions"]);
        //Read IsTraceEnabledForEvents value from app.config
        bool IsTraceEnabledForEvents = Convert.ToBoolean(ConfigurationManager.AppSettings["AppInsightTraceForCustomEvents"]);
        //Read IsTraceEnabledForMetrics value from app.config
        bool IsTraceEnabledForMetrics = Convert.ToBoolean(ConfigurationManager.AppSettings["AppInsightTraceForMetrics"]);
        private AppInsightLogger() { }
        //cosntrut the values
        public static AppInsightLogger Instance
        {
            get
            {
                return instance;
            }
        }
        /// <summary>
        /// This method log User actions and other events. Used to track user behavior or to monitor performance.
        /// </summary>
        /// <param name="message"></param>
        public void TrackEvent(string message)
        {
            if (IsTraceEnabledForEvents)
            {
                //assign Instrumentation Key to TelemetryClient object            
                client.InstrumentationKey = InstrumentationKey;
                client.TrackEvent(message);
                //flush the buffer data
                client.Flush();
            }
        }
        public void TrackStartEvent(string methodname)
        {
            if (IsTraceEnabledForEvents)
            {
                //assign Instrumentation Key to TelemetryClient object            
                client.InstrumentationKey = InstrumentationKey;
                //get AppInsightMessageForMethodStart from app.config
                string startMsg = string.Format(ConfigurationManager.AppSettings["AppInsightMessageForMethodStart"], methodname);
                client.TrackEvent(startMsg);
                //flush the buffer data
                client.Flush();
            }
        }
        public void TrackEndEvent(string methodname)
        {
            if (IsTraceEnabledForEvents)
            {
                //assign Instrumentation Key to TelemetryClient object            
                client.InstrumentationKey = InstrumentationKey;
                //get AppInsightMessageForMethodStart from app.config
                string startMsg = string.Format(ConfigurationManager.AppSettings["AppInsightMessageForMethodEnd"], methodname);
                client.TrackEvent(startMsg);
                //flush the buffer data
                client.Flush();
            }
        }
        /// <summary>
        /// This method logs Performance measurements such as queue lengths...etc.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="duration"></param>
        public void TrackMetric(string message, long duration)
        {
            if (IsTraceEnabledForMetrics)
            {
                //assign Instrumentation Key to TelemetryClient object 
                client.InstrumentationKey = InstrumentationKey;
                client.TrackMetric(message, duration);
                //flush the buffer data
                client.Flush();
            }
        }
        /// <summary>
        /// This method Log exceptions for diagnosis. Trace where they occur in relation to other events and examine stack traces.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        /// <param name="EventID"></param>
        public void Exception(string message, Exception exception, string methodName)
        {
            //check TraceEnable or not from app.config
            if (IsTraceEnabledForExceptions)
            {
                string exceptionMsg = message;
                //assign Instrumentation Key to TelemetryClient object 
                client.InstrumentationKey = InstrumentationKey;
                //create dictionary object <string,string>
                Dictionary<string, string> prop = new Dictionary<string, string>();
                //add exception.message and Methodname to dictionay
                //checking is innerexception is null or not
                //if it is null add exception strack trace to dictionary
                if (exception.InnerException == null)
                {
                    exceptionMsg = exception.ToString();
                }
                prop["Message"] = exceptionMsg;
                prop["MethodName"] = methodName;
                //log the exception
                client.TrackException(exception, prop);
                //flush the buffer data
                client.Flush();
            }
        }






    }

}
