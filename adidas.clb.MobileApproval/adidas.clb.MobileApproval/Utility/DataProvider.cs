//-----------------------------------------------------------
// <copyright file="DataProvider.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Queue;
using adidas.clb.MobileApproval.Models;

namespace adidas.clb.MobileApproval.Utility
{
    /// The class that contains the methods realted to connecting azure table storage.
    public static class DataProvider
    {
        /// <summary>
        /// method to get azure table storage object instance
        /// </summary>
        /// <param name="TableName">takes tablename as input</param>
        /// <returns>Azure Table Instance</returns>        
        public static CloudTable GetAzureTableInstance(String TableName)
        {
            // Retrieve the storage account from the connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
            CloudConfigurationManager.GetSetting(CoreConstants.AzureTables.AzureStorageConnectionString));
            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();            
            // Create the CloudTable object that represents the table.
            CloudTable table = tableClient.GetTableReference(TableName);
            return table;
        }

        /// <summary>
        /// method to get azure queue storage object instance
        /// </summary>
        /// <param name="QueueName">takes queuename as inpu</param>
        /// <returns>Azure Queue Instance</returns>        
        public static CloudQueue GetAzureQueueInstance(String QueueName)
        {
            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting(CoreConstants.AzureTables.AzureStorageConnectionString));
            // Create the queue client.
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            // Retrieve a reference to a queue.
            CloudQueue queue = queueClient.GetQueueReference(QueueName);
            return queue;
        }

        /// <summary>
        /// Method wich used to map properties of two objects
        /// </summary>
        /// <typeparam name="T1">takes output type</typeparam>
        /// <typeparam name="T2">takes input type</typeparam>
        /// <param name="inputModel">takes input object</param>
        /// <returns>returns mapped object</returns>
        public static T1 ResponseObjectMapper<T1, T2>(T2 inputModel)
        {
            var entity = Activator.CreateInstance<T1>();            
            var properties = inputModel.GetType().GetProperties();
            foreach (var entry in properties)
            {
                var propertyInfo = entity.GetType().GetProperty(entry.Name);
                if (propertyInfo != null)
                    propertyInfo.SetValue(entity, entry.GetValue(inputModel), null);
            }
            return entity;
        }

        /// <summary>
        /// method to form error response
        /// </summary>
        /// <typeparam name="T">takes type as input</typeparam>
        /// <param name="code">takes code as input</param>
        /// <param name="shorttext">takes short text as input</param>
        /// <param name="longtext">takes long text as input</param>
        /// <returns>returns error response</returns>
        public static PersonalizationResponseDTO<T> PersonalizationResponseError<T>(string code, string shorttext, string longtext)
        {
            var ResponseBackends = new PersonalizationResponseDTO<T>();
            ResponseBackends.result = default(T);
            ResponseBackends.error = new ErrorDTO(code, shorttext, longtext);
            return ResponseBackends;
        }
            
    }
}