//-----------------------------------------------------------
// <copyright file="synchDAL.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Queue;
using adidas.clb.MobileApproval.Exceptions;
using adidas.clb.MobileApproval.Models;
using adidas.clb.MobileApproval.Utility;
using System.Configuration;
namespace adidas.clb.MobileApproval.App_Code.DAL.Synch
{
    /// <summary>
    /// The class which implements methods for data access layer of synch.
    /// </summary> 
    public class SynchDAL
    {
        //Application insights interface reference for logging the error details into Application Insight azure service.
        static IAppInsight InsightLogger { get { return AppInsightLogger.Instance; } }
        public static int maxThreadSleepInMilliSeconds = Convert.ToInt32(ConfigurationManager.AppSettings["MaxThreadSleepInMilliSeconds"]);
        public static int maxRetryCount = Convert.ToInt32(ConfigurationManager.AppSettings["MaxRetryCount"]);
        /// <summary>
        /// method to get all requests per user
        /// </summary>
        /// <param name="UserID">takes userid as input</param>
        /// <returns>returns list of requests associated to user</returns>
        public List<RequestEntity> GetUserRequests(string UserID, string requeststatus)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                //partionkey filter
                string partitionkeyfilter = TableQuery.GenerateFilterCondition(CoreConstants.AzureTables.PartitionKey, QueryComparisons.Equal, string.Concat(CoreConstants.AzureTables.RequestsPK, UserID));
                //request status filter
                string statusfilter = TableQuery.GenerateFilterCondition(CoreConstants.AzureTables.Status, QueryComparisons.Equal, requeststatus);
                //generate query to get all requests per user based on filter conditions
                TableQuery<RequestEntity> query = new TableQuery<RequestEntity>().Where(partitionkeyfilter);
                //call dataprovider method to get entities from azure table
                List<RequestEntity> allrequests = DataProvider.GetEntitiesList<RequestEntity>(CoreConstants.AzureTables.RequestTransactions, query);
                return allrequests;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                //LoggerHelper.WriteToLog(exception + " - Error while retrieving requests per user from RequestTransactions azure table in DAL : "
                //+ exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new DataAccessException();
            }
        }

        /// <summary>
        /// method to get requests per userbackend
        /// </summary>
        /// <param name="UserID">takes userid as input</param>
        /// <param name="backendID">takes backendid as input</param>
        /// <returns>returns list of requets associated userbackend</returns>
        public List<RequestEntity> GetUserBackendRequests(string UserID, string backendID, string requeststatus)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                string partitionkeyfilter = TableQuery.GenerateFilterCondition(CoreConstants.AzureTables.PartitionKey, QueryComparisons.Equal, string.Concat(CoreConstants.AzureTables.RequestsPK, UserID));
                string backendfilter = TableQuery.GenerateFilterCondition(CoreConstants.AzureTables.BackendId, QueryComparisons.Equal, backendID);
                //combine partionkey filter with backend filter 
                string combinefilter = TableQuery.CombineFilters(partitionkeyfilter, TableOperators.And, backendfilter);
                //request status filter
                string statusfilter = TableQuery.GenerateFilterCondition(CoreConstants.AzureTables.Status, QueryComparisons.Equal, requeststatus);
                //final filter to get requests based on partitionkey, backendid, request status
                string finalfilter = TableQuery.CombineFilters(combinefilter, TableOperators.And, statusfilter);
                //generate query to get all user associated requests
                TableQuery<RequestEntity> query = new TableQuery<RequestEntity>().Where(combinefilter);
                //call dataprovider method to get entities from azure table
                List<RequestEntity> allrequests = DataProvider.GetEntitiesList<RequestEntity>(CoreConstants.AzureTables.RequestTransactions, query);
                return allrequests;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                //LoggerHelper.WriteToLog(exception + " - Error while retrieving requests per userbackend from RequestTransactions azure table in DAL : "
                //+ exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new DataAccessException();
            }
        }

        /// <summary>
        /// method to get request
        /// </summary>
        /// <param name="requestID">takes requestid as input</param>
        /// <returns>returns request</returns>
        public RequestEntity GetRequest(string userID, string requestID)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                //call dataprovider method to get entities from azure table
                RequestEntity request = DataProvider.Retrieveentity<RequestEntity>(CoreConstants.AzureTables.RequestTransactions, string.Concat(CoreConstants.AzureTables.RequestsPK, userID), requestID);
                return request;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                //LoggerHelper.WriteToLog(exception + " - Error while retrieving request from RequestTransactions azure table in DAL : "
                //+ exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new DataAccessException();
            }
        }

        /// <summary>
        /// method to get SAS PDFUri
        /// </summary>
        /// <param name="PDFUri">takes PDFUri as input</param>
        /// <returns>returns SAS PDFUri</returns>
        public Uri GetSASPdfUri(string PDFUri)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                //call dataprovider method to get sas blob uri                
                return DataProvider.GetBlobSASUri(PDFUri);
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                //LoggerHelper.WriteToLog(exception + " - Error while retrieving shared access service pdf uri in DAL : "
                //+ exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new DataAccessException();
            }
        }

        /// <summary>
        /// method to get approvers per request
        /// </summary>
        /// <param name="requestID">takes requestid as input</param>
        /// <returns>returns list of approvers per request</returns>
        public List<ApproverEntity> GetApprovers(string requestID)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                //generate query to get all approvers associated request
                TableQuery<ApproverEntity> query = new TableQuery<ApproverEntity>().Where(TableQuery.GenerateFilterCondition(CoreConstants.AzureTables.PartitionKey, QueryComparisons.Equal, string.Concat(CoreConstants.AzureTables.ApproverPK, requestID)));
                //call dataprovider method to get entities from azure table
                List<ApproverEntity> approvers = DataProvider.GetEntitiesList<ApproverEntity>(CoreConstants.AzureTables.RequestTransactions, query);
                return approvers;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                //LoggerHelper.WriteToLog(exception + " - Error while retrieving approvers from RequestTransactions azure table in DAL : "
                //+ exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new DataAccessException();
            }
        }

        /// <summary>
        /// method to get fields associated to request
        /// </summary>
        /// <param name="requestID">takes requestid as input</param>
        /// <returns>returns list of fields per request</returns>
        public List<FieldEntity> GetFields(string requestID)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                //generate query to get all fields associated request
                TableQuery<FieldEntity> query = new TableQuery<FieldEntity>().Where(TableQuery.GenerateFilterCondition(CoreConstants.AzureTables.PartitionKey, QueryComparisons.Equal, string.Concat(CoreConstants.AzureTables.FieldPK, requestID)));
                //call dataprovider method to get entities from azure table
                List<FieldEntity> fields = DataProvider.GetEntitiesList<FieldEntity>(CoreConstants.AzureTables.RequestTransactions, query);
                return fields;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                //LoggerHelper.WriteToLog(exception + " - Error while retrieving fields from RequestTransactions azure table in DAL : "
                //+ exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new DataAccessException();
            }
        }

        /// <summary>
        /// method to get user backend synch
        /// </summary>
        /// <param name="UserID">takes user id as input</param>
        /// /// <param name="UserBackendID">takes user backend id as input</param>
        /// <returns>returns backend synch entity for user</returns>
        public SynchEntity GetUserBackendSynch(string UserID, string UserBackendID)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                //call dataprovider method to get entities from azure table
                SynchEntity synchentity = DataProvider.Retrieveentity<SynchEntity>(CoreConstants.AzureTables.UserDeviceConfiguration, (string.Concat(CoreConstants.AzureTables.BackendSynchPK, UserID)), (string.Concat(CoreConstants.AzureTables.BackendSynchPK, UserBackendID)));
                return synchentity;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                //LoggerHelper.WriteToLog(exception + " - Error while retrieving userbackendsynch from userdeviceconfig azure table in DAL : "
                //+ exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new DataAccessException();
            }
        }

        /// <summary>
        /// method to get request synch
        /// </summary>
        /// <param name="requestsynchpk">takes request synch partitionkey as input</param>
        /// /// <param name="requestid">takes request id as input</param>
        /// <returns>returns backend synch entity for user</returns>
        public RequestSynchEntity GetRequestSynch(string requestsynchpk, string requestid)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                //call dataprovider method to get entities from azure table
                RequestSynchEntity synchentity = DataProvider.Retrieveentity<RequestSynchEntity>(CoreConstants.AzureTables.UserDeviceConfiguration, requestsynchpk, requestid);
                return synchentity;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                //LoggerHelper.WriteToLog(exception + " - Error while retrieving request synch from userdeviceconfig azure table in DAL : "
                //+ exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new DataAccessException();
            }
        }

        /// <summary>
        /// method to add/upadte Request entity to azure table
        /// </summary>
        /// <param name="synch">takes userbackend synch entity as input</param>
        public void AddUpdateBackendSynch(SynchEntity synch)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                //call dataprovider method to insert entity into azure table
                DataProvider.InsertReplaceEntity<SynchEntity>(CoreConstants.AzureTables.UserDeviceConfiguration, synch);
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                //LoggerHelper.WriteToLog(exception + " - Error while inserting userbackend synch into requestTransactions azure table in DAL : "
                //+ exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new DataAccessException();
            }
        }

        /// <summary>
        /// method to add/upadte Request synch entity to azure table
        /// </summary>
        /// <param name="synch">takes request synch entity as input</param>
        public void AddUpdateRequestSynch(RequestSynchEntity synch)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                //call dataprovider method to insert entity into azure table
                DataProvider.InsertReplaceEntity<RequestSynchEntity>(CoreConstants.AzureTables.UserDeviceConfiguration, synch);
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                //LoggerHelper.WriteToLog(exception + " - Error while inserting request synch into requestTransactions azure table in DAL : "
                //+ exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new DataAccessException();
            }
        }

        /// <summary>
        /// method to get userbackends
        /// </summary>
        /// <param name="UserID">takes userid as input</param>
        /// <returns> returns list of user backends associated to user</returns>
        public List<UserBackendEntity> GetUserAllBackends(string UserID, List<string> userbackendidslist)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                string finalfilter = string.Empty;
                //partionkey filter
                string partitionkeyfilter = TableQuery.GenerateFilterCondition(CoreConstants.AzureTables.PartitionKey, QueryComparisons.Equal, string.Concat(CoreConstants.AzureTables.UserBackendPK, UserID));
                //loop through each userbackend to generate rowkey filter for each one
                foreach (string userbackendid in userbackendidslist)
                {
                    string rowkeyfilter = TableQuery.GenerateFilterCondition(CoreConstants.AzureTables.RowKey, QueryComparisons.Equal, userbackendid);
                    //combine partitionkey filter with rowkey to get each entity
                    string currentrowfilter = TableQuery.CombineFilters(partitionkeyfilter, TableOperators.And, rowkeyfilter);
                    //if it is at first postion, no need to add OR condotion
                    if ((userbackendidslist.First() == userbackendid))
                    {
                        finalfilter = currentrowfilter;
                    }
                    else
                    {
                        finalfilter = TableQuery.CombineFilters(finalfilter, TableOperators.Or, currentrowfilter);
                    }
                }
                //generate query to get all user associated backends
                TableQuery<UserBackendEntity> query = new TableQuery<UserBackendEntity>().Where(finalfilter);
                List<UserBackendEntity> alluserbackends = DataProvider.GetEntitiesList<UserBackendEntity>(CoreConstants.AzureTables.UserDeviceConfiguration, query);
                return alluserbackends;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                //LoggerHelper.WriteToLog(exception + " - Error while retrieving userbackends from userdeviceconfig azure table in DAL : "
                //+ exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new DataAccessException();
            }
        }

        /// <summary>
        /// method to get approvals per userbackend
        /// </summary>
        /// <param name="UserID">takes userid as input</param>
        /// <param name="backendID">takes backendid as input</param>
        /// /// <param name="approvalstatus">takes approvalstatus as input</param>
        /// <returns>returns list of approvals associated userbackend</returns>
        public List<ApprovalEntity> GetUserBackendApprovals(string UserID, string backendID, string approvalstatus)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                string partitionkeyfilter = TableQuery.GenerateFilterCondition(CoreConstants.AzureTables.PartitionKey, QueryComparisons.Equal, string.Concat(CoreConstants.AzureTables.ApprovalPK, UserID));
                string backendfilter = TableQuery.GenerateFilterCondition(CoreConstants.AzureTables.BackendId, QueryComparisons.Equal, backendID);
                //combine partionkey filter with backend filter 
                string combinefiletr = TableQuery.CombineFilters(partitionkeyfilter, TableOperators.And, backendfilter);
                //request status filter
                string statusfilter = TableQuery.GenerateFilterCondition(CoreConstants.AzureTables.Status, QueryComparisons.Equal, approvalstatus);
                //final filter to get requests based on partitionkey, backendid, request status
                string finalfilter = TableQuery.CombineFilters(combinefiletr, TableOperators.And, statusfilter);
                //generate query to get all user associated requests
                TableQuery<ApprovalEntity> query = new TableQuery<ApprovalEntity>().Where(finalfilter);
                //call dataprovider method to get entities from azure table
                List<ApprovalEntity> allapprovals = DataProvider.GetEntitiesList<ApprovalEntity>(CoreConstants.AzureTables.RequestTransactions, query);
                return allapprovals;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                //LoggerHelper.WriteToLog(exception + " - Error while retrieving approvals per userbackend from RequestTransactions azure table in DAL : "
                //+ exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new DataAccessException();
            }
        }

        /// <summary>
        /// method to get all requests per user
        /// </summary>
        /// <param name="UserID">takes userid as input</param>
        /// <returns>returns list of requests associated to user</returns>
        public List<ApprovalEntity> GetUserApprovalsForCount(string UserID, string approvalstatus)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                //partionkey filter
                string partitionkeyfilter = TableQuery.GenerateFilterCondition(CoreConstants.AzureTables.PartitionKey, QueryComparisons.Equal, string.Concat(CoreConstants.AzureTables.ApprovalPK, UserID));
                //request status filter
                string statusfilter = TableQuery.GenerateFilterCondition(CoreConstants.AzureTables.Status, QueryComparisons.Equal, approvalstatus);
                //final filter to get approvals based on partitionkey, status
                string finalfilter = TableQuery.CombineFilters(partitionkeyfilter, TableOperators.And, statusfilter);
                //generate query to get all approvals per user based on status                
                TableQuery<ApprovalEntity> query = new TableQuery<ApprovalEntity>().Where(finalfilter);
                //call dataprovider method to get entities from azure table
                List<ApprovalEntity> allapprovals = DataProvider.GetEntitiesList<ApprovalEntity>(CoreConstants.AzureTables.RequestTransactions, query);
                return allapprovals;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                //LoggerHelper.WriteToLog(exception + " - Error while retrieving approvals count per user from RequestTransactions azure table in DAL : "
                //+ exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new DataAccessException();
            }
        }
        public List<ApprovalEntity> GetAllUserApprovalsForCount(string UserID)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                //partionkey filter
                string partitionkeyfilter = TableQuery.GenerateFilterCondition(CoreConstants.AzureTables.PartitionKey, QueryComparisons.Equal, string.Concat(CoreConstants.AzureTables.ApprovalPK, UserID));
               
                //final filter to get approvals based on partitionkey, status
                //string finalfilter = TableQuery.CombineFilters(partitionkeyfilter, TableOperators.And, statusfilter);
                //generate query to get all approvals per user based on status                
                TableQuery<ApprovalEntity> query = new TableQuery<ApprovalEntity>().Where(partitionkeyfilter);
                //call dataprovider method to get entities from azure table
                List<ApprovalEntity> allapprovals = DataProvider.GetEntitiesList<ApprovalEntity>(CoreConstants.AzureTables.RequestTransactions, query);
                return allapprovals;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                //LoggerHelper.WriteToLog(exception + " - Error while retrieving approvals count per user from RequestTransactions azure table in DAL : "
                //+ exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new DataAccessException();
            }
        }

        /// <summary>
        /// Adding  message into update trigger input queue
        /// </summary>
        /// <param name="content"></param>
        /// <param name="PartitionKey"></param>
        public void AddMessagestoInputQueue(List<string> lstcontent)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                int RetryAttemptCount = 0;
                bool IsSuccessful = false;
                // Create the queue client.
                CloudQueueClient cqdocClient = GetQueueClient();
                // Retrieve a reference to a queue.
                CloudQueue queuedoc= GetMissedUpdatesInputQueue(cqdocClient);                
                // Async enqueue the message
                AsyncCallback callBack = new AsyncCallback(AddMessageComplete);
                int documentCount = 0;
                //add each message from list
                foreach (string content in lstcontent)
                {

                    string dMessage = string.Empty;
                    dMessage = content;
                    CloudQueueMessage message = new CloudQueueMessage(dMessage);
                    do
                    {
                        try
                        {
                            queuedoc.BeginAddMessage(message, callBack, null);
                            documentCount++;
                            IsSuccessful = true;
                           // InsightLogger.TrackEvent("UpdateTriggering, Action :: Put update message in queue , Response :: Success");
                        }
                        catch (StorageException storageException)
                        {
                            //Increasing RetryAttemptCount variable
                            RetryAttemptCount = RetryAttemptCount + 1;
                            //Checking retry call count is eual to max retry count or not
                            if (RetryAttemptCount == maxRetryCount)
                            {
                               // InsightLogger.Exception("UpdateTriggering :: " + callerMethodName + " method :: Retry attempt count: [ " + RetryAttemptCount + " ]", storageException, callerMethodName);
                                throw new DataAccessException(storageException.Message, storageException.InnerException);
                            }
                            else
                            {
                                //InsightLogger.Exception("UpdateTriggering :: " + callerMethodName + " method :: Retry attempt count: [ " + RetryAttemptCount + " ]", storageException, callerMethodName);
                                //Putting the thread into some milliseconds sleep  and again call the same method call.
                                Thread.Sleep(maxThreadSleepInMilliSeconds);
                            }
                        }
                    } while (!IsSuccessful);

                }

            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                throw new DataAccessException(exception.Message, exception.InnerException);
            }


        }
        static void AddMessageComplete(IAsyncResult ar)
        {
            // do something
            string tmp = string.Empty;
        }
        public  static CloudQueueClient GetQueueClient()
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                // Parse the connection string and return a reference to the storage account
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["GenericMobileStorageConnectionString"]);

                // Create the queue client.
                CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
                // set retry for the connection for transient failures
                queueClient.DefaultRequestOptions = new QueueRequestOptions
                {
                    RetryPolicy = new LinearRetry(TimeSpan.FromSeconds(5), 3)
                };
                return queueClient;

            }
            catch (Exception innerexception)
            {
                InsightLogger.Exception(innerexception.Message, innerexception, callerMethodName);
                throw new DataAccessException(innerexception.Message, innerexception.InnerException);
            }

        }
        public static CloudQueue GetMissedUpdatesInputQueue(CloudQueueClient queuePath)
        {
            string callerMethodName = string.Empty;
            try
            {

                callerMethodName = CallerInformation.TrackCallerMethodName();
                //read queue name from app.config :: AppSettings and return queue
                CloudQueue queue = queuePath.GetQueueReference(ConfigurationManager.AppSettings["VIPMessagesQueue"]);
                return queue;

            }

            catch (Exception innerexception)
            {
                InsightLogger.Exception(innerexception.Message, innerexception, callerMethodName);
                throw new DataAccessException(innerexception.Message, innerexception.InnerException);
            }
        }

    }
}