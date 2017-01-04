using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Queue;
using adidas.clb.job.GeneratePDF.Exceptions;
using adidas.clb.job.GeneratePDF.Utility;
namespace adidas.clb.job.GeneratePDF.Helpers
{
    public class AzureQueues
    {
        //Application insights interface reference for logging the error details into Application Insight azure service.
        static IAppInsight InsightLogger { get { return AppInsightLogger.Instance; } }
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
                return queueClient;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                throw new DataAccessException(exception.Message, exception.InnerException);
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
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                //read queue name from app.config :: AppSettings
                return queuePath.GetQueueReference(ConfigurationManager.AppSettings["UpdateTriggerInputQueue"]);
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                throw new DataAccessException(exception.Message, exception.InnerException);
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
