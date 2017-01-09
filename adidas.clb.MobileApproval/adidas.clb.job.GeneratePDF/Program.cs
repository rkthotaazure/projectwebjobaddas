using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using adidas.clb.job.GeneratePDF.Exceptions;
using adidas.clb.job.GeneratePDF.Utility;
using adidas.clb.job.GeneratePDF.Helpers;
namespace adidas.clb.job.GeneratePDF
{
    // To learn more about Microsoft Azure WebJobs SDK, please see http://go.microsoft.com/fwlink/?LinkID=320976
    class Program
    {
        //Application insights interface reference for logging the error details into Application Insight azure service.
        static IAppInsight InsightLogger { get { return AppInsightLogger.Instance; } }
        // Please set the following connection strings in app.config for this WebJob to run:
        // AzureWebJobsDashboard and AzureWebJobsStorage
        static void Main()
        {
            //Download Azcopy.exe from blobcontainer for uploading pdf files into blob container
            AzCopyConfig.LoadAzCopyConfigFromBlob();
            //Download Logo from imagescontainer for PDF Files
            AzCopyConfig.LoadImageFromBlob();
            JobHostConfiguration config = new JobHostConfiguration();
            // Add Triggers and Binders for Timer Trigger.             
            config.NameResolver = new MyResolver();
            JobHost host = new JobHost(config);
            // The following code ensures that the WebJob will be running continuously
            host.RunAndBlock();
        }

        /// <summary>
        /// Dynamically defines a %name% variables i.e write a queue names from app.config file to queue trigger method
        /// </summary>
        public class MyResolver : INameResolver
        {
            public string Resolve(string name)
            {
                try
                {
                    string queueName = string.Empty;
                    //if generate pdf queue method has triggered then send generate pdf queue name to method call
                    if (name == CoreConstants.AzureQueues.PDFQueue)
                    {
                        queueName = Convert.ToString(ConfigurationManager.AppSettings["GeneratePDFQueue"]);
                    }                   
                    return queueName;
                }
                catch (Exception exception)
                {
                    InsightLogger.Exception("Error in MyResolver :: Resolve() method, Error Message:" + exception.Message, exception, "Main");
                    throw exception;
                }
            }
        }
        
       
    }
}
