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
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Queue;

namespace adidas.clb.job.RequestsUpdate.Utility
{
    /// The class that contains the methods realted to connecting azure table storage.
    public static class DataProvider
    {
        static IAppInsight InsightLogger { get { return AppInsightLogger.Instance; } }
        /// <summary>
        /// method to get azure table storage object instance
        /// </summary>
        /// <param name="TableName">takes table name as input</param>
        /// <returns>Azure Table Instance</returns>        
        public static CloudTable GetAzureTableInstance(string TableName)
        {
            //Get Caller Method name
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                // Retrieve the storage account from the connection string.
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting(CoreConstants.AzureTables.AzureStorageConnectionString));
                // Create the table client.
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
                // set retry for the connection for transient failures
                tableClient.DefaultRequestOptions = new TableRequestOptions
                {
                    RetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(5), 3)
                };
                // Create the CloudTable object that represents the table.
                CloudTable table = tableClient.GetTableReference(TableName);
                return table;
            }
            catch (Exception exception)
            {
                //write exception into application insights
                InsightLogger.Exception(exception.Message + " - Error in DataProver while getting instance of azure table " + TableName, exception, callerMethodName);
                throw new Exception();
            }
        }

        /// <summary>
        /// method to get azure queue storage object instance
        /// </summary>
        /// <param name="QueueName">takes queuename as inpu</param>
        /// <returns>Azure Queue Instance</returns>        
        public static CloudQueue GetAzureQueueInstance(string QueueName)
        {
            try
            {
                // Retrieve storage account from connection string.
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting(CoreConstants.AzureTables.AzureStorageConnectionString));
                // Create the queue client.
                CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
                // set retry for the connection for transient failures
                queueClient.DefaultRequestOptions = new QueueRequestOptions
                {
                    RetryPolicy = new LinearRetry(TimeSpan.FromSeconds(5), 3)
                };
                // Retrieve a reference to a queue.
                CloudQueue queue = queueClient.GetQueueReference(QueueName);
                return queue;
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - exception in dataprovider while getting insatance of azure queue " + QueueName + " :- "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new Exception();
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
            //Get Caller Method name
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
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
            catch (Exception exception)
            {
                //write exception into application insights
                InsightLogger.Exception(exception.Message + " - Error in DataProver while properties matching in object mapper", exception, callerMethodName);
                throw new Exception();
            }
        }

        /// <summary>
        /// method to retrive entity from azure table
        /// </summary>
        /// <typeparam name="T">Takes table entity type as input</typeparam>
        /// <param name="tablename">takes table name as input</param>
        /// <param name="partitionkey">takes partition key as input</param>
        /// <param name="rowkey">takes row key as input</param>
        /// <returns>returns entity</returns>
        public static T Retrieveentity<T>(string tablename, string partitionkey, string rowkey) where T : ITableEntity, new()
        {
            //Get Caller Method name
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                //get's azure table instance
                CloudTable ReferenceDataTable = GetAzureTableInstance(tablename);
                TableOperation RetrieveUser = TableOperation.Retrieve<T>(partitionkey, rowkey);
                TableResult RetrievedResultUser = ReferenceDataTable.Execute(RetrieveUser);
                return (T)RetrievedResultUser.Result;
            }
            catch (Exception exception)
            {
                //write exception into application insights
                InsightLogger.Exception(exception.Message + " - Error in DataProver while retrieving entity from " + tablename, exception, callerMethodName);
                throw new Exception();
            }
        }

        /// <summary>
        /// method to insert entity to azure table
        /// </summary>
        /// <typeparam name="T">Takes table entity type as input</typeparam>
        /// <param name="tablename">takes table name as input</param>
        /// <param name="entity">takes entity as input to insert</param>
        public static void InsertEntity<T>(string tablename, T entity) where T : ITableEntity, new()
        {
            //Get Caller Method name
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                CloudTable ReferenceDataTable = GetAzureTableInstance(tablename);
                TableOperation insertOperation = TableOperation.Insert(entity);
                ReferenceDataTable.Execute(insertOperation);
            }
            catch (Exception exception)
            {
                //write exception into application insights
                InsightLogger.Exception(exception.Message + " - Error in DataProver while inserting entity in " + tablename, exception, callerMethodName);
                throw new Exception();
            }
        }

        /// <summary>
        /// method to insertor replace entity to azure table
        /// </summary>
        /// <typeparam name="T">Takes table entity type as input</typeparam>
        /// <param name="tablename">takes table name as input</param>
        /// <param name="entity">takes entity as input to insert</param>
        public static void InsertReplaceEntity<T>(string tablename, T entity) where T : ITableEntity, new()
        {
            //Get Caller Method name
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                CloudTable ReferenceDataTable = GetAzureTableInstance(tablename);
                TableOperation insertreplaceOperation = TableOperation.InsertOrReplace(entity);
                ReferenceDataTable.Execute(insertreplaceOperation);
            }
            catch (Exception exception)
            {
                //write exception into application insights
                InsightLogger.Exception(exception.Message + " - Error in DataProver while insert or replace entity in " + tablename, exception, callerMethodName);
                throw new Exception();
            }
        }

        /// <summary>
        /// method to add multiple entities to azure table
        /// </summary>
        /// <typeparam name="T">Takes table entity type as input</typeparam>
        /// <param name="tablename">takes table name as input</param>
        /// <param name="entitieslist">takes entities list as input</param>
        public static void AddEntities<T>(string tablename, List<T> entitieslist) where T : ITableEntity, new()
        {
            //Get Caller Method name
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                //get's azure table instance
                CloudTable UserBackendConfigurationTable = GetAzureTableInstance(tablename);
                TableBatchOperation batchOperation = new TableBatchOperation();
                //insert list of entities into batch operation
                foreach (T entity in entitieslist)
                {
                    batchOperation.InsertOrReplace(entity);
                }
                UserBackendConfigurationTable.ExecuteBatch(batchOperation);
            }
            catch (Exception exception)
            {
                //write exception into application insights
                InsightLogger.Exception(exception.Message + " - Error in DataProver while adding entities to " + tablename, exception, callerMethodName);
                throw new Exception();
            }
        }

        /// <summary>
        /// method to update entity to azure table
        /// </summary>
        /// <typeparam name="T">Takes table entity type as input</typeparam>
        /// <param name="tablename">takes table name as input</param>
        /// <param name="entity">takes entity as input to insert</param>
        public static void UpdateEntity<T>(string tablename, T entity) where T : ITableEntity, new()
        {
            //Get Caller Method name
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                CloudTable ReferenceDataTable = GetAzureTableInstance(tablename);
                entity.ETag = "*";
                TableOperation updateOperation = TableOperation.Replace(entity);
                ReferenceDataTable.Execute(updateOperation);
            }
            catch (Exception exception)
            {
                //write exception into application insights
                InsightLogger.Exception(exception.Message + " - Error in DataProver while updating entity in " + tablename, exception, callerMethodName);
                throw new Exception();
            }
        }

        /// <summary>
        /// method to get entities list from azure table
        /// </summary>
        /// <typeparam name="T">Takes table entity type as input</typeparam>
        /// <param name="tablename">takes table name as input</param>
        /// <param name="query">takes query as input</param>
        /// <returns>returns entities list</returns>
        public static List<T> GetEntitiesList<T>(string tablename, TableQuery<T> query) where T : ITableEntity, new()
        {
            //Get Caller Method name
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                //get's azure table instance
                CloudTable UserDeviceConfigurationTable = DataProvider.GetAzureTableInstance(tablename);
                //TableQuery<BackendEntity> query = new TableQuery<BackendEntity>().Where(TableQuery.GenerateFilterCondition(CoreConstants.AzureTables.PartitionKey, QueryComparisons.Equal, CoreConstants.AzureTables.Backend));
                List<T> allentities = UserDeviceConfigurationTable.ExecuteQuery(query).ToList();
                return allentities;
            }
            catch (Exception exception)
            {
                //write exception into application insights
                InsightLogger.Exception(exception.Message + " - Error in DataProver while getting entities from " + tablename, exception, callerMethodName);
                throw new Exception();
            }
        }

        /// <summary>
        /// method to delete multiple entities from azure table
        /// </summary>
        /// <typeparam name="T">Takes table entity type as input</typeparam>
        /// <param name="tablename">takes table name as input</param>
        /// <param name="entitieslist">takes entities list as input</param>
        public static void RemoveEntities<T>(string tablename, List<T> entitieslist) where T : ITableEntity, new()
        {
            //Get Caller Method name
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                //get's azure table instance
                CloudTable UserBackendConfigurationTable = GetAzureTableInstance(tablename);
                TableBatchOperation batchOperation = new TableBatchOperation();
                //insert list of entities into batch operation
                foreach (T entity in entitieslist)
                {
                    entity.ETag = "*";
                    batchOperation.Add(TableOperation.Delete(entity));
                }
                UserBackendConfigurationTable.ExecuteBatch(batchOperation);                
            }
            catch (Exception exception)
            {
                //write exception into application insights
                InsightLogger.Exception(exception.Message + " - Error in DataProver while removing entity from " + tablename, exception, callerMethodName);
                throw new Exception();
            }
        }

        /// <summary>
        /// method to add message to queue
        /// </summary>
        /// <param name="queuename">takes queue name as input</param>
        /// <param name="message">takes message as input</param>
        public static void AddMessagetoQueue(string queuename, string message)
        {
            try
            {
                //get's azure queue instance    
                CloudQueue queue = GetAzureQueueInstance(queuename);
                //serialize object to string           
                CloudQueueMessage queuemessage = new CloudQueueMessage(message);
                //adds message to queue
                queue.AddMessage(queuemessage);
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - exception in dataprovider while adding message to queue " + queuename + " :- "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new Exception();
            }
        }
    }
}