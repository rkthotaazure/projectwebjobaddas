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
        //getting Max Retry count,MaxThreadSleepInMilliSeconds from web.config
        public static int maxThreadSleepInMilliSeconds = Convert.ToInt32(ConfigurationManager.AppSettings["MaxThreadSleepInMilliSeconds"]);
        public static int maxRetryCount = Convert.ToInt32(ConfigurationManager.AppSettings["MaxRetryCount"]);
        public static string azureTableReference = ConfigurationManager.AppSettings["AzureTables.ReferenceData"];
        public static string azureTableUserDeviceConfiguration = ConfigurationManager.AppSettings["AzureTables.UserDeviceConfiguration"];
        //read GetPDFs value from configuration
        public static bool IsGeneratePdfs = Convert.ToBoolean(ConfigurationManager.AppSettings["GetPDFs"]);
        //read VIP Flag  value from configuration
        public static bool IsVIPFlag = Convert.ToBoolean(ConfigurationManager.AppSettings["VIPFlag"]);
        //RequestTransactions
        public static string azureTableRequestTransactions = ConfigurationManager.AppSettings["AzureTables.RequestTransactions"];
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
        public void CollectUsersNeedUpdateByBackend(string BackendID, DateTime currentTime)
        {
            string callerMethodName = string.Empty;
            try
            {
                InsightLogger.TrackEvent("UpdateTriggering, Action :: Collect the users needing update for the backend [" + BackendID + "]");
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
                tasks[1] = Task.Factory.StartNew(() => WriteMessagesIntoInputQueue(entityCollection, BackendID,currentTime), TaskCreationOptions.LongRunning);
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
        private void WriteMessagesIntoInputQueue(BlockingCollection<List<UserBackend>> source, string BackendID,DateTime currentTimeStamp)
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
                        Parallel.ForEach<UserBackend>(item.ToList(), userBackend => 
                        {
                            //checking is user backend needs update or not with the help of Updatetriggering rule R2
                            if (userBackend.LastUpdate != null && utRule.IsuserBackendNeedsUpdate(userBackend.UpdateTriggered, userBackend.LastUpdate, userBackend.DefaultUpdateFrequency, currentTimeStamp))
                            {
                                InsightLogger.TrackEvent("UpdateTriggering, Action :: Is User [ " + userBackend.UserID + " ] need update for backend:[" + userBackend.BackendID + " ] based on UT Rule R2 , Response :: true");
                                //parse data to UpdateTriggeringMsg class and seralize UpdateTriggeringMsg object into json string                           
                                //put json string into update triggering input queue
                                AddMessagestoInputQueue(ConvertUserUpdateMsgToUpdateTriggeringMsg(userBackend, BackendID));
                            }
                            else
                            {
                                InsightLogger.TrackEvent("UpdateTriggering, Action :: Is User [ " + userBackend.UserID + " ] need update for backend:[" + userBackend.BackendID + " ] based on UT Rule R2 , Response :: false");
                            }
                        });                                            
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
        private void AddMessagestoInputQueue(string content)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                int RetryAttemptCount = 0;
                bool IsSuccessful = false;
                // Create the queue client.
                CloudQueueClient cqdocClient = AzureQueues.GetQueueClient();
                // Retrieve a reference to a queue.
                CloudQueue queuedoc = AzureQueues.GetInputQueue(cqdocClient);
                // Async enqueue the message
                AsyncCallback callBack = new AsyncCallback(AddMessageComplete);
                int documentCount = 0;
                //add each message from list
                if(string.IsNullOrEmpty(content))
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
                            InsightLogger.TrackEvent("UpdateTriggering, Action :: Put update message in queue , Response :: Success");
                        }
                        catch (StorageException storageException)
                        {
                            //Increasing RetryAttemptCount variable
                            RetryAttemptCount = RetryAttemptCount + 1;
                            //Checking retry call count is eual to max retry count or not
                            if (RetryAttemptCount == maxRetryCount)
                            {
                                InsightLogger.Exception("UpdateTriggering :: " + callerMethodName + " method :: Retry attempt count: [ " + RetryAttemptCount + " ]", storageException, callerMethodName);
                                throw new DataAccessException(storageException.Message, storageException.InnerException);
                            }
                            else
                            {
                                InsightLogger.Exception("UpdateTriggering :: " + callerMethodName + " method :: Retry attempt count: [ " + RetryAttemptCount + " ]", storageException, callerMethodName);
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
                    VIP = IsVIPFlag,
                    GetPDFs = IsGeneratePdfs,
                    ChangeAfter = objuserBackend.LastUpdate

                };
                //Serialize UpdateTriggeringMsg Object into json string
                updatetriggeringmsg = JsonConvert.SerializeObject(ObjUTMsg);
                InsightLogger.TrackEvent("UpdateTriggering, Action :: Prepare update triggering message , Response :: message:" + updatetriggeringmsg);

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
                List<BackendEntity> allBackends = DataProvider.RetrieveEntities<BackendEntity>(azureTableReference, CoreConstants.AzureTables.Backend);
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
                //// Call Retrive Entity method and Assign the result to a UserBackendEntity object.                
                BackendEntity backendEntity = DataProvider.RetrieveEntity<BackendEntity>(azureTableReference, CoreConstants.AzureTables.Backend, backendID);
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
        public void CollectUsersMissedUpdatesByBackend(string BackendID,DateTime currentTimestamp)
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
                tasks[1] = Task.Factory.StartNew(() => WriteMissedUpdatesMessagesIntoInputQueue(entityUserMissedupdateCollection, BackendID,currentTimestamp), TaskCreationOptions.LongRunning);
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
        private void WriteMissedUpdatesMessagesIntoInputQueue(BlockingCollection<List<UserBackend>> msource, string mBackendID,DateTime curtime)
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
                        Parallel.ForEach<UserBackend>(mitem.ToList(), muserBackend => 
                        {
                            //checking is user backend update missing or not with the help of Updatetriggering rule R6
                            if (utRule.IsUserUpdateMissing(muserBackend.UpdateTriggered, muserBackend.ExpectedUpdate, curtime))
                            {
                                InsightLogger.TrackEvent("UpdateTriggering, Action :: Is User [ " + muserBackend.UserID + " ] missed updates for the backend:[" + muserBackend.BackendID + " ] based on UT Rule R6 , Response :: true");
                                //parse data to UpdateTriggeringMsg class and seralize UpdateTriggeringMsg object into json string                           
                                //put json string into update triggering input queue
                                AddMessagestoInputQueue(ConvertUserUpdateMsgToUpdateTriggeringMsg(muserBackend, mBackendID));
                            }
                            else
                            {
                                InsightLogger.TrackEvent("UpdateTriggering, Action :: Is User [ " + muserBackend.UserID + " ] missed updates for the backend:[" + muserBackend.BackendID + " ] based on UT Rule R6 , Response :: false");
                            }

                            //Checking is any request missed update for this userbackend
                            CollectsRequestsMissedUpdateByBackendID(muserBackend.BackendID, muserBackend.UserID, curtime);

                        });                       
                      
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
        public void UpdateUserBackendExpectedUpdateTime(string backendID, string userName, string QueueName)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                //getting Userbackend partitionkey (UB_UserName)
                string userBackendID = CoreConstants.AzureTables.UserBackendPK + userName;
                // Assign the result to a UserBackendEntity object.
                UserBackendEntity updateEntity = DataProvider.RetrieveEntity<UserBackendEntity>(azureTableUserDeviceConfiguration, userBackendID, backendID);
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
                        // Execute the Replace TableOperation.
                        DataProvider.UpdateEntity<UserBackendEntity>(azureTableUserDeviceConfiguration, updateEntity);
                        InsightLogger.TrackEvent(QueueName + " , Action :: Compute and set Expected Updated Timestamp(UT Rule :: R3) for the userbackend : " + userName + " ,  Response : Success");
                    }
                    else
                    {
                        InsightLogger.TrackEvent(QueueName + " , Action :: Compute and set Expected Updated Timestamp(UT Rule :: R3) for the userbackend : " + userName + " ,  Response : Failed");
                    }
                }
                else
                {
                    InsightLogger.TrackEvent(QueueName + " , Action :: Compute and set Expected Updated Timestamp(UT Rule :: R3) for the userbackend : " + userName + " ,  Response : Failed");
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
        public void UpdateRequestExpectedUpdateTime(string backendID, List<RequestUpdateMsg> lstrequests, string QueueName)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                //get backend details
                BackendEntity backendDetails = GetBackendDetailsByBackendID(backendID);
                if (backendDetails != null)
                {
                    //get expected update time by using update triggering rule :: R4
                    DateTime expectedUpdateTime = utRule.GetRequestExpectedUpdate(backendDetails.AverageRequestLatency, backendDetails.LastRequestLatency);
                    foreach (RequestUpdateMsg reqdt in lstrequests)
                    {
                        string serviceLayerRequestID = reqdt.request.ID;
                        string userID = reqdt.request.UserID;
                        //get request sync details based on serviceLayerRequestID                 
                        RequestEntity reqSyncEntity = DataProvider.RetrieveEntity<RequestEntity>(azureTableRequestTransactions, CoreConstants.AzureTables.RequestPK + userID, serviceLayerRequestID);
                        if (reqSyncEntity != null)
                        {
                            // update request ExpectedUpdate value 
                            reqSyncEntity.ExpectedUpdate = expectedUpdateTime;
                            //update request UpdateTriggered
                            reqSyncEntity.UpdateTriggered = true;
                            // Execute the update operation.
                            DataProvider.UpdateEntity<RequestEntity>(azureTableRequestTransactions, reqSyncEntity);
                            InsightLogger.TrackEvent(QueueName + " , Action :: Compute and set Expected Updated Timestamp(UT Rule :: R4) for the requestID : " + serviceLayerRequestID + " ,  Response : Success");
                        }
                        else
                        {
                            InsightLogger.TrackEvent(QueueName + " , Action :: Compute and set Expected Updated Timestamp(UT Rule :: R4) for the requestID : " + serviceLayerRequestID + " ,  Response : Failed");

                        }
                    }
                   

                }
                else
                {
                    InsightLogger.TrackEvent(QueueName + " , Action :: Compute and set Expected Updated Timestamp for requests based on UT Rule :: R4  ,  Response : Failed");

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
        public void CollectsRequestsMissedUpdateByBackendID(string backendID,string userID,DateTime timestamp)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                InsightLogger.TrackEvent("UpdateTriggering, Action :: collecting the requests which have missed updates for the backend [" + backendID + "] and user: [" + userID + "]");

                var ctsRequests = new CancellationTokenSource();
                //get's azure table instance
                CloudTable RequestsMissedDeviceConfigurationTable = DataProvider.GetAzureTableInstance(ConfigurationManager.AppSettings["AzureTables.RequestTransactions"]);
                //Get all the userbackends associated with the backend

                string partitionFilter = TableQuery.GenerateFilterCondition(CoreConstants.AzureTables.PartitionKey, QueryComparisons.Equal, CoreConstants.AzureTables.RequestPK + userID);
                string rowfilter = TableQuery.GenerateFilterCondition(CoreConstants.AzureTables.BackendID, QueryComparisons.Equal, backendID);
                TableQuery<RequestEntity> tquerymissedRequests = new TableQuery<RequestEntity>().Where(TableQuery.CombineFilters(partitionFilter, TableOperators.And, rowfilter));

                Task[] taskRequestCollection = new Task[2];
                var entityMissedupdateRequestsCollection = new BlockingCollection<List<RequestEntity>>();
                taskRequestCollection[0] = Task.Factory.StartNew(() => ReadMissedUpdatesRequestsByBackend(RequestsMissedDeviceConfigurationTable, tquerymissedRequests, entityMissedupdateRequestsCollection), TaskCreationOptions.LongRunning);
                taskRequestCollection[1] = Task.Factory.StartNew(() => WriteMissedUpdatesRequestsIntoInputQueue(entityMissedupdateRequestsCollection, backendID, userID, timestamp), TaskCreationOptions.LongRunning);
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
        private void ReadMissedUpdatesRequestsByBackend(CloudTable requestTableReference, TableQuery<RequestEntity> rtq, BlockingCollection<List<RequestEntity>> missedUpdateRequestsCollection)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                double rRowcount = 0;
                TableContinuationToken rTableContinuationToken = null;
                TableQuerySegment<RequestEntity> rQueryResponse;
                List<RequestEntity> lstMissedRequests = null;

                //by defaylt azure ExecuteQuery will return 1000 records in single call, if reterival rows is more than 1000 then we need to use ExecuteQuerySegmented

                do
                {
                    rQueryResponse = requestTableReference.ExecuteQuerySegmented<RequestEntity>(rtq, rTableContinuationToken, null, null);
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
                    lstMissedRequests = new List<RequestEntity>();
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
        private void WriteMissedUpdatesRequestsIntoInputQueue(BlockingCollection<List<RequestEntity>> rsource, string rBackendID,string rUserID,DateTime CurTimestamp)
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
                        List<RequestEntity> reqmissedUpdateslst = new List<Models.RequestEntity>();
                        Parallel.ForEach<RequestEntity>(requestitem.ToList(), requestDetails => 
                        {
                            //checking is request  update missing or not with the help of Updatetriggering rule R6
                            if (utRule.IsRequestUpdateMissing(requestDetails.UpdateTriggered, requestDetails.ExpectedUpdate, CurTimestamp))
                            {
                                InsightLogger.TrackEvent("UpdateTriggering, Action :: Is Request [ " + requestDetails.RowKey + " ] needs update based on UT Rule R6 , Response :: true");
                                //add request details to RequestSynchEntity list
                                reqmissedUpdateslst.Add(requestDetails);
                                //parse data to UpdateTriggeringMsg class and seralize UpdateTriggeringMsg object into json string                           
                                //put json string into update triggering input queue
                                AddMessagestoInputQueue(ConvertRequestUpdateMsgToUpdateTriggeringMsg(reqmissedUpdateslst, rBackendID, rUserID));
                            }
                            else
                            {
                                InsightLogger.TrackEvent("UpdateTriggering, Action :: Is Request [ " + requestDetails.RowKey + " ] needs update based on UT Rule R6 , Response :: false");
                            }
                        });                      
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
        private string ConvertRequestUpdateMsgToUpdateTriggeringMsg(List<RequestEntity> reqlst, string rBackendName,string userID)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                //add RequestUpdateMsg to list
                List<RequestUpdateMsg> lstRequestUpdateMsg = new List<RequestUpdateMsg>();

                string updatetriggeringmsg = string.Empty;
                foreach (RequestEntity objrequestSynch in reqlst)
                {
                    //create object and assign values to properties for Backend class 
                    Backend objBackend = new Backend()
                    {
                        BackendID = objrequestSynch.BackendID,
                        BackendName = rBackendName
                    };
                    //create object and assign values to properties for Request class 
                    Request objRequest = new Request()
                    {
                        ID = objrequestSynch.ID,
                        UserID = userID,
                        Backend = objBackend
                    };
                    //create object and assign values to properties for RequestUpdateMsg class 
                    RequestUpdateMsg objRequestMsg = new RequestUpdateMsg()
                    {
                        ServiceLayerReqID = objrequestSynch.RowKey,
                        request = objRequest

                    };
                    lstRequestUpdateMsg.Add(objRequestMsg);
                }


                //create object and assign values to properties for UpdateTriggeringMsg class 
                UpdateTriggeringMsg ObjUTMsg = new UpdateTriggeringMsg()
                {
                    Users = null,
                    Requests = lstRequestUpdateMsg,
                    VIP = IsVIPFlag,
                    GetPDFs = IsGeneratePdfs

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
        public void UpdateUserBackends(List<UserUpdateMsg> lstUsers, bool vip, bool generatePDF, Nullable<DateTime> changeAfter, string queueName)
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
                        InsightLogger.TrackEvent(queueName + " , Action :: For each provided user in Update triggering message, Response :: User:" + userID);
                        //creating Backend agent request query i.e RequestsUpdateQuery format
                        BackendUser objUser = new BackendUser()
                        {
                            UserID = users.UserID,
                            UserName = users.UserName
                        };

                        foreach (Backend backend in lstbackends)
                        {
                            //Creating request update query which is input for backend agent requestupdateretrival api
                            InsightLogger.TrackEvent(queueName + " , Action :: For each backend in user, Response :: Backend: " + backend.BackendID + " ,User ::" + userID);
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
                            InsightLogger.TrackEvent(queueName + " , Action :: Prepare agent user message, Response :: message:" + backendUserQuery);
                            acknowledgment = string.Empty;
                            //initalize object for api service provider for callingt the web api
                            APIServiceProvider ObjserviceProvider = new APIServiceProvider();
                            //call backend agent api with backendID and input queue message and getting the acknowledgment from API
                            Task.Factory.StartNew(() =>
                            {
                                ObjserviceProvider.CallBackendAgent(backend.BackendID, backendUserQuery, CoreConstants.Category.User, queueName);
                            });
                            //if acknowledgment is not null or not empty then update the userbackend expected updatetime
                            //if (!string.IsNullOrEmpty(acknowledgment))
                            //{
                            //update ExpectedUpdateTime  with the help of update trigger Rule :: R3
                            UpdateUserBackendExpectedUpdateTime(backend.BackendID, userID, queueName);
                            //}

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
        public void UpdateRequests(List<RequestUpdateMsg> lstRequests, bool vip, bool generatePDF, Nullable<DateTime> changeAfter, string queueName)
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
                        InsightLogger.TrackEvent(queueName + " , Action :: For each backend in requests List, Response :: Backend ID: " + backendID);

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
                            InsightLogger.TrackEvent(queueName + " , Action :: Prepare agent requests message, Response :: message:" + requestsUpdateQuery);
                            acknowledgment = string.Empty;
                            //initalize object for api service provider for callingt the web api
                            APIServiceProvider ObjserviceProvider = new APIServiceProvider();
                            //call backend agent api with backendID and input queue message and getting the acknowledgment from API
                            //acknowledgment = 
                            Task.Factory.StartNew(() =>
                            {
                                ObjserviceProvider.CallBackendAgent(backendID, requestsUpdateQuery, CoreConstants.Category.Request, queueName);
                            });

                            //if acknowledgment is not null or not empty then update the userbackend expected updatetime
                            //if (!string.IsNullOrEmpty(acknowledgment))
                            //{
                            //update ExpectedUpdateTime  with the help of update trigger Rule :: R3
                            UpdateRequestExpectedUpdateTime(backendID, lstRequestsByBackend.ToList(), queueName);
                            //}
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
