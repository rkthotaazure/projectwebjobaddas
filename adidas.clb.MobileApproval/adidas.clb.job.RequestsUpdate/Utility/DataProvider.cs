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
using Microsoft.WindowsAzure.Storage.Blob;

namespace adidas.clb.job.RequestsUpdate.Utility
{
    /// The class that contains the methods realted to connecting azure table storage.
    public static class DataProvider
    {
        /// <summary>
        /// method to get azure table storage object instance
        /// </summary>
        /// <param name="TableName">takes table name as input</param>
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
        ///  method to get azure storage blob container  object instance
        /// </summary>
        /// <param name="Blobname">takes blob name as input</param>
        /// <returns>returns blob container instance</returns>
        public static CloudBlobContainer GetAzureBlobContainerInstance(string Blobname)
        {
            // Retrieve the storage account from the connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting(CoreConstants.AzureTables.AzureStorageConnectionString));
            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            // Create the Cloudblob container object that represents the blob.
            CloudBlobContainer container = blobClient.GetContainerReference(Blobname);
            return container;
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
            TableOperation RetrieveUser = TableOperation.Retrieve<T>(partitionkey, rowkey);
            TableResult RetrievedResultUser = ReferenceDataTable.Execute(RetrieveUser);
            return (T)RetrievedResultUser.Result;
        }

        /// <summary>
        /// method to insert entity to azure table
        /// </summary>
        /// <typeparam name="T">Takes table entity type as input</typeparam>
        /// <param name="tablename">takes table name as input</param>
        /// <param name="entity">takes entity as input to insert</param>
        public static void InsertEntity<T>(string tablename, T entity) where T : ITableEntity, new()
        {
            CloudTable ReferenceDataTable = GetAzureTableInstance(tablename);
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
            CloudTable ReferenceDataTable = GetAzureTableInstance(tablename);
            TableOperation insertreplaceOperation = TableOperation.InsertOrReplace(entity);
            ReferenceDataTable.Execute(insertreplaceOperation);
        }

        /// <summary>
        /// method to add multiple entities to azure table
        /// </summary>
        /// <typeparam name="T">Takes table entity type as input</typeparam>
        /// <param name="tablename">takes table name as input</param>
        /// <param name="entitieslist">takes entities list as input</param>
        public static void AddEntities<T>(string tablename, List<T> entitieslist) where T : ITableEntity, new()
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

        /// <summary>
        /// method to update entity to azure table
        /// </summary>
        /// <typeparam name="T">Takes table entity type as input</typeparam>
        /// <param name="tablename">takes table name as input</param>
        /// <param name="entity">takes entity as input to insert</param>
        public static void UpdateEntity<T>(string tablename, T entity) where T : ITableEntity, new()
        {
            CloudTable ReferenceDataTable = GetAzureTableInstance(tablename);
            entity.ETag = "*";
            TableOperation updateOperation = TableOperation.Replace(entity);
            ReferenceDataTable.Execute(updateOperation);
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
            //get's azure table instance
            CloudTable UserDeviceConfigurationTable = DataProvider.GetAzureTableInstance(tablename);
            //TableQuery<BackendEntity> query = new TableQuery<BackendEntity>().Where(TableQuery.GenerateFilterCondition(CoreConstants.AzureTables.PartitionKey, QueryComparisons.Equal, CoreConstants.AzureTables.Backend));
            List<T> allentities = UserDeviceConfigurationTable.ExecuteQuery(query).ToList();
            return allentities;
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

    }
}