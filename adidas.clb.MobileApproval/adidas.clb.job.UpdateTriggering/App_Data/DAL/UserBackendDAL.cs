//-----------------------------------------------------------
// <copyright file="UserBackendDAL.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using adidas.clb.job.UpdateTriggering.App_Data.BAL;
using adidas.clb.job.UpdateTriggering.Exceptions;
using adidas.clb.job.UpdateTriggering.Helpers;
using adidas.clb.job.UpdateTriggering.Models;
using adidas.clb.job.UpdateTriggering.Utility;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Timers;
using System.Threading.Tasks;

namespace adidas.clb.job.UpdateTriggering.App_Data.DAL
{
    /// <summary>
    /// Implements UserBackendDAL Class
    /// </summary>
    class UserBackendDAL
    {

        //Application insights interface reference for logging the error details into Application Insight azure service.
        static IAppInsight InsightLogger { get { return AppInsightLogger.Instance; } }
        //decalre UpdateTriggeringRules calss object 
        private UpdateTriggeringRules utRule;
        public UserBackendDAL()
        {
            //create new object for UpdateTriggeringRules class
            utRule = new UpdateTriggeringRules();
        }
        /// <summary>
        /// Get all user backends which are need update from UserBackend Azure table based on  Rule R2 & backend
        /// </summary>
        /// <param name="BackendName"></param>
        public void CollectUsersNeedUpdateByBackend(string BackendID)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                var cts = new CancellationTokenSource();
                //get's azure table instance
                CloudTable UserDeviceConfigurationTable = DataProvider.GetAzureTableInstance(ConfigurationManager.AppSettings["AzureTables.UserDeviceConfiguration"]);
                //Get all the userbackends associated with the backend
                TableQuery<UserBackend> tquery = new TableQuery<UserBackend>().Where(TableQuery.GenerateFilterCondition(CoreConstants.AzureTables.RowKey, QueryComparisons.Equal, BackendID));

