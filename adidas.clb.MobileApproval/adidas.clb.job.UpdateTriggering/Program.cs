//-----------------------------------------------------------
// <copyright file="Program.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using adidas.clb.job.UpdateTriggering.Exceptions;
using adidas.clb.job.UpdateTriggering.Helpers;
using adidas.clb.job.UpdateTriggering.Utility;
using Microsoft.Azure;
using Microsoft.Azure.WebJobs;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace adidas.clb.job.UpdateTriggering
{
    // To learn more about Microsoft Azure WebJobs SDK, please see http://go.microsoft.com/fwlink/?LinkID=320976
    class Program
    {
        static IAppInsight InsightLogger { get { return AppInsightLogger.Instance; } }
        // Please set the following connection strings in app.config for this WebJob to run:
        // AzureWebJobsDashboard and AzureWebJobsStorage
        static void Main()
        {
            try
            {
               
                //Download Azcopy.exe from blobcontainer for uploading pdf files into blob container
                AzCopyConfig.LoadAzCopyConfigFromBlob();
                //Download Logo from imagescontainer for PDF Files
                AzCopyConfig.LoadImageFromBlob();
                JobHostConfiguration config = new JobHostConfiguration();
                // Add Triggers and Binders for Timer Trigger.               
                config.UseTimers();
                config.NameResolver = new MyResolver();
                JobHost host = new JobHost(config);
                // The following code ensures that the WebJob will be running continuously
                host.RunAndBlock();

            }
            catch (Exception exception)
            {               
                InsightLogger.Exception("Error in Progaram :: Main() method, Error Message:" + exception.Message,exception, "Main");
            }

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
                        //if updatetriggering queue method has triggered then send ut queue name to method call
                        case CoreConstants.AzureQueues.UTQueue:
                            queueName = Convert.ToString(ConfigurationManager.AppSettings["UpdateTriggerInputQueue"]);
                            break;
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
