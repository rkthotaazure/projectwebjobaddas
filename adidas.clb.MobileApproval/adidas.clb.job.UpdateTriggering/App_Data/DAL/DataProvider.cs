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
using System.Threading;
using System.Web;
using System.Threading.Tasks;
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
            try
            {
                TableResult RetrievedResultUser = null;
                //Max Retry call from web.config
                int maxRetryCount = Convert.ToInt32(ConfigurationManager.AppSettings["MaxRetryCount"]);
                int RetryAttemptCount = 0;
                bool IsSuccessful = false;
                do
                {
                    try
                    {
                        //get's azure table instance
                        CloudTable ReferenceDataTable = GetAzureTableInstance(tablename);
                        TableOperation RetrieveUser = TableOperation.Retrieve<T>(partitionkey, rowkey);
                        RetrievedResultUser = ReferenceDataTable.Execute(RetrieveUser);
                        IsSuccessful = true;
                        return (T)RetrievedResultUser.Result;


                    }
                    catch (StorageException storageException)
                    {
                        //Increasing RetryAttemptCount variable
                        RetryAttemptCount = RetryAttemptCount + 1;
                        //Checking retry call count is eual to max retry count or not
                        if (RetryAttemptCount == maxRetryCount)
                        {
                            InsightLogger.Exception("Error in DataProvider:: Retrieveentity() method :: Retry attempt count: [ " + RetryAttemptCount + " ]", storageException, "Retrieveentity");
                            IsSuccessful = true;
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
                int maxRetryCount = Convert.ToInt32(ConfigurationManager.AppSettings["MaxRetryCount"]);
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


                    }
                    catch (StorageException storageException)
                    {
                        //Increasing RetryAttemptCount variable
                        RetryAttemptCount = RetryAttemptCount + 1;
                        //Checking retry call count is eual to max retry count or not
                        if (RetryAttemptCount == maxRetryCount)
                        {
                            InsightLogger.Exception("Error in DataProvider:: UpdateEntity() method :: Retry attempt count: [ " + RetryAttemptCount + " ]", storageException, "UpdateEntity");
                            IsSuccessful = true;
                            throw new DataAccessException(storageException.Message, storageException.InnerException);

                        }
                        else
                        {
                            InsightLogger.Exception("Error in DataProvider:: UpdateEntity() method :: Retry attempt count: [ " + RetryAttemptCount + " ]", storageException, "UpdateEntity");
                            //Putting the thread into some milliseconds sleep  and again call the same method call.
                            Thread.Sleep(Convert.ToInt32(ConfigurationManager.AppSettings["MaxThreadSleepInMilliSeconds"]));
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

                //Max Retry call from web.config
                int maxRetryCount = Convert.ToInt32(ConfigurationManager.AppSettings["MaxRetryCount"]);
                int RetryAttemptCount = 0;
                bool IsSuccessful = false;
                do
                {
                    try
                    {
                        CloudTable ReferenceDataTable = GetAzureTableInstance(tablename);
                        TableOperation InsertOperation = TableOperation.Insert(entity);
                        ReferenceDataTable.Execute(InsertOperation);


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
                            Thread.Sleep(Convert.ToInt32(ConfigurationManager.AppSettings["MaxThreadSleepInMilliSeconds"]));
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