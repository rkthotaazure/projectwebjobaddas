using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using adidas.clb.job.GeneratePDF.Exceptions;
using adidas.clb.job.GeneratePDF.Utility;
namespace adidas.clb.job.GeneratePDF
{
    // To learn more about Microsoft Azure WebJobs SDK, please see http://go.microsoft.com/fwlink/?LinkID=320976
    class Program
    {
        static IAppInsight InsightLogger { get { return AppInsightLogger.Instance; } }
        // Please set the following connection strings in app.config for this WebJob to run:
        // AzureWebJobsDashboard and AzureWebJobsStorage
        static void Main()
        {
            JobHostConfiguration config = new JobHostConfiguration();
            // Add Triggers and Binders for Timer Trigger.             
            config.NameResolver = new MyResolver();
            JobHost host = new JobHost(config);
            // The following code ensures that the WebJob will be running continuously
            host.RunAndBlock();
        }

        /// <summary>
        /// Dynamically send the queue names from app.config file to queue trigger method
        /// </summary>
        public class MyResolver : INameResolver
        {
            public string Resolve(string name)
            {
                try
                {
                    string queueName = string.Empty;
                    switch (name)
                    {
                        //if generate pdf queue method has triggered then send generate pdf queue name to method call
                        case CoreConstants.AzureQueues.PDFQueue:
                            queueName = Convert.ToString(ConfigurationManager.AppSettings["GeneratePDFQueue"]);
                            break;
                        default:
                            break;
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
