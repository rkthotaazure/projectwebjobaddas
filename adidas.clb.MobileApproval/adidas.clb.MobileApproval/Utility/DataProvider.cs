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
using adidas.clb.MobileApproval.Models;

namespace adidas.clb.MobileApproval.Utility
{
    /// The class that contains the methods realted to connecting azure table storage.
    public static class DataProvider
    {
        /// <summary>
        /// method to get azure table storage object instance
        /// </summary>
        /// <param name="TableName"></param>
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
        /// Method wich used to map properties of two objects
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="inputModel"></param>
        /// <returns></returns>
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

        public static PersonalizationResponseDTO<T> PersonalizationResponseError<T>(string code, string shorttext, string longtext)
        {
            var ResponseBackends = new PersonalizationResponseDTO<T>();
            ResponseBackends.result = default(T);
            ResponseBackends.error = new ErrorDTO(code, shorttext, longtext);
            return ResponseBackends;
        }
            
    }
}