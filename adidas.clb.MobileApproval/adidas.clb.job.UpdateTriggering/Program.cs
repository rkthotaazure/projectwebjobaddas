//-----------------------------------------------------------
// <copyright file="Program.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using adidas.clb.job.UpdateTriggering.Exceptions;
using adidas.clb.job.UpdateTriggering.Helpers;
using adidas.clb.job.UpdateTriggering.Utility;
using adidas.clb.job.UpdateTriggering.App_Data.DAL;
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
    public class Program
    {
        static IAppInsight InsightLogger { get { return AppInsightLogger.Instance; } }
        // Please set the following connection strings in app.config for this WebJob to run:
        // AzureWebJobsDashboard and AzureWebJobsStorage
        static void Main()
        {
            try
            {             
                
                JobHostConfiguration config = new JobHostConfiguration();
                //QueueTrigger function runs singleton on a single instance
                config.Queues.BatchSize = Convert.ToInt32(ConfigurationManager.AppSettings["QueueBatchSize"]);
                // Add Triggers and Binders for Timer Trigger.               
                config.UseTimers();
                config.NameResolver = new MyResolver();
                JobHost host = new JobHost(config);

                // This method inserts/updates the next collecting time(Regular/Missed) in Azure ReferenceData Table
                // This method should not run continuously,it runs only when the web job has started.
                host.Call(typeof(Program).GetMethod("UpdateNextCollectingTime"));
                // The following code ensures that the WebJob will be running continuously
                host.RunAndBlock();

            }
            catch (Exception exception)
            {               
                InsightLogger.Exception("Error in Progaram :: Main() method, Error Message:" + exception.Message,exception, "Main");
            }

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
                    //if updatetriggering queue method has triggered then send ut queue name to method call
                    if (name == CoreConstants.AzureQueues.UTQueue)
                    {
                        queueName = Convert.ToString(ConfigurationManager.AppSettings["UpdateTriggerInputQueue"]);
                    }
                    else if (name == CoreConstants.AzureQueues.VIPQueue)
                    {
                        queueName = Convert.ToString(ConfigurationManager.AppSettings["VIPMessagesQueue"]);
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
        /// <summary>
        /// This method inserts/updates the next collecting time(Regular/Missed) in Azure ReferenceData Table
        /// This method should not run continuously,it runs only when the web job has started
        /// </summary>
        [NoAutomaticTrigger]
        public static void UpdateNextCollectingTime()
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                ////Create object for NextUserCollectingTime class
                bool IsFirstTime = true;
                NextUserCollectingTimeDAL objdal = new NextUserCollectingTimeDAL();
                //call the UpdateNextCollectingTime method which will update the Next Collecting Time of the each backend
                objdal.UpdateNextCollectingTime(IsFirstTime);
            }
            catch (DataAccessException dalexception)
            {               
                //write data layer exception into application insights
                InsightLogger.Exception(dalexception.Message, dalexception, callerMethodName);
            }
            catch (Exception exception)
            {               
                //write exception into application insights
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
            }
            
        }
    }
}