                Task[] tasks = new Task[2];
                var entityCollection = new BlockingCollection<List<UserBackend>>();
                //read Userbackends from azure table which are needs update
                tasks[0] = Task.Factory.StartNew(() => ReadUsrBackendsDataFromAzureTable(UserDeviceConfigurationTable, tquery, entityCollection), TaskCreationOptions.LongRunning);
                //write update trigger messages into input queue
                tasks[1] = Task.Factory.StartNew(() => WriteMessagesIntoInputQueue(entityCollection, BackendID), TaskCreationOptions.LongRunning);
                int timeoutperiod = Convert.ToInt32(CloudConfigurationManager.GetSetting("timeoutperiod"));
                if (!Task.WaitAll(tasks, timeoutperiod, cts.Token))
                {
                    cts.Cancel();
                }
                else
                {
                    entityCollection.Dispose();
                }
            }

            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                throw new DataAccessException(exception.Message, exception.InnerException);
            }

        }
        /// <summary>
        /// Read all the userbackends which needs to update  from userbackend azure table by backend name
        /// </summary>
        /// <param name="tableReference"></param>
        /// <param name="tq"></param>
        /// <param name="collection"></param>
        private void ReadUsrBackendsDataFromAzureTable(CloudTable tableReference, TableQuery<UserBackend> tq, BlockingCollection<List<UserBackend>> collection)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                double rowcount = 0;
                TableContinuationToken tableContinuationToken = null;
                TableQuerySegment<UserBackend> queryResponse;
                List<UserBackend> lstUserBackends = null;
                //by defaylt azure ExecuteQuery will return 1000 records in single call, if reterival rows is more than 1000 then we need to use ExecuteQuerySegmented

                do
                {
                    queryResponse = tableReference.ExecuteQuerySegmented<UserBackend>(tq, tableContinuationToken, null, null);
                    //queryResponse will fetch the rows from userbackend azure table untill tableContinuationToken is null 
                    if (queryResponse.ContinuationToken != null)
                    {
                        tableContinuationToken = queryResponse.ContinuationToken;
                    }
                    else
                    {
                        tableContinuationToken = null;
                    }

                    rowcount += queryResponse.Results.Count;
                    lstUserBackends = new List<UserBackend>();
                    //adding result set to List<UserBackendEntity>
                    lstUserBackends.AddRange(queryResponse.Results);
                    //adding List<UserBackendEntity> to BlockingCollection<List<UserBackendEntity>>
                    collection.Add(lstUserBackends);
                    lstUserBackends = null;
                } while (tableContinuationToken != null);
                collection.CompleteAdding();
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                throw new DataAccessException(exception.Message, exception.InnerException);
            }


        }
        /// <summary>
        /// If user backends needs update then put the messages into update trigger input queue
        /// </summary>
        /// <param name="source"></param>
        /// <param name="ObjBackend"></param>
        private void WriteMessagesIntoInputQueue(BlockingCollection<List<UserBackend>> source, string BackendID)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();

                foreach (var item in source.GetConsumingEnumerable())
                {
                    if (item.ToList() != null)
                    {
                        List<string> msgFormat = new List<string>();
                        foreach (UserBackend userBackend in item.ToList())
                        {
                            //checking is user backend needs update or not with the help of Updatetriggering rule R2
                            if (userBackend.LastUpdate != null && utRule.IsuserBackendNeedsUpdate(userBackend.UpdateTriggered, userBackend.LastUpdate, userBackend.DefaultUpdateFrequency))
                            {
                                //clone values to UpdateTriggeringMsg class
                                msgFormat.Add(ConvertUserUpdateMsgToUpdateTriggeringMsg(userBackend, BackendID));

                            }


                        }
                        //add list of messages into update triggering input queue
                        if (msgFormat.Count > 0)
                        {
                            AddMessagestoInputQueue(msgFormat);
                        }

                    }

                }
            }
            catch (BusinessLogicException dalexception)
            {
                throw dalexception;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                throw new DataAccessException(exception.Message, exception.InnerException);
            }

        }
        /// <summary>
        /// Adding  message into update trigger input queue
        /// </summary>
        /// <param name="content"></param>
        /// <param name="PartitionKey"></param>
        private void AddMessagestoInputQueue(List<string> content)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                // Create the queue client.
                CloudQueueClient cqdocClient = AzureQueues.GetQueueClient();
                // Retrieve a reference to a queue.
                CloudQueue queuedoc = AzureQueues.GetInputQueue(cqdocClient);
                // Async enqueue the message
                AsyncCallback callBack = new AsyncCallback(AddMessageComplete);
                int documentCount = 0;

                foreach (string entry in content)
                {
                    string dMessage = string.Empty;
                    dMessage = entry;
                    CloudQueueMessage message = new CloudQueueMessage(dMessage);
                    queuedoc.BeginAddMessage(message, callBack, null);
                    documentCount++;
                }

            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                throw new DataAccessException(exception.Message, exception.InnerException);
            }


        }
        /// <summary>
        /// Prepare UpdateTriggeringMsg json string
        /// </summary>
        /// <param name="objuserBackend"></param>
        /// <param name="BackendName"></param>
        /// <returns></returns>
        private string ConvertUserUpdateMsgToUpdateTriggeringMsg(UserBackend objuserBackend, string BackendName)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                string updatetriggeringmsg = string.Empty;
                //Backend list
                List<Backend> lstBackend = null;
                //Users list
                List<UserUpdateMsg> lstuserUpdateMsg = null;
                if (objuserBackend != null)
                {
                    lstBackend = new List<Backend>();
                    lstBackend.Add(new Backend()
                    {
                        //Backend ID
                        BackendID = objuserBackend.BackendID,
                        BackendName = BackendName

                    });

                    lstuserUpdateMsg = new List<UserUpdateMsg>();
                    //creating object for UserUpdateMsg class
                    lstuserUpdateMsg.Add(new UserUpdateMsg()
                    {
                        UserID = objuserBackend.UserID,
                        Backends = lstBackend
                    });
                }

                //creating object for UpdateTriggeringMsg class
                UpdateTriggeringMsg ObjUTMsg = new UpdateTriggeringMsg()
                {
                    Users = lstuserUpdateMsg,
                    Requests = null,
                    VIP = false,
                    GetPDFs = false,
                    ChangeAfter = objuserBackend.LastUpdate

                };
                //Serialize UpdateTriggeringMsg Object into json string
                updatetriggeringmsg = JsonConvert.SerializeObject(ObjUTMsg);
                return updatetriggeringmsg;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                throw new DataAccessException(exception.Message, exception.InnerException);

            }
        }
        /// <summary>
        /// Get all backend details from azure table
        /// </summary>
        /// <returns></returns>
        public List<BackendEntity> GetBackends()
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                //get's azure table instance
                CloudTable UserDeviceConfigurationTable = DataProvider.GetAzureTableInstance(ConfigurationManager.AppSettings["AzureTables.ReferenceData"]);
                TableQuery<BackendEntity> query = new TableQuery<BackendEntity>().Where(TableQuery.GenerateFilterCondition(CoreConstants.AzureTables.PartitionKey, QueryComparisons.Equal, CoreConstants.AzureTables.Backend));
                List<BackendEntity> allBackends = UserDeviceConfigurationTable.ExecuteQuery(query).ToList();
                return allBackends;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                throw new DataAccessException(exception.Message, exception.InnerException);
            }
        }
        /// <summary>
        /// This method will return backend detailsfrom azure table by backend ID
        /// </summary>
        /// <param name="backendID"></param>
        /// <returns></returns>
        public BackendEntity GetBackendDetailsByBackendID(string backendID)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                //get's azure table instance
                CloudTable tblReferenceData = DataProvider.GetAzureTableInstance(ConfigurationManager.AppSettings["AzureTables.ReferenceData"]);
                // Create a retrieve operation that takes a UserBackend entity.
                TableOperation retrieveOperation = TableOperation.Retrieve<BackendEntity>(CoreConstants.AzureTables.Backend, backendID);
                // Execute the operation.
                TableResult retrievedResult = tblReferenceData.Execute(retrieveOperation);
                // Assign the result to a UserBackendEntity object.
                BackendEntity backendEntity = (BackendEntity)retrievedResult.Result;
                return backendEntity;
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
        /// <summary>
        /// This method collects all the user backends which are missed updates based on BackendID
        /// </summary>
        /// <param name="BackendID"></param>
        public void CollectUsersMissedUpdatesByBackend(string BackendID)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                var cts = new CancellationTokenSource();
                //get's azure table instance
                CloudTable UserMissedDeviceConfigurationTable = DataProvider.GetAzureTableInstance(ConfigurationManager.AppSettings["AzureTables.UserDeviceConfiguration"]);
                //Get all the userbackends associated with the backend
                TableQuery<UserBackend> tquerymissedupdate = new TableQuery<UserBackend>().Where(TableQuery.GenerateFilterCondition(CoreConstants.AzureTables.RowKey, QueryComparisons.Equal, BackendID));

                Task[] tasks = new Task[2];
                var entityUserMissedupdateCollection = new BlockingCollection<List<UserBackend>>();
                tasks[0] = Task.Factory.StartNew(() => ReadMissedUpdatesFromUsrBackendsDataFromAzureTable(UserMissedDeviceConfigurationTable, tquerymissedupdate, entityUserMissedupdateCollection), TaskCreationOptions.LongRunning);
                tasks[1] = Task.Factory.StartNew(() => WriteMissedUpdatesMessagesIntoInputQueue(entityUserMissedupdateCollection, BackendID), TaskCreationOptions.LongRunning);
                int timeoutperiod = Convert.ToInt32(CloudConfigurationManager.GetSetting("timeoutperiod"));
                if (!Task.WaitAll(tasks, timeoutperiod, cts.Token))
                {
                    cts.Cancel();
                }
                else
                {
                    //dispose blocking collection
                    entityUserMissedupdateCollection.Dispose();
                }

            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                throw new DataAccessException(exception.Message, exception.InnerException);
            }

        }
        /// <summary>
        /// Read all the userbackends based on backend and put into blocking collection list
        /// </summary>
        /// <param name="mtableReference"></param>
        /// <param name="mtq"></param>
        /// <param name="missedUpdateCollection"></param>
        private void ReadMissedUpdatesFromUsrBackendsDataFromAzureTable(CloudTable mtableReference, TableQuery<UserBackend> mtq, BlockingCollection<List<UserBackend>> missedUpdateCollection)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                double mrowcount = 0;
                TableContinuationToken mtableContinuationToken = null;
                TableQuerySegment<UserBackend> mqueryResponse;
                List<UserBackend> lstMissedUserBackends = null;

                //by defaylt azure ExecuteQuery will return 1000 records in single call, if reterival rows is more than 1000 then we need to use ExecuteQuerySegmented

                do
                {
                    mqueryResponse = mtableReference.ExecuteQuerySegmented<UserBackend>(mtq, mtableContinuationToken, null, null);
                    //queryResponse will fetch the rows from userbackend azure table untill tableContinuationToken is null 
                    if (mqueryResponse.ContinuationToken != null)
                    {
                        mtableContinuationToken = mqueryResponse.ContinuationToken;
                    }
                    else
                    {
                        mtableContinuationToken = null;
                    }

                    mrowcount += mqueryResponse.Results.Count;
                    lstMissedUserBackends = new List<UserBackend>();
                    //adding result set to List<UserBackendEntity>
                    lstMissedUserBackends.AddRange(mqueryResponse.Results);
                    //adding List<UserBackendEntity> to BlockingCollection<List<UserBackendEntity>>
                    missedUpdateCollection.Add(lstMissedUserBackends);
                    lstMissedUserBackends = null;
                } while (mtableContinuationToken != null);
                missedUpdateCollection.CompleteAdding();
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                throw new DataAccessException(exception.Message, exception.InnerException);
            }


        }
        /// <summary>
        /// Check each userbackend whether it's missed update or not,if any userbackend missed update keep the messages into update trigger input queue
        /// in UpdateTriggerMSg format
        /// </summary>
        /// <param name="msource"></param>
        /// <param name="mBackendID"></param>
        private void WriteMissedUpdatesMessagesIntoInputQueue(BlockingCollection<List<UserBackend>> msource, string mBackendID)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                foreach (var mitem in msource.GetConsumingEnumerable())
                {
                    if (mitem.ToList() != null)
                    {
                        List<string> msgFormat = new List<string>();
                        foreach (UserBackend muserBackend in mitem.ToList())
                        {
                            //checking is user backend update missing or not with the help of Updatetriggering rule R6
                            if (utRule.IsUserUpdateMissing(muserBackend.UpdateTriggered, muserBackend.ExpectedUpdate))
                            {
                                //put the json message of UpdateTriggeringMsg class format into update triggering input queue.
                                msgFormat.Add(ConvertUserUpdateMsgToUpdateTriggeringMsg(muserBackend, mBackendID));
                            }
                        }
                        //add list of messages into update triggering input queue
                        if (msgFormat.Count > 0)
                        {
                            AddMessagestoInputQueue(msgFormat);
                        }

                    }

                }
            }
            catch (BusinessLogicException dalexception)
            {
                throw dalexception;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                throw new DataAccessException(exception.Message, exception.InnerException);
            }

        }
        /// <summary>
        /// This method updates UserBackend ExpectedUpdateTime
        /// </summary>
        /// <param name="backendID"></param>
        /// <param name="userName"></param>
        public void UpdateUserBackendExpectedUpdateTime(string backendID, string userName)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                //getting Userbackend partitionkey (UB_UserName)
                string userBackendID = CoreConstants.AzureTables.UserBackendPK + userName;
                CloudTable UserDeviceConfigurationTable = DataProvider.GetAzureTableInstance(ConfigurationManager.AppSettings["AzureTables.UserDeviceConfiguration"]);
                // Create a retrieve operation that takes a UserBackend entity.
                TableOperation retrieveOperation = TableOperation.Retrieve<UserBackendEntity>(userBackendID, backendID);
                // Execute the operation.
                TableResult retrievedResult = UserDeviceConfigurationTable.Execute(retrieveOperation);
                // Assign the result to a UserBackendEntity object.
                UserBackendEntity updateEntity = (UserBackendEntity)retrievedResult.Result;
                if (updateEntity != null)
                {
                    //get backend details from service layer by backendid
                    BackendEntity backendDetails = GetBackendDetailsByBackendID(backendID);
                    if (backendDetails != null)
                    {
                        // update userbackend ExpectedUpdate value  by using update triggering rule :: R3
                        updateEntity.ExpectedUpdate = utRule.GetUserBackendExpectedUpdate(backendDetails.AverageAllRequestsLatency, backendDetails.LastAllRequestsLatency);
                        //update userbackend UpdateTriggered
                        updateEntity.UpdateTriggered = true;
                        // Create the Replace TableOperation.
                        TableOperation updateOperation = TableOperation.Replace(updateEntity);
                        // Execute the operation.
                        UserDeviceConfigurationTable.Execute(updateOperation);
                    }
                }
            }
            catch (BusinessLogicException dalexception)
            {
                throw dalexception;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                throw new DataAccessException(exception.Message, exception.InnerException);
            }
        }
        /// <summary>
        /// This method updates Request ExpectedUpdateTime
        /// </summary>
        /// <param name="backendID"></param>
        /// <param name="serviceLayerRequestID"></param>
        public void UpdateRequestExpectedUpdateTime(string backendID, string serviceLayerRequestID)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                //get request details based on serviceLayerRequestID        
                CloudTable UserDeviceConfigurationTable = DataProvider.GetAzureTableInstance(ConfigurationManager.AppSettings["AzureTables.UserDeviceConfiguration"]);
                TableQuery<RequestSynchEntity> tquery = new TableQuery<RequestSynchEntity>().Where(TableQuery.GenerateFilterCondition(CoreConstants.AzureTables.RowKey, QueryComparisons.Equal, serviceLayerRequestID));
                List<RequestSynchEntity> lstUserBackend = UserDeviceConfigurationTable.ExecuteQuery(tquery).ToList();
                if (lstUserBackend != null)
                {
                    // Assign the result to a RequestSynchEntity object.
                    RequestSynchEntity updateEntity = lstUserBackend.FirstOrDefault();
                    if (updateEntity != null)
                    {
                        //get backend details
                        BackendEntity backendDetails = GetBackendDetailsByBackendID(backendID);
                        if (backendDetails != null)
                        {
                            // update request ExpectedUpdate value by using update triggering rule :: R4
                            updateEntity.ExpectedUpdate = utRule.GetRequestExpectedUpdate(backendDetails.AverageRequestLatency, backendDetails.LastRequestLatency);
                            //update request UpdateTriggered
                            updateEntity.UpdateTriggered = true;
                            // Create the Replace TableOperation.
                            TableOperation updateOperation = TableOperation.Replace(updateEntity);
                            // Execute the operation.
                            UserDeviceConfigurationTable.Execute(updateOperation);
                        }
                    }
                }

            }
            catch (BusinessLogicException dalexception)
            {
                throw dalexception;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                throw new DataAccessException(exception.Message, exception.InnerException);
            }
        }
        /// <summary>
        /// This methid collects missed requests which have missed updates and put these requests into UT input queue in requestsUpdateMsg Format
        /// </summary>
        /// <param name="backendID"></param>
        public void CollectsRequestsMissedUpdateByBackendID(string backendID)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                var ctsRequests = new CancellationTokenSource();
                //get's azure table instance
                CloudTable RequestsMissedDeviceConfigurationTable = DataProvider.GetAzureTableInstance(ConfigurationManager.AppSettings["AzureTables.RequestTransactions"]);
                //Get all the userbackends associated with the backend

                string partitionFilter = TableQuery.GenerateFilterCondition(CoreConstants.AzureTables.PartitionKey, QueryComparisons.Equal, CoreConstants.AzureTables.RequestSynchPK);
                string rowfilter = TableQuery.GenerateFilterCondition(CoreConstants.AzureTables.BackendID, QueryComparisons.Equal, backendID);
                TableQuery<RequestSynchEntity> tquerymissedRequests = new TableQuery<RequestSynchEntity>().Where(TableQuery.CombineFilters(partitionFilter, TableOperators.And, rowfilter));

                Task[] taskRequestCollection = new Task[2];
                var entityMissedupdateRequestsCollection = new BlockingCollection<List<RequestSynchEntity>>();
                taskRequestCollection[0] = Task.Factory.StartNew(() => ReadMissedUpdatesRequestsByBackend(RequestsMissedDeviceConfigurationTable, tquerymissedRequests, entityMissedupdateRequestsCollection), TaskCreationOptions.LongRunning);
                taskRequestCollection[1] = Task.Factory.StartNew(() => WriteMissedUpdatesRequestsIntoInputQueue(entityMissedupdateRequestsCollection, backendID), TaskCreationOptions.LongRunning);
                int requestTimeoutperiod = Convert.ToInt32(CloudConfigurationManager.GetSetting("timeoutperiod"));
                if (!Task.WaitAll(taskRequestCollection, requestTimeoutperiod, ctsRequests.Token))
                {
                    ctsRequests.Cancel();
                }
                else
                {
                    //dispose blocking collection
                    entityMissedupdateRequestsCollection.Dispose();
                }
            }
            catch (BusinessLogicException dalexception)
            {
                throw dalexception;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                throw new DataAccessException(exception.Message, exception.InnerException);
            }
        }
        /// <summary>
        /// This method will return all the requests which have missed updates from azure table
        /// </summary>
        /// <param name="requestTableReference"></param>
        /// <param name="rtq"></param>
        /// <param name="missedUpdateRequestsCollection"></param>
        private void ReadMissedUpdatesRequestsByBackend(CloudTable requestTableReference, TableQuery<RequestSynchEntity> rtq, BlockingCollection<List<RequestSynchEntity>> missedUpdateRequestsCollection)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                double rRowcount = 0;
                TableContinuationToken rTableContinuationToken = null;
                TableQuerySegment<RequestSynchEntity> rQueryResponse;
                List<RequestSynchEntity> lstMissedRequests = null;

                //by defaylt azure ExecuteQuery will return 1000 records in single call, if reterival rows is more than 1000 then we need to use ExecuteQuerySegmented

                do
                {
                    rQueryResponse = requestTableReference.ExecuteQuerySegmented<RequestSynchEntity>(rtq, rTableContinuationToken, null, null);
                    //queryResponse will fetch the rows from userbackend azure table untill tableContinuationToken is null 
                    if (rQueryResponse.ContinuationToken != null)
                    {
                        rTableContinuationToken = rQueryResponse.ContinuationToken;
                    }
                    else
                    {
                        rTableContinuationToken = null;
                    }

                    rRowcount += rQueryResponse.Results.Count;
                    lstMissedRequests = new List<RequestSynchEntity>();
                    //adding result set to List<UserBackendEntity>
                    lstMissedRequests.AddRange(rQueryResponse.Results);
                    //adding List<UserBackendEntity> to BlockingCollection<List<UserBackendEntity>>
                    missedUpdateRequestsCollection.Add(lstMissedRequests);
                    lstMissedRequests = null;
                } while (rTableContinuationToken != null);
                missedUpdateRequestsCollection.CompleteAdding();
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                throw new DataAccessException(exception.Message, exception.InnerException);
            }


        }
        /// <summary>
        /// This method verfies whether the request list missed updates or not based on UT Rule  R6
        /// If any request missed updates then it will converts the request detaisl into update triggering message format and
        /// put the messages into Update triggering input queue
        /// </summary>
        /// <param name="rsource"></param>
        /// <param name="rBackendID"></param>
        private void WriteMissedUpdatesRequestsIntoInputQueue(BlockingCollection<List<RequestSynchEntity>> rsource, string rBackendID)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();

                foreach (var requestitem in rsource.GetConsumingEnumerable())
                {
                    if (requestitem.ToList() != null)
                    {
                        List<string> rmsgFormat = new List<string>();
                        foreach (RequestSynchEntity requestDetails in requestitem.ToList())
                        {
                            //checking is request  update missing or not with the help of Updatetriggering rule R6
                            if (utRule.IsRequestUpdateMissing(requestDetails.UpdateTriggered, requestDetails.ExpectedUpdate))
                            {
                                //put the json message of UpdateTriggeringMsg class format into update triggering input queue.
                                rmsgFormat.Add(ConvertRequestUpdateMsgToUpdateTriggeringMsg(requestDetails, rBackendID));
                            }
                        }
                        //add list of messages into update triggering input queue
                        if (rmsgFormat.Count > 0)
                        {
                            AddMessagestoInputQueue(rmsgFormat);
                        }

                    }

                }
            }
            catch (BusinessLogicException dalexception)
            {
                throw dalexception;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                throw new DataAccessException(exception.Message, exception.InnerException);
            }

        }
        /// <summary>
        /// This method converts request details into Update Triggering Message Format
        /// </summary>
        /// <param name="objrequestSynch"></param>
        /// <param name="rBackendName"></param>
        /// <returns></returns>
        private string ConvertRequestUpdateMsgToUpdateTriggeringMsg(RequestSynchEntity objrequestSynch, string rBackendName)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                string updatetriggeringmsg = string.Empty;

                //create object and assign values to properties for Backend class 
                Backend objBackend = new Backend()
                {
                    BackendID = objrequestSynch.BackendID,
                    BackendName = rBackendName
                };
                //create object and assign values to properties for Request class 
                Request objRequest = new Request()
                {
                    ID = objrequestSynch.RowKey,
                    UserID= objrequestSynch.UserID,
                    Backend = objBackend
                };
                //create object and assign values to properties for RequestUpdateMsg class 
                RequestUpdateMsg objRequestMsg = new RequestUpdateMsg()
                {
                    ServiceLayerReqID = objrequestSynch.RowKey,
                    request = objRequest

                };
                //add RequestUpdateMsg to list
                List<RequestUpdateMsg> lstRequestUpdateMsg = new List<RequestUpdateMsg>();
                lstRequestUpdateMsg.Add(objRequestMsg);
                //create object and assign values to properties for UpdateTriggeringMsg class 
                UpdateTriggeringMsg ObjUTMsg = new UpdateTriggeringMsg()
                {
                    Users = null,
                    Requests = lstRequestUpdateMsg,
                    VIP = false,
                    GetPDFs = false

                };
                //Serialize UpdateTriggeringMsg Object into json string
                updatetriggeringmsg = JsonConvert.SerializeObject(ObjUTMsg);
                return updatetriggeringmsg;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                throw new DataAccessException(exception.Message, exception.InnerException);

            }
        }
        /// <summary>
        /// This method updates the userbackend expected updatetime
        /// </summary>
        /// <param name="lstUsers"></param>
        /// <param name="queueMessage"></param>
        public void UpdateUserBackends(List<UserUpdateMsg> lstUsers, bool vip, bool generatePDF, Nullable<DateTime> changeAfter)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                string backendUserQuery = string.Empty;
                //checking users in UserUpdateMsg is not null or not
                if (lstUsers != null && lstUsers.Count > 0)
                {
                    string acknowledgment = string.Empty;
                    string userID = string.Empty;
                    List<Backend> lstbackends = null;

                    //foreach users in  UserUpdateMsg            
                    foreach (UserUpdateMsg users in lstUsers)
                    {
                        //getting list of backends in each user
                        lstbackends = users.Backends.ToList();
                        userID = users.UserID;
                        //creating Backend agent request query i.e RequestsUpdateQuery format
                        BackendUser objUser = new BackendUser()
                        {
                            UserID = users.UserID,
                            UserName = users.UserName
                        };


                        foreach (Backend backend in lstbackends)
                        {
                            //Creating request update query which is input for backend agent requestupdateretrival api
                            RequestsUpdateQuery objReqQuery = new RequestsUpdateQuery()
                            {
                                User = objUser,
                                BackendID = backend.BackendID,
                                Requests = null,
                                VIP = vip,
                                GetPDFs = generatePDF,
                                ChangeAfter = changeAfter
                            };
                            //convert RequestsUpdateQuery object into json string
                            backendUserQuery = JsonConvert.SerializeObject(objReqQuery);
                            InsightLogger.TrackEvent("RequestsUpdateQuery Message:" + backendUserQuery);
                            acknowledgment = string.Empty;
                            //initalize object for api service provider for callingt the web api
                            APIServiceProvider ObjserviceProvider = new APIServiceProvider();
                            //call backend agent api with backendID and input queue message and getting the acknowledgment from API
                            acknowledgment = ObjserviceProvider.CallBackendAgent(backend.BackendID, backendUserQuery);
                            //if acknowledgment is not null or not empty then update the userbackend expected updatetime
                            if (!string.IsNullOrEmpty(acknowledgment))
                            {
                                //update ExpectedUpdateTime  with the help of update trigger Rule :: R3
                                this.UpdateUserBackendExpectedUpdateTime(backend.BackendID, userID);
                            }

                        }
                        //clear the RequestsUpdateQuery message
                        backendUserQuery = string.Empty;
                        lstbackends = null;
                        userID = string.Empty;
                    }

                }

            }
            catch (ServiceLayerException serviceexception)
            {
                throw serviceexception;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                throw new DataAccessException(exception.Message, exception.InnerException);
            }
        }
        /// <summary>
        /// This method will updates the ExpectedUpdateTime of each request
        /// </summary>
        /// <param name="lstRequests"></param>
        /// <param name="queueMessage"></param>
        public void UpdateRequests(List<RequestUpdateMsg> lstRequests, bool vip, bool generatePDF, Nullable<DateTime> changeAfter)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                string requestsUpdateQuery = string.Empty;
                //checking requests in RequestUpdateMsg is not null or not
                if (lstRequests != null && lstRequests.Count > 0)
                {

                    string acknowledgment = string.Empty;
                    string serviceLayerRequestID = string.Empty;
                    string requestBackendID = string.Empty;
                    //get distinct backends from RequestUpdateMsg list
                    var bakcends = lstRequests.Select(o => o.request.Backend.BackendID).Distinct();                                      
                    
                    //foreach backend id
                    foreach (string backendID in bakcends.ToList())
                    {
                        //getting list of RequestUpdateMsg's by same backend
                        var lstRequestsByBackend = (from requests in lstRequests
                                     where requests.request.Backend.BackendID.Equals(backendID)
                                     select requests);
                        if (lstRequestsByBackend != null)
                        {                            
                            //Prepare RequestsUpdateQuery message
                            RequestsUpdateQuery objReqQuery = new RequestsUpdateQuery()
                            {
                                User = null,
                                BackendID = backendID,
                                Requests = lstRequestsByBackend,
                                VIP = vip,
                                GetPDFs = generatePDF,
                                ChangeAfter = changeAfter
                            };
                            //convert RequestsUpdateQuery object into json string
                            requestsUpdateQuery = JsonConvert.SerializeObject(objReqQuery);
                            InsightLogger.TrackEvent("RequestsUpdateQuery Message:" + requestsUpdateQuery);
                            acknowledgment = string.Empty;
                            //initalize object for api service provider for callingt the web api
                            APIServiceProvider ObjserviceProvider = new APIServiceProvider();
                            //call backend agent api with backendID and input queue message and getting the acknowledgment from API
                            acknowledgment = ObjserviceProvider.CallBackendAgent(backendID, requestsUpdateQuery);
                            //if acknowledgment is not null or not empty then update the userbackend expected updatetime
                            if (!string.IsNullOrEmpty(acknowledgment))
                            {
                                //update ExpectedUpdateTime  with the help of update trigger Rule :: R3
                                this.UpdateRequestExpectedUpdateTime(backendID, serviceLayerRequestID);
                            }
                            //clearing RequestUpdateMsg list
                            lstRequestsByBackend = null;
                            requestsUpdateQuery = string.Empty;
                        }
                        
                    }

                }
            }
            catch (BusinessLogicException balexception)
            {
                throw balexception;
            }
            catch (DataAccessException dalexception)
            {
                throw dalexception;
            }
            catch (ServiceLayerException serviceexception)
            {
                throw serviceexception;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                throw new DataAccessException(exception.Message, exception.InnerException);
            }
        }

    }
}
