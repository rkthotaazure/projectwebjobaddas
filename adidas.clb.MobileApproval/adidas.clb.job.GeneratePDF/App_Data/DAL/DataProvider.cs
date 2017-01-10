//-----------------------------------------------------------
// <copyright file="DataProvider.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using adidas.clb.job.GeneratePDF.Models;
using adidas.clb.job.GeneratePDF.Exceptions;
using adidas.clb.job.GeneratePDF.Utility;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Threading;
namespace adidas.clb.job.GeneratePDF.App_Data.DAL
{
    /// The class that contains the methods realted to connecting azure table storage.
    public static class DataProvider
    {
        //Application insights interface reference for logging the error details into Application Insight azure service.
        static IAppInsight InsightLogger { get { return AppInsightLogger.Instance; } }
        //getting Max Retry count,MaxThreadSleepInMilliSeconds from web.config
        public static int maxThreadSleepInMilliSeconds = Convert.ToInt32(ConfigurationManager.AppSettings["MaxThreadSleepInMilliSeconds"]);
        public static int maxRetryCount = Convert.ToInt32(ConfigurationManager.AppSettings["MaxRetryCount"]);
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
                CloudTable table = null;
                //Max Retry call from web.config               
                int RetryAttemptCount = 0;
                bool IsSuccessful = false;
                do
                {
                    try
                    {
                        //Get Caller Method name from CallerInformation class
                        callerMethodName = CallerInformation.TrackCallerMethodName();
                        // Retrieve the storage account from the connection string.
                        CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["GenericMobileStorageConnectionString"]);
                        // Create the table client.
                        CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
                        // Create the CloudTable object that represents the table.
                        table = tableClient.GetTableReference(TableName);
                        IsSuccessful = true;
                    }
                    catch (StorageException storageException)
                    {
                        //Increasing RetryAttemptCount variable
                        RetryAttemptCount = RetryAttemptCount + 1;
                        //Checking retry call count is eual to max retry count or not
                        if (RetryAttemptCount == maxRetryCount)
                        {
                            InsightLogger.Exception("Error in DataProvider:: Retrieveentity() method :: Retry attempt count: [ " + RetryAttemptCount + " ]", storageException, "Retrieveentity");
                            throw new DataAccessException(storageException.Message, storageException.InnerException);
                        }
                        else
                        {
                            InsightLogger.Exception("Error in DataProvider:: Retrieveentity() method :: Retry attempt count: [ " + RetryAttemptCount + " ]", storageException, "Retrieveentity");
                            //Putting the thread into some milliseconds sleep  and again call the same method call.
                            Thread.Sleep(Convert.ToInt32(ConfigurationManager.AppSettings["MaxThreadSleepInMilliSeconds"]));
                        }
                    }
                } while (!IsSuccessful);
                return table;

            }

            catch (Exception innerexception)
            {
                InsightLogger.Exception(innerexception.Message, innerexception, "Retrieveentity");
                throw new DataAccessException(innerexception.Message, innerexception.InnerException);
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
                int RetryAttemptCount = 0;
                bool IsSuccessful = false;
                do
                {
                    try
                    {
                        //get's azure table instance
                        CloudTable ReferenceDataTable = GetAzureTableInstance(tablename);
                        // Create a retrieve operation that takes a T entity.
                        TableOperation RetrieveUser = TableOperation.Retrieve<T>(partitionkey, rowkey);
                        // Execute the operation.
                        RetrievedResultUser = ReferenceDataTable.Execute(RetrieveUser);
                        IsSuccessful = true;



                    }
                    catch (StorageException storageException)
                    {
                        //Increasing RetryAttemptCount variable
                        RetryAttemptCount = RetryAttemptCount + 1;
                        //Checking retry call count is eual to max retry count or not
                        if (RetryAttemptCount == maxRetryCount)
                        {
                            InsightLogger.Exception("Error in DataProvider:: Retrieveentity() method :: Retry attempt count: [ " + RetryAttemptCount + " ]", storageException, "Retrieveentity");
                            throw new DataAccessException(storageException.Message, storageException.InnerException);

                        }
                        else
                        {
                            InsightLogger.Exception("Error in DataProvider:: Retrieveentity() method :: Retry attempt count: [ " + RetryAttemptCount + " ]", storageException, "Retrieveentity");
                            //Putting the thread into some milliseconds sleep  and again call the same method call.
                            Thread.Sleep(Convert.ToInt32(ConfigurationManager.AppSettings["MaxThreadSleepInMilliSeconds"]));
                        }
                    }
                } while (!IsSuccessful);
                return (T)RetrievedResultUser.Result;

            }

            catch (Exception innerexception)
            {
                InsightLogger.Exception(innerexception.Message, innerexception, "Retrieveentity");
                throw new DataAccessException(innerexception.Message, innerexception.InnerException);
            }

        }

        /// <summary>
        /// This method returns List of Entites based on Partitionkey and rowkey
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tablename"></param>
        /// <param name="partitionkey"></param>
        /// <param name="rowkey"></param>
        /// <returns></returns>
        public static List<T> RetrieveEntities<T>(string tablename, string partitionkey) where T : ITableEntity, new()
        {
            try
            {
                List<T> entitesList = null;
                int RetryAttemptCount = 0;
                bool IsSuccessful = false;
                do
                {
                    try
                    {
                        //get's azure table instance                      
                        CloudTable ReferenceDataTable = GetAzureTableInstance(tablename);
                        TableQuery<T> tquery = new TableQuery<T>().Where(TableQuery.GenerateFilterCondition(CoreConstants.AzureTables.PartitionKey, QueryComparisons.Equal, partitionkey));
                        entitesList = ReferenceDataTable.ExecuteQuery(tquery).ToList();
                        IsSuccessful = true;

                    }
                    catch (StorageException storageException)
                    {
                        //Increasing RetryAttemptCount variable
                        RetryAttemptCount = RetryAttemptCount + 1;
                        //Checking retry call count is eual to max retry count or not
                        if (RetryAttemptCount == maxRetryCount)
                        {
                            InsightLogger.Exception("Error in DataProvider:: Retrieveentity() method :: Retry attempt count: [ " + RetryAttemptCount + " ]", storageException, "Retrieveentity");
                            throw new DataAccessException(storageException.Message, storageException.InnerException);

                        }
                        else
                        {
                            InsightLogger.Exception("Error in DataProvider:: Retrieveentity() method :: Retry attempt count: [ " + RetryAttemptCount + " ]", storageException, "Retrieveentity");
                            //Putting the thread into some milliseconds sleep  and again call the same method call.
                            Thread.Sleep(Convert.ToInt32(ConfigurationManager.AppSettings["MaxThreadSleepInMilliSeconds"]));
                        }
                    }
                } while (!IsSuccessful);
                return entitesList;

            }

            catch (Exception innerexception)
            {
                InsightLogger.Exception(innerexception.Message, innerexception, "Retrieveentity");
                throw new DataAccessException(innerexception.Message, innerexception.InnerException);
            }

        }
        /// <summary>
        /// This method updates the azure table entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tablename"></param>
        /// <param name="entity"></param>
        public static void UpdateEntity<T>(string tablename, T entity) where T : ITableEntity, new()
        {
            try
            {

                //Max Retry call from web.config             
                int RetryAttemptCount = 0;
                bool IsSuccessful = false;
                do
                {
                    try
                    {
                        CloudTable ReferenceDataTable = GetAzureTableInstance(tablename);
                        entity.ETag = "*";
                        TableOperation updateOperation = TableOperation.Replace(entity);
                        ReferenceDataTable.Execute(updateOperation);
                        IsSuccessful = true;

                    }
                    catch (StorageException storageException)
                    {
                        //Increasing RetryAttemptCount variable
                        RetryAttemptCount = RetryAttemptCount + 1;
                        //Checking retry call count is eual to max retry count or not
                        if (RetryAttemptCount == maxRetryCount)
                        {
                            InsightLogger.Exception("Error in DataProvider:: UpdateEntity() method :: Retry attempt count: [ " + RetryAttemptCount + " ]", storageException, "UpdateEntity");
                            throw new DataAccessException(storageException.Message, storageException.InnerException);

                        }
                        else
                        {
                            InsightLogger.Exception("Error in DataProvider:: UpdateEntity() method :: Retry attempt count: [ " + RetryAttemptCount + " ]", storageException, "UpdateEntity");
                            //Putting the thread into some milliseconds sleep  and again call the same method call.
                            Thread.Sleep(maxThreadSleepInMilliSeconds);
                        }
                    }
                } while (!IsSuccessful);


            }

            catch (Exception innerexception)
            {
                InsightLogger.Exception(innerexception.Message, innerexception, "UpdateEntity");
                throw new DataAccessException(innerexception.Message, innerexception.InnerException);
            }


        }
        /// <summary>
        /// This method Inserts thenew entity in azure table
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tablename"></param>
        /// <param name="entity"></param>
        public static void InsertEntity<T>(string tablename, T entity) where T : ITableEntity, new()
        {

            try
            {
                int RetryAttemptCount = 0;
                bool IsSuccessful = false;
                do
                {
                    try
                    {
                        CloudTable ReferenceDataTable = GetAzureTableInstance(tablename);
                        TableOperation InsertOperation = TableOperation.Insert(entity);
                        ReferenceDataTable.Execute(InsertOperation);
                        IsSuccessful = true;

                    }
                    catch (StorageException storageException)
                    {
                        //Increasing RetryAttemptCount variable
                        RetryAttemptCount = RetryAttemptCount + 1;
                        //Checking retry call count is eual to max retry count or not
                        if (RetryAttemptCount == maxRetryCount)
                        {
                            InsightLogger.Exception("Error in DataProvider:: InsertEntity() method :: Retry attempt count: [ " + RetryAttemptCount + " ]", storageException, "InsertEntity");
                            IsSuccessful = true;
                            throw new DataAccessException(storageException.Message, storageException.InnerException);

                        }
                        else
                        {
                            InsightLogger.Exception("Error in DataProvider:: InsertEntity() method :: Retry attempt count: [ " + RetryAttemptCount + " ]", storageException, "InsertEntity");
                            //Putting the thread into some milliseconds sleep  and again call the same method call.
                            Thread.Sleep(maxThreadSleepInMilliSeconds);
                        }
                    }
                } while (!IsSuccessful);


            }

            catch (Exception innerexception)
            {
                InsightLogger.Exception(innerexception.Message, innerexception, "InsertEntity");
                throw new DataAccessException(innerexception.Message, innerexception.InnerException);
            }


        }

    }
}