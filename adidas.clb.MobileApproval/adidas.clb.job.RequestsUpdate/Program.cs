//-----------------------------------------------------------
// <copyright file="Program.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace adidas.clb.job.RequestsUpdate
{
    // To learn more about Microsoft Azure WebJobs SDK, please see http://go.microsoft.com/fwlink/?LinkID=320976
    class Program
    {
        // Please set the following connection strings in app.config for this WebJob to run:
        // AzureWebJobsDashboard and AzureWebJobsStorage
        static void Main()
        {
            JobHostConfiguration config = new JobHostConfiguration();
            //QueueTrigger function runs singleton on a single instance
            config.Queues.BatchSize = Convert.ToInt32(ConfigurationManager.AppSettings["QueueBatchSize"]);
            var host = new JobHost(config);
            log4net.Config.XmlConfigurator.Configure();
            // The following code ensures that the WebJob will be running continuously
            host.RunAndBlock();
        }
    }
}
