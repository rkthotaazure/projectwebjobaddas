using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Configuration;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Queue;
using adidas.clb.job.UpdateTriggering.Exceptions;
using adidas.clb.job.UpdateTriggering.Utility;
namespace adidas.clb.job.UpdateTriggering.Helpers
{
    public class AzureQueues
    {
        //Application insights interface reference for logging the error details into Application Insight azure service.
        static IAppInsight InsightLogger { get { return AppInsightLogger.Instance; } }
        //getting Max Retry count,MaxThreadSleepInMilliSeconds from web.config
        public static int maxThreadSleepInMilliSeconds = Convert.ToInt32(ConfigurationManager.AppSettings["MaxThreadSleepInMilliSeconds"]);
        public static int maxRetryCount = Convert.ToInt32(ConfigurationManager.AppSettings["MaxRetryCount"]);
        /// <summary>
        /// This method creates the queue client
        /// </summary>
        /// <returns></returns>
        public static CloudQueueClient GetQueueClient()
        {
            string callerMethodName = string.Empty;
            try
            {      
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                // Parse the connection string and return a reference to the storage account
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["UpdateTriggerAzureQueues"]);
               
                // Create the queue client.
                CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
                // set retry for the connection for transient failures
                queueClient.DefaultRequestOptions = new QueueRequestOptions
                {
                    RetryPolicy = new LinearRetry(TimeSpan.FromSeconds(5), 3)
                };
                return queueClient;

            }
            catch (Exception innerexception)
            {
                InsightLogger.Exception(innerexception.Message, innerexception, "Retrieveentity");
                throw new DataAccessException(innerexception.Message, innerexception.InnerException);
            }

        }
        /// <summary>
        /// This method Retrieves a reference to a Update Trigger Input Queue.
        /// </summary>
        /// <param name="queuePath"></param>
        /// <returns></returns>
        public static CloudQueue GetInputQueue(CloudQueueClient queuePath)
        {
            string callerMethodName = string.Empty;
            try
            {
               
                callerMethodName = CallerInformation.TrackCallerMethodName();
                //read queue name from app.config :: AppSettings and return queue
                CloudQueue queue = queuePath.GetQueueReference(ConfigurationManager.AppSettings["UpdateTriggerInputQueue"]);                
                return queue;

            }

            catch (Exception innerexception)
            {
                InsightLogger.Exception(innerexception.Message, innerexception, "Retrieveentity");
                throw new DataAccessException(innerexception.Message, innerexception.InnerException);
            }           
        }
        /// <summary>
        /// This method Retrieves a reference to a Request PDF Queue.
        /// </summary>
        /// <param name="queuePath"></param>
        /// <returns></returns>
        public static CloudQueue GetRequestPDFQueue(CloudQueueClient queuePath)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                //read queue name from app.config :: AppSettings
                return queuePath.GetQueueReference(ConfigurationManager.AppSettings["RequsetPDFQueue"]);
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                throw new DataAccessException(exception.Message, exception.InnerException);
            }
        }


    }
}
