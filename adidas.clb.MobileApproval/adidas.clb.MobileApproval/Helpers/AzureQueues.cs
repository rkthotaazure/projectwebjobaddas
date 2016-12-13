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
using adidas.clb.MobileApproval.Exceptions;
using adidas.clb.MobileApproval.Utility;
namespace adidas.clb.MobileApproval.Helpers
{
    public class AzureQueues
    {
        public static CloudQueueClient GetQueueClient()
        {
            try
            {
                // Parse the connection string and return a reference to the storage account
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["GenericMobileStorageConnectionString"]);
                // Create the queue client.
                CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
                return queueClient;
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + "Error while creating queue client in  - AzureQueues :: GetQueueClient() "
                  + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);

                throw new DataAccessException(exception.Message, exception.InnerException);
            }

        }

        public static CloudQueue GetRequestUpdateQueue(CloudQueueClient queuePath)
        {
            try
            {
                //read queue name from app.config :: AppSettings
                return queuePath.GetQueueReference(ConfigurationManager.AppSettings["RequestUpdateQueue"]);
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error while read queue name from app.config in  AzureQueues :: GetInputQueue()"
                  + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);

                throw new DataAccessException(exception.Message, exception.InnerException);
            }
        }
       


    }
}
