//-----------------------------------------------------------
// <copyright file="DataProvider.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using adidas.clb.job.UpdateTriggering.Models;
using adidas.clb.job.UpdateTriggering.Exceptions;
using adidas.clb.job.UpdateTriggering.Utility;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
namespace adidas.clb.job.UpdateTriggering.App_Data.DAL
{
    /// The class that contains the methods realted to connecting azure table storage.
    public static class DataProvider
    {
        //Application insights interface reference for logging the error details into Application Insight azure service.
        static IAppInsight InsightLogger { get { return AppInsightLogger.Instance; } }
        /// <summary>
        /// method to get azure table storage object instance
        /// </summary>
        /// <param name="TableName"></param>
        /// <returns>Azure Table Instance</returns>        
        public static CloudTable GetAzureTableInstance(String TableName)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                // Retrieve the storage account from the connection string.
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["GenericMobileStorageConnectionString"]);
                // Create the table client.
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
                // Create the CloudTable object that represents the table.
                CloudTable table = tableClient.GetTableReference(TableName);
                return table;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                throw new DataAccessException(exception.Message, exception.InnerException);
            }
           
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

        /// <summary>
        /// This method retrieves the azure table entity information
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tablename"></param>
        /// <param name="partitionkey"></param>
        /// <param name="rowkey"></param>
        /// <returns></returns>
        public static T Retrieveentity<T>(string tablename, string partitionkey, string rowkey) where T : ITableEntity, new()
        {
            //get's azure table instance
            CloudTable ReferenceDataTable = GetAzureTableInstance(tablename);
            TableOperation RetrieveUser = TableOperation.Retrieve<T>(partitionkey, rowkey);
            TableResult RetrievedResultUser = ReferenceDataTable.Execute(RetrieveUser);
            return (T)RetrievedResultUser.Result;
        }
        /// <summary>
        /// This method updates the azure table entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tablename"></param>
        /// <param name="entity"></param>
        public static void UpdateEntity<T>(string tablename, T entity) where T : ITableEntity, new()
        {
            CloudTable ReferenceDataTable = GetAzureTableInstance(tablename);
            entity.ETag = "*";
            TableOperation updateOperation = TableOperation.Replace(entity);
            ReferenceDataTable.Execute(updateOperation);
        }
        /// <summary>
        /// This method Inserts thenew entity in azure table
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tablename"></param>
        /// <param name="entity"></param>
        public static void InsertEntity<T>(string tablename, T entity) where T : ITableEntity, new()
        {
            CloudTable ReferenceDataTable = GetAzureTableInstance(tablename);
            //entity.ETag = "*";
            TableOperation InsertOperation = TableOperation.Insert(entity);
            ReferenceDataTable.Execute(InsertOperation);
        }

    }
}