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
        public static CloudTable GetAzureTableInstance(string TableName)
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
        public static CloudQueue GetAzureQueueInstance(string QueueName)
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
            //loop each property and add to result object
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
            //call errordto method to pass error in to response dto
            ResponseBackends.error = new ErrorDTO(code, shorttext, longtext);
            return ResponseBackends;
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
            //get's azure table instance
            CloudTable ReferenceDataTable = GetAzureTableInstance(tablename);
            //retrieve entity based on partitionkey and rowkey
            TableOperation RetrieveUser = TableOperation.Retrieve<T>(partitionkey, rowkey);
            TableResult RetrievedResultUser = ReferenceDataTable.Execute(RetrieveUser);
            return (T)RetrievedResultUser.Result;
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

        /// <summary>
        /// method to insert entity to azure table
        /// </summary>
        /// <typeparam name="T">Takes table entity type as input</typeparam>
        /// <param name="tablename">takes table name as input</param>
        /// <param name="entity">takes entity as input to insert</param>
        public static void InsertEntity<T>(string tablename, T entity) where T : ITableEntity, new()
        {
            //get's azure table instance
            CloudTable ReferenceDataTable = GetAzureTableInstance(tablename);
            //insert entity
            TableOperation insertOperation = TableOperation.Insert(entity);
            ReferenceDataTable.Execute(insertOperation);
        }

        /// <summary>
        /// method to insertor replace entity to azure table
        /// </summary>
        /// <typeparam name="T">Takes table entity type as input</typeparam>
        /// <param name="tablename">takes table name as input</param>
        /// <param name="entity">takes entity as input to insert</param>
        public static void InsertReplaceEntity<T>(string tablename, T entity) where T : ITableEntity, new()
        {
            //get's azure table instance
            CloudTable ReferenceDataTable = GetAzureTableInstance(tablename);
            //insert if not exists otherwise replace
            TableOperation insertreplaceOperation = TableOperation.InsertOrReplace(entity);
            ReferenceDataTable.Execute(insertreplaceOperation);
        }

        /// <summary>
        /// method to update entity to azure table
        /// </summary>
        /// <typeparam name="T">Takes table entity type as input</typeparam>
        /// <param name="tablename">takes table name as input</param>
        /// <param name="entity">takes entity as input to insert</param>
        public static void UpdateEntity<T>(string tablename, T entity) where T : ITableEntity, new()
        {
            //get's azure table instance
            CloudTable ReferenceDataTable = GetAzureTableInstance(tablename);
            entity.ETag = "*";
            //replace entity
            TableOperation updateOperation = TableOperation.Replace(entity);
            ReferenceDataTable.Execute(updateOperation);            
        }

        /// <summary>
        /// method to add message to queue
        /// </summary>
        /// <param name="queuename">takes queue name as input</param>
        /// <param name="message">takes message as input</param>
        public static void AddMessagetoQueue(string queuename, string message)
        {
            //get's azure queue instance    
            CloudQueue queue =GetAzureQueueInstance(queuename);
            //serialize object to string           
            CloudQueueMessage queuemessage = new CloudQueueMessage(message);
            //adds message to queue
            queue.AddMessage(queuemessage);
        }

        /// <summary>
        /// method to add multiple entities to azure table
        /// </summary>
        /// <typeparam name="T">Takes table entity type as input</typeparam>
        /// <param name="tablename">takes table name as input</param>
        /// <param name="entitieslist">takes entities list as input</param>
        public static void AddEntities<T>(string tablename,List<T> entitieslist) where T : ITableEntity, new()
        {
            //get's azure table instance
            CloudTable UserBackendConfigurationTable =GetAzureTableInstance(tablename);
            TableBatchOperation batchOperation = new TableBatchOperation();
            //insert list of entities into batch operation
            foreach (T entity in entitieslist)
            {
                batchOperation.Insert(entity);
            }
            UserBackendConfigurationTable.ExecuteBatch(batchOperation);
        }

        /// <summary>
        /// method to delete multiple entities from azure table
        /// </summary>
        /// <typeparam name="T">Takes table entity type as input</typeparam>
        /// <param name="tablename">takes table name as input</param>
        /// <param name="entitieslist">takes entities list as input</param>
        public static void RemoveEntities<T>(string tablename, List<T> entitieslist) where T : ITableEntity, new()
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

        /// <summary>
        /// method to get entities list from azure table
        /// </summary>
        /// <typeparam name="T">Takes table entity type as input</typeparam>
        /// <param name="tablename">takes table name as input</param>
        /// <param name="query">takes query as input</param>
        /// <returns>returns entities list</returns>
        public static List<T> GetEntitiesList<T>(string tablename,TableQuery<T> query) where T : ITableEntity, new()
        {
            //get's azure table instance
            CloudTable UserDeviceConfigurationTable = DataProvider.GetAzureTableInstance(tablename);
            //TableQuery<BackendEntity> query = new TableQuery<BackendEntity>().Where(TableQuery.GenerateFilterCondition(CoreConstants.AzureTables.PartitionKey, QueryComparisons.Equal, CoreConstants.AzureTables.Backend));
            List<T> allentities = UserDeviceConfigurationTable.ExecuteQuery(query).ToList();
            return allentities;
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
            var ResponseBackends = new SynchResponseItemDTO<T>();
            ResponseBackends.result = default(T);
            //call errordto to add error to response dto
            ResponseBackends.error = new ErrorDTO(code, shorttext, longtext);
            return ResponseBackends;
        }
    }
}

