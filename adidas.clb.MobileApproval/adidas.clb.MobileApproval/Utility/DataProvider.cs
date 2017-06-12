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
using Microsoft.WindowsAzure.Storage.Blob;
using adidas.clb.MobileApproval.Models;
using adidas.clb.MobileApproval.Exceptions;
namespace adidas.clb.MobileApproval.Utility
{
    /// The class that contains the methods realted to connecting azure table storage.
    public static class DataProvider
    {
        //Application insights interface reference for logging the error details into Application Insight azure service.
        static IAppInsight InsightLogger { get { return AppInsightLogger.Instance; } }
        /// <summary>
        /// method to get azure table storage object instance
        /// </summary>
        /// <param name="TableName">takes tablename as input</param>
        /// <returns>Azure Table Instance</returns>        
        public static CloudTable GetAzureTableInstance(string TableName)
        {
            try
            {
                // Retrieve the storage account from the connection string.
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting(CoreConstants.AzureTables.AzureStorageConnectionString));
                // Create the table client.            
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
                // set retry for the connection for transient failures
                tableClient.DefaultRequestOptions = new TableRequestOptions
                {
                    RetryPolicy = new LinearRetry(TimeSpan.FromSeconds(5), 3)
                };
                // Create the CloudTable object that represents the table.
                CloudTable table = tableClient.GetTableReference(TableName);
                return table;
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - exception in dataprovider while getting insatance of azure table " + TableName + " :- "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
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
        /// <typeparam name="T1">takes output type</typeparam>
        /// <typeparam name="T2">takes input type</typeparam>
        /// <param name="inputModel">takes input object</param>
        /// <returns>returns mapped object</returns>
        public static T1 ResponseObjectMapper<T1, T2>(T2 inputModel)
        {
            try
            {
                var entity = Activator.CreateInstance<T1>();
                var properties = inputModel.GetType().GetProperties();
                //loop each property and add to result object
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
                LoggerHelper.WriteToLog(exception + " - exception in dataprovider while mapping properties in object mapper:- "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new Exception();
            }
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
            try
            {
                var ResponseBackends = new PersonalizationResponseDTO<T>();
                ResponseBackends.result = default(T);
                //call errordto method to pass error in to response dto
                ResponseBackends.error = new ErrorDTO(code, shorttext, longtext);
                return ResponseBackends;
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - exception in dataprovider while instantiating personalizationresponse error:- "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
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
            try
            {
                //get's azure table instance
                CloudTable ReferenceDataTable = GetAzureTableInstance(tablename);
                //retrieve entity based on partitionkey and rowkey
                TableOperation RetrieveUser = TableOperation.Retrieve<T>(partitionkey, rowkey);
                TableResult RetrievedResultUser = ReferenceDataTable.Execute(RetrieveUser);

                return (T)RetrievedResultUser.Result;
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - exception in dataprovider while retrieving entity from table " + tablename + " :- "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new Exception();
            }
        }

        /// <summary>
        /// method to delete entity from azure table
        /// </summary>
        /// <typeparam name="T">Takes table entity type as input</typeparam>
        /// <param name="tablename">takes table name as input</param>
        /// <param name="partitionkey">takes partition key as input</param>
        /// <param name="rowkey">takes row key as input</param>
        /// <returns>returns entity</returns>
        public static T DeleteEntity<T>(string tablename, string partitionkey, string rowkey) where T : ITableEntity, new()
        {
            try
            {
                //get's azure table instance
                CloudTable ReferenceDataTable = GetAzureTableInstance(tablename);
                //retrieve entity based on partitionkey and rowkey
                TableOperation RetrieveUser = TableOperation.Retrieve<T>(partitionkey, rowkey);
                TableResult RetrievedResultUser = ReferenceDataTable.Execute(RetrieveUser);
                T deleteUserEntity = (T)RetrievedResultUser.Result;
                //delete retrieved user entity
                if (deleteUserEntity != null)
                {
                    //delete entity if it is exists
                    TableOperation deleteOperation = TableOperation.Delete(deleteUserEntity);
                    ReferenceDataTable.Execute(deleteOperation);
                }
                return deleteUserEntity;
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - exception in dataprovider while delete entity from table " + tablename + " :- "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
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
            try
            {
                //get's azure table instance
                CloudTable ReferenceDataTable = GetAzureTableInstance(tablename);
                //insert entity
                TableOperation insertOperation = TableOperation.Insert(entity);
                ReferenceDataTable.Execute(insertOperation);
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - exception in dataprovider while inserting entity to table " + tablename + " :- "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
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
            try
            {
                //get's azure table instance
                CloudTable ReferenceDataTable = GetAzureTableInstance(tablename);
                //insert if not exists otherwise replace
                TableOperation insertreplaceOperation = TableOperation.InsertOrReplace(entity);
                ReferenceDataTable.Execute(insertreplaceOperation);
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - exception in dataprovider while insert or replace entity to table " + tablename + " :- "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
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
            try
            {
                //get's azure table instance
                CloudTable ReferenceDataTable = GetAzureTableInstance(tablename);
                entity.ETag = "*";
                //replace entity
                TableOperation updateOperation = TableOperation.Replace(entity);
                ReferenceDataTable.Execute(updateOperation);
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - exception in dataprovider while update entity to table " + tablename + " :- "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
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

        /// <summary>
        /// method to add multiple entities to azure table
        /// </summary>
        /// <typeparam name="T">Takes table entity type as input</typeparam>
        /// <param name="tablename">takes table name as input</param>
        /// <param name="entitieslist">takes entities list as input</param>
        public static void AddEntities<T>(string tablename, List<T> entitieslist) where T : ITableEntity, new()
        {
            try
            {
                //get's azure table instance
                CloudTable UserBackendConfigurationTable = GetAzureTableInstance(tablename);
                TableBatchOperation batchOperation = new TableBatchOperation();
                //insert list of entities into batch operation
                foreach (T entity in entitieslist)
                {
                    batchOperation.Insert(entity);
                }
                UserBackendConfigurationTable.ExecuteBatch(batchOperation);
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - exception in dataprovider while adding entities to table " + tablename + " :- "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
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
            try
            {
                //get's azure table instance
                CloudTable UserBackendConfigurationTable = GetAzureTableInstance(tablename);
                TableBatchOperation batchOperation = new TableBatchOperation();
                //insert list of entities into batch operation
                foreach (T entity in entitieslist)
                {
                    batchOperation.Add(TableOperation.Delete(entity));
                }
                UserBackendConfigurationTable.ExecuteBatch(batchOperation);
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - exception in dataprovider while removing entities from table " + tablename + " :- "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
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
            try
            {
                //get's azure table instance
                CloudTable UserDeviceConfigurationTable = DataProvider.GetAzureTableInstance(tablename);
                //TableQuery<BackendEntity> query = new TableQuery<BackendEntity>().Where(TableQuery.GenerateFilterCondition(CoreConstants.AzureTables.PartitionKey, QueryComparisons.Equal, CoreConstants.AzureTables.Backend));
                List<T> allentities = UserDeviceConfigurationTable.ExecuteQuery(query).ToList();
                return allentities;
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - exception in dataprovider while getting entities list from table " + tablename + " :- "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new Exception();
            }
        }

        /// <summary>
        /// method to form error response
        /// </summary>
        /// <typeparam name="T">takes type as input</typeparam>
        /// <param name="code">takes code as input</param>
        /// <param name="shorttext">takes short text as input</param>
        /// <param name="longtext">takes long text as input</param>
        /// <returns>returns error response</returns>
        public static SynchResponseItemDTO<T> SynchResponseError<T>(string code, string shorttext, string longtext)
        {
            try
            {
                var ResponseBackends = new SynchResponseItemDTO<T>();
                ResponseBackends.result = default(T);
                //call errordto to add error to response dto
                ResponseBackends.error = new ErrorDTO(code, shorttext, longtext);
                return ResponseBackends;
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - exception in dataprovider while instantiating synch response error object:- "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new Exception();
            }
        }

        /// <summary>
        /// This method returns ApprovalResponseError
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="code"></param>
        /// <param name="shorttext"></param>
        /// <param name="longtext"></param>
        /// <returns></returns>
        public static ApprovalResponseDTO<T> ApprovalResponseError<T>(string code, string shorttext, string longtext)
        {
            try
            {
                var ApprovalResponse = new ApprovalResponseDTO<T>();
                ApprovalResponse.result = default(T);
                ApprovalResponse.error = new ErrorDTO(code, shorttext, longtext);
                return ApprovalResponse;
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - exception in dataprovider while instantiating approval response error object:- "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new Exception();
            }
        }

        /// <summary>
        /// method to get azure table storage object instance
        /// </summary>
        /// <param name="TableName">takes tablename as input</param>
        /// <returns>Azure Table Instance</returns>        
        public static Uri GetBlobSASUri(string BlobUri)
        {
            try
            {
                // Retrieve the storage account from the connection string.
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting(CoreConstants.AzureTables.AzureStorageConnectionString));

                var readPolicy = new SharedAccessBlobPolicy()
                {
                    Permissions = SharedAccessBlobPermissions.Read,
                    SharedAccessExpiryTime = DateTime.UtcNow + TimeSpan.FromMinutes(5)
                };
                CloudBlockBlob blob = new CloudBlockBlob(new Uri(BlobUri), storageAccount.Credentials);
                Uri saspdfuri = new Uri(blob.Uri.AbsoluteUri + blob.GetSharedAccessSignature(readPolicy));

                return saspdfuri;
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - exception in dataprovider while getting SAS Uri for Blob :- "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new Exception();
            }
        }
        /// <summary>
        /// This method retrieves the azure table entity information
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tablename"></param>
        /// <param name="partitionkey"></param>
        /// <param name="rowkey"></param>
        /// <returns></returns>
        public static T RetrieveEntity<T>(string tablename, string partitionkey, string rowkey) where T : ITableEntity, new()
        {
            try
            {
                TableResult RetrievedResultUser = null;

                //get's azure table instance
                CloudTable ReferenceDataTable = GetAzureTableInstance(tablename);
                // Create a retrieve operation that takes a T entity.
                TableOperation RetrieveUser = TableOperation.Retrieve<T>(partitionkey, rowkey);
                // Execute the operation.
                RetrievedResultUser = ReferenceDataTable.Execute(RetrieveUser);
                return (T)RetrievedResultUser.Result;

            }

            catch (Exception innerexception)
            {
                InsightLogger.Exception(innerexception.Message, innerexception, "Retrieveentity");
                throw new DataAccessException(innerexception.Message, innerexception.InnerException);
            }

        }
    }
}

