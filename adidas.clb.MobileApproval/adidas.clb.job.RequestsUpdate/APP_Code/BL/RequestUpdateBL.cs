﻿//-----------------------------------------------------------
// <copyright file="RequestUpdateBL.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using adidas.clb.job.RequestsUpdate.APP_Code.DAL;
using adidas.clb.job.RequestsUpdate.Exceptions;
using adidas.clb.job.RequestsUpdate.Models;
using adidas.clb.job.RequestsUpdate.Utility;

namespace adidas.clb.job.RequestsUpdate.APP_Code.BL
{
    /// <summary>
    /// class which implements methods for business logic layer of RequestUpdate.
    /// </summary>
    public class RequestUpdateBL
    {
        //Application insights interface reference for logging the error/ custom events details into Application Insight azure service.
        static IAppInsight InsightLogger { get { return AppInsightLogger.Instance; } }
        //Get TaskNotStartedStatus value from configuration file
        private static string taskNotStartedStatus = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["TaskNotStartedStatus"]);
        //Get TaskUnreadStatus value from configuration file
        private static string taskUnreadStatus = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["TaskUnreadStatus"]);
        //Get TaskReadStatus value from configuration file
        private static string taskreadStatus = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["TaskReadStatus"]);
        //Get  task waiting status value from configuration file
        private static string WaitingStatus = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["WaitingTaskStatusValue"]);
        /// <summary>
        /// BL method to add request entity into azure table
        /// </summary>
        /// <param name="request">takes request as input</param>
        public int AddUpdateRequest(BackendRequest backendrequest, string UserID, string backendId, DateTime requestUpdateMsgTriggerTimestamp, DateTime? utQueueEntryTimestamp)
        {
            //Get Caller Method name
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                RequestUpdateDAL requestupdatedal = new RequestUpdateDAL();
                int requestLatency = 0;
                //get the request to update
                RequsetEntity requestentity = null;
                requestentity = requestupdatedal.GetRequest(string.Concat(CoreConstants.AzureTables.RequestsPK, UserID), backendrequest.RequestsList.ID);
                //if request exists update otherwise craete new request
                if (requestentity != null)
                {
                    requestentity.Created = backendrequest.RequestsList.Created;
                    requestentity.LastUpdate = DateTime.Now;
                    requestentity.Status = backendrequest.RequestsList.Status;
                    requestentity.Title = backendrequest.RequestsList.Title;
                    requestentity.UpdateTriggered = false;
                    //set requestUpdateMsgTriggerTimestamp
                    requestentity.Request_ReqUpdateQueueMsgTriggerTimestamp = requestUpdateMsgTriggerTimestamp;
                    //set ResponseInsertIntostorageTimestamp
                    DateTime responseInsertIntostorageTimestamp = DateTime.Now;
                    requestentity.Request_ResponseInsertIntostorageTimestamp = responseInsertIntostorageTimestamp;
                    //calling DAL method to update request entity
                    requestupdatedal.AddUpdateRequest(requestentity);
                    //calculate request latency from Queue Entry to response inser into azure table storage
                    TimeSpan timeDiff = responseInsertIntostorageTimestamp - Convert.ToDateTime(requestentity.Request_QueueMsgEntryTimestamp);
                    requestLatency = (int)timeDiff.TotalMilliseconds;
                }
                else
                {
                    //generating request entity from input request obj by adding partitionkey and rowkey
                    requestentity = DataProvider.ResponseObjectMapper<RequsetEntity, Request>(backendrequest.RequestsList);
                    requestentity.PartitionKey = string.Concat(CoreConstants.AzureTables.RequestsPK, UserID);
                    requestentity.RowKey = backendrequest.RequestsList.ID;
                    //adding service layer requestid to entity                
                    requestentity.ServiceLayerReqID = backendrequest.ServiceLayerReqID;
                    requestentity.BackendID = backendId;
                    requestentity.UpdateTriggered = false;
                    requestentity.LastUpdate = DateTime.Now;
                    //set requestUpdate Message Trigger Timestamp value
                    requestentity.Request_ReqUpdateQueueMsgTriggerTimestamp = requestUpdateMsgTriggerTimestamp;
                    //set ResponseInsertIntostorageTimestamp
                    DateTime responseInsertIntostorageTimestamp = DateTime.Now;
                    requestentity.Request_ResponseInsertIntostorageTimestamp = responseInsertIntostorageTimestamp;
                    //add requester deatils to request entity
                    if (backendrequest.RequestsList.Requester != null)
                    {
                        requestentity.RequesterID = backendrequest.RequestsList.Requester.UserID;
                        requestentity.RequesterName = backendrequest.RequestsList.Requester.Name;
                    }
                    //calling DAL method to add request entity
                    requestupdatedal.AddUpdateRequest(requestentity);
                    //calculate request latency 
                    TimeSpan timeDiff = responseInsertIntostorageTimestamp - Convert.ToDateTime(utQueueEntryTimestamp);
                    requestLatency = (int)timeDiff.TotalMilliseconds;
                }
                return requestLatency;
            }
            catch (DataAccessException DALexception)
            {
                throw DALexception;
            }
            catch (Exception exception)
            {
                //write exception into application insights
                InsightLogger.Exception(exception.Message + " - Error in BL while inserting request", exception, callerMethodName);
                throw new BusinessLogicException();
            }
        }
        /// <summary>
        /// This method calculates the request latency
        /// </summary>
        /// <param name="reqEntity"></param>
        /// <param name="utQueueEntryTimestamp"></param>
        /// <returns></returns>
        public int AddUpdateRequestServiceLayerTimestamp(RequsetEntity reqEntity, DateTime? utQueueEntryTimestamp)
        {
            //Get Caller Method name
            string callerMethodName = string.Empty;
            try
            {
                //get request latency in milliseconds
                int requestLatency = 0;
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                DateTime? responseInsertIntostorageTimestamp = null;
                DateTime? msgEntryTimeStamp = null;
                RequestUpdateDAL requestupdatedal = new RequestUpdateDAL();
                //get the request to update
                RequsetEntity existingrequest = requestupdatedal.GetRequest(reqEntity.PartitionKey, reqEntity.RowKey);
                //if request exists update otherwise craete new request
                if (existingrequest != null)
                {
                    //set ResponseInsertIntostorageTimestamp
                    responseInsertIntostorageTimestamp = DateTime.Now;
                    msgEntryTimeStamp = existingrequest.Request_QueueMsgEntryTimestamp;
                    existingrequest.Request_ResponseInsertIntostorageTimestamp = responseInsertIntostorageTimestamp;
                    //calling DAL method to add request entity
                    requestupdatedal.AddUpdateRequest(existingrequest);
                }
                else
                {
                    msgEntryTimeStamp = utQueueEntryTimestamp;
                }

                //calculate request latency 
                TimeSpan timeDiff = Convert.ToDateTime(responseInsertIntostorageTimestamp) - Convert.ToDateTime(msgEntryTimeStamp);
                requestLatency = timeDiff.Milliseconds;
                return requestLatency;
            }
            catch (DataAccessException DALexception)
            {
                throw DALexception;
            }
            catch (Exception exception)
            {
                //write exception into application insights
                InsightLogger.Exception(exception.Message + " - Error in BL while inserting request", exception, callerMethodName);
                throw new BusinessLogicException();
            }
        }
        /// <summary>
        /// BL method to add approval entity into azure table
        /// </summary>
        /// <param name="request">takes request as input</param>
        public void AddUpdateApproval(List<Approvers> approverslist, string requestid, string UserID, string backendId, int missingconfirmationlimit, string requestTitle)
        {
            //Get Caller Method name
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                RequestUpdateDAL requestupdatedal = new RequestUpdateDAL();

                //get the user tasks  from list of approvers got from backend
                List<Approvers> lstapprover = (from apr in approverslist
                                               where apr.User.UserID.Equals(UserID) && (!string.IsNullOrEmpty(apr.Status) && apr.Status != taskNotStartedStatus)
                                               select apr).ToList();

                if (lstapprover != null && lstapprover.Count > 0)
                {

                    foreach (Approvers approver in lstapprover)
                    {
                        string partitionkey = string.Concat(CoreConstants.AzureTables.ApprovalPK, UserID);
                        string approverOrder = Convert.ToString(approver.Order);
                        string serviceLayerTaskID = requestid + "_" + approverOrder;
                        //get approval from service layer
                        ApprovalEntity ServiceLayerApproval = requestupdatedal.GetApproval(partitionkey, serviceLayerTaskID);
                        //get the current approver from list of approvers got from backend
                        // Approvers approver = approverslist.Find(x => x.User.UserID == UserID);
                        //check if approval exists or not
                        if (ServiceLayerApproval != null)
                        {
                            if (!ServiceLayerApproval.BackendConfirmed)
                            {
                                //check for Status Confirmation and update approval flags
                                if (ServiceLayerApproval.Status == approver.Status)
                                {
                                    ServiceLayerApproval.Backendoverwritten = false;
                                    ServiceLayerApproval.BackendConfirmed = true;
                                    ServiceLayerApproval.Missingconfirmations = 0;
                                }
                                else
                                {
                                    //increase Missingconfirmation limit as status is not matching
                                    ServiceLayerApproval.Missingconfirmations = ServiceLayerApproval.Missingconfirmations + 1;
                                    ServiceLayerApproval.Backendoverwritten = false;
                                    ServiceLayerApproval.BackendConfirmed = false;
                                    ServiceLayerApproval.TaskViewStatus = taskreadStatus;
                                    //Add message to update triggering VIP queue to trigger request update.
                                    RequestUpdateTrigger(requestid, UserID, backendId);
                                    //InsightLogger.TrackSpecificEvent("Missing Conformation:" + missingconfirmationlimit);
                                    //check for Missing confirmation limit
                                    if (ServiceLayerApproval.Missingconfirmations > missingconfirmationlimit)
                                    {
                                        ServiceLayerApproval.Backendoverwritten = true;
                                        ServiceLayerApproval.BackendConfirmed = true;
                                        ServiceLayerApproval.Missingconfirmations = 0;
                                        ServiceLayerApproval.Status = approver.Status;
                                        
                                    }
                                }
                            }
                            else
                            {
                                //if user approve /reject the unread task from backend application then need to update the task view status as read
                                if (approver.Status != WaitingStatus)
                                {
                                    ServiceLayerApproval.TaskViewStatus = taskreadStatus;
                                }
                                ServiceLayerApproval.Status = approver.Status;
                            }
                            ServiceLayerApproval.DueDate = approver.DueDate;
                            ServiceLayerApproval.Created = approver.Created;
                            ServiceLayerApproval.DecisionDate = approver.DecisionDate;

                            ////temp
                            //if (ServiceLayerApproval.Status == CoreConstants.AzureTables.Waiting && string.IsNullOrEmpty(ServiceLayerApproval.TaskViewStatus))
                            //{
                            //    ServiceLayerApproval.TaskViewStatus = taskUnreadStatus;
                            //}
                            //calling DAL method to add request entity
                            requestupdatedal.AddUpdateApproval(ServiceLayerApproval);

                        }
                        else
                        {
                            //generating approval entity from input approver,request obj by adding partitionkey and rowkey
                            ApprovalEntity approvalentity = new ApprovalEntity();
                            approvalentity.PartitionKey = partitionkey;
                            approvalentity.RowKey = serviceLayerTaskID;
                            approvalentity.RequestId = requestid;
                            string status = approver.Status;
                            //if (string.IsNullOrEmpty(status))
                            //{
                            //    status = CoreConstants.AzureTables.Waiting;
                            //}
                            approvalentity.Status = status;
                            approvalentity.BackendID = backendId;
                            approvalentity.ServiceLayerTaskID = serviceLayerTaskID;
                            approvalentity.TaskTitle = requestTitle;
                            approvalentity.Created = approver.Created;
                            approvalentity.DueDate = approver.DueDate;
                            //if it is new task then set task view status as "New"
                            approvalentity.TaskViewStatus = taskUnreadStatus;
                            //calling DAL method to add request entity
                            requestupdatedal.AddUpdateApproval(approvalentity);

                        }
                    }
                }


            }
            catch (DataAccessException DALexception)
            {
                throw DALexception;
            }
            catch (Exception exception)
            {
                //write exception into application insights
                InsightLogger.Exception(exception.Message + " - Error in BL while inserting Approval", exception, callerMethodName);
                throw new BusinessLogicException();
            }
        }

        /// <summary>
        /// BL method to add approvers entities into azure table
        /// </summary>
        /// <param name="approvers">takes approvers as input</param>
        /// <param name="requsetid">takes requestid as input</param>
        public void AddUpdateApprovers(List<Approvers> approvers, string requsetid)
        {
            //Get Caller Method name
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                List<ApproverEntity> approversListEntity = new List<ApproverEntity>();
                //generating approvers list entities by adding partitionkey and rowkey
                foreach (Approvers approver in approvers)
                {
                    ApproverEntity approverentity = DataProvider.ResponseObjectMapper<ApproverEntity, Approvers>(approver);
                    //add approver userID and userName to entity
                    approverentity.UserID = approver.User.UserID;
                    approverentity.UserName = approver.User.UserName;
                    approverentity.PartitionKey = string.Concat(CoreConstants.AzureTables.ApproverPK, requsetid);
                    approverentity.RowKey = string.Concat(requsetid, CoreConstants.AzureTables.UnderScore, approver.Order);
                    approverentity.Comment = approver.ApproverComment;
                    approversListEntity.Add(approverentity);
                }
                RequestUpdateDAL requestupdatedal = new RequestUpdateDAL();
                //calling dal method to remove existing approvers entities
                requestupdatedal.RemoveExistingApprovers(requsetid);
                //calling dal method to add approvers entities
                requestupdatedal.AddApprovers(approversListEntity);
            }
            catch (DataAccessException DALexception)
            {
                throw DALexception;
            }
            catch (Exception exception)
            {
                //write exception into application insights
                InsightLogger.Exception(exception.Message + " - Error in BL while inserting Approvers", exception, callerMethodName);
                throw new BusinessLogicException();
            }
        }

        /// <summary>
        /// BL method to fields entities into azure table
        /// </summary>
        /// <param name="genericInfoFields">takes generic fileds as input</param>
        /// <param name="overviewFields">takes over fields as input</param>
        /// <param name="requestid">takes requestid as input</param>
        public void AddUpdateFields(List<Field> genericInfoFields, List<Field> overviewFields, string requestid)
        {
            //Get Caller Method name
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                List<FieldEntity> listfiledsentity = new List<FieldEntity>();
                //generating fields list entities by adding partitionkey and rowkey
                foreach (Field field in genericInfoFields)
                {
                    FieldEntity fieldentity = DataProvider.ResponseObjectMapper<FieldEntity, Field>(field);
                    fieldentity.PartitionKey = string.Concat(CoreConstants.AzureTables.FieldPK, requestid);
                    fieldentity.RowKey = string.Concat(requestid, CoreConstants.AzureTables.UnderScore, field.Name);
                    listfiledsentity.Add(fieldentity);
                }
                //generating fields list entities by adding partitionkey and rowkey
                foreach (Field field in overviewFields)
                {
                    FieldEntity fieldentity = DataProvider.ResponseObjectMapper<FieldEntity, Field>(field);
                    fieldentity.PartitionKey = string.Concat(CoreConstants.AzureTables.FieldPK, requestid);
                    fieldentity.RowKey = string.Concat(requestid, CoreConstants.AzureTables.UnderScore, field.Name);
                    listfiledsentity.Add(fieldentity);
                }
                RequestUpdateDAL requestupdatedal = new RequestUpdateDAL();
                //calling dal method to remove existing field entities
                requestupdatedal.RemoveExistingFields(requestid);
                //calling DAL method to add fields entities
                requestupdatedal.AddFields(listfiledsentity);
            }
            catch (DataAccessException DALexception)
            {
                throw DALexception;
            }
            catch (Exception exception)
            {
                //write exception into application insights
                InsightLogger.Exception(exception.Message + " - Error in BL while inserting fields", exception, callerMethodName);
                throw new BusinessLogicException();
            }
        }

        /// <summary>
        /// method to add Request PDF uri to Request entity
        /// </summary>
        /// <param name="urivalue">takes temp blob uri as input</param>        
        public void AddPDFUriToRequest(Uri urivalue, string UserId, string RequestID)
        {
            //Get Caller Method name
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                RequestUpdateDAL requestupdatedal = new RequestUpdateDAL();
                //calling DAL method to add Requset PDF uri to request entity
                requestupdatedal.AddPDFUriToRequest(urivalue, UserId, RequestID);
            }
            catch (DataAccessException DALexception)
            {
                throw DALexception;
            }
            catch (Exception exception)
            {
                //write exception into application insights
                InsightLogger.Exception(exception.Message + " - Error in BL while updating pdf uri in request entity", exception, callerMethodName);
                throw new BusinessLogicException();
            }
        }

        /// <summary>
        /// method to caliculate physical size of request object
        /// </summary>
        /// <param name="backendrequest">takes request as input</param>
        /// <returns>returns size of request object</returns>
        public int CalculateRequestSize(BackendRequest backendrequest)
        {
            //Get Caller Method name
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                //converting object to stream
                using (Stream stream = new MemoryStream())
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    //converting stream to binary formatter to get size
                    formatter.Serialize(stream, backendrequest);
                    return Convert.ToInt32(stream.Length); ;
                }
            }
            catch (DataAccessException DALexception)
            {
                throw DALexception;
            }
            catch (Exception exception)
            {
                //write exception into application insights
                InsightLogger.Exception(exception.Message + " - Error in BL while caliculating request size", exception, callerMethodName);
                throw new BusinessLogicException();
            }
        }

        /// <summary>
        /// method to caliculate average sizes and latencies to update in userbackend
        /// </summary>
        /// <param name="userId">takes userid as input</param>
        /// <param name="BackendId">takes backendid as input</param>
        /// <param name="Totalrequestssize">takes totalrequestsize as input</param>
        /// <param name="TotalRequestlatency">takes total request latency as input</param>
        /// <param name="requestscount">takes request count as input</param>
        public void UpdateUserBackend(string userId, string BackendId, int Totalrequestssize, int TotalRequestlatency, int requestscount, DateTime reqUpdateMsgTriggerTimestamp, DateTime serviceLayerUpdateTimestamp)
        {
            //Get Caller Method name
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                //create object for RequestUpdateDAL class
                //calling DAL method to add update userbackend entity
                RequestUpdateDAL requestupdatedal = new RequestUpdateDAL();
                UserBackendEntity userbackend = requestupdatedal.GetUserBackend(userId, BackendId);
                if (userbackend != null)
                {
                    //set Request Update Message Trigger Timestamp value
                    userbackend.RequestUpdateMsgTriggerTimestamp = reqUpdateMsgTriggerTimestamp;
                    //set Response Insert Into storage Timestamp value
                    userbackend.ResponseInsertIntostorageTimestamp = serviceLayerUpdateTimestamp;
                    //calling methods to update Open requests, open approvals and urgent approvals
                    userbackend.OpenRequests = requestupdatedal.GetOpenRequestsCount(userId, BackendId);
                    userbackend.OpenApprovals = requestupdatedal.GetOpenApprovalsCount(userId, BackendId);
                    userbackend.UrgentApprovals = requestupdatedal.GetUrgentApprovalsCount(userId, BackendId);
                    //declare int variable for calculating average request latency
                    int averageRequestLatency = 0;
                    //if request count more than one.
                    if (requestscount > 0)
                    {
                        //updating average, last request sizes for user backend 
                        userbackend.AverageRequestSize = GetAverage(userbackend.AverageRequestSize, userbackend.TotalRequestsCount, Totalrequestssize, requestscount);
                        userbackend.AverageAllRequestsSize = GetAverage(userbackend.AverageAllRequestsSize, userbackend.TotalBatchRequestsCount, Totalrequestssize, requestscount);
                        userbackend.LastRequestSize = Convert.ToInt32(Totalrequestssize / requestscount);
                        userbackend.LastAllRequestsSize = Totalrequestssize;
                        //updating average, last request latencies for user backend
                        averageRequestLatency = GetAverage(userbackend.AverageRequestLatency, userbackend.TotalRequestsCount, TotalRequestlatency, requestscount);
                        //if average requestlatency is zero then set TotalRequestlatency vlaue as average request latency
                        userbackend.AverageRequestLatency = (averageRequestLatency > 0) ? averageRequestLatency : TotalRequestlatency;
                        userbackend.AverageAllRequestsLatency = GetAverage(userbackend.AverageAllRequestsLatency, userbackend.TotalBatchRequestsCount, TotalRequestlatency, requestscount);
                        userbackend.LastRequestLatency = Convert.ToInt32(TotalRequestlatency / requestscount);
                        userbackend.LastAllRequestsLatency = TotalRequestlatency;
                        //updaing total requests per userbackend and total request batches/messages per userbackend
                        userbackend.TotalRequestsCount = userbackend.TotalRequestsCount + requestscount;
                        userbackend.TotalBatchRequestsCount = userbackend.TotalBatchRequestsCount + 1;
                    }
                    //calculate latency from update triggering to service layer
                    TimeSpan latencyDiff = serviceLayerUpdateTimestamp - Convert.ToDateTime(userbackend.QueueMsgEntryTimestamp);
                    userbackend.LastPullLatency = (int)latencyDiff.TotalMilliseconds;
                    userbackend.LastUpdate = DateTime.Now;
                    userbackend.UpdateTriggered = false;
                    //calling DAL method to update userbackend
                    requestupdatedal.UpdateUserBackend(userbackend);
                }
            }
            catch (DataAccessException DALexception)
            {
                throw DALexception;
            }
            catch (Exception exception)
            {
                //write exception into application insights
                InsightLogger.Exception(exception.Message + " - Error in BL while updating userbackend", exception, callerMethodName);
                throw new BusinessLogicException();
            }
        }

        /// <summary>
        /// method to caliculate average
        /// </summary>
        /// <param name="existingaverage">takes as existingaverage input</param>
        /// <param name="existingcount">takes as existingcount input</param>
        /// <param name="currentsize">takes as currentsize input</param>
        /// <param name="currentcount">takes as currentcount input</param>
        /// <returns>returns average</returns>
        public int GetAverage(int existingaverage, int existingcount, int currentsize, int currentcount)
        {
            float currentaverage = (currentsize + (existingaverage * existingcount)) / (existingcount + currentcount);
            return Convert.ToInt32(currentaverage);
        }

        /// <summary>
        /// method to get backend entity
        /// </summary>
        /// <param name="backendid">takes backend id as input</param>
        /// <returns>returns backend entity</returns>
        public BackendEntity Getbackend(string backendid)
        {
            RequestUpdateDAL requestupdatedal = new RequestUpdateDAL();
            //calling dal method to get backend entity to be updated
            BackendEntity backend = requestupdatedal.GetBackend(backendid);
            return backend;
        }

        /// <summary>
        /// method to caliculate average sizes and latencies to update in userbackend
        /// </summary>
        /// <param name="BackendId">takes backendid as input</param>
        /// <param name="Totalrequestssize">takes totalrequestsize as input</param>
        /// <param name="TotalRequestlatency">takes total request latency as input</param>
        /// <param name="requestscount">takes request count as input</param>
        public void UpdateBackend(BackendEntity backend, int Totalrequestssize, int TotalRequestlatency, int requestscount)
        {
            //Get Caller Method name
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                //declare int local variable for getting avg Request Latency
                int avgRequestLatency = 0;
                //declare string variable for converting backend entity into json string
                //string backendjsonstring = Newtonsoft.Json.JsonConvert.SerializeObject(backend);
                string backendid = backend.BackendID;
                //if request count greathan zero.
                if (requestscount > 0)
                {
                    //updating average, last request sizes for backend
                    InsightLogger.TrackSpecificEvent("RequestUpdateWebJob ::  action:Calculate  backend(" + backendid + ") AverageRequestSize , existing AverageRequestSize:" + backend.AverageRequestSize + " Exisiting Total Request Count :" + backend.TotalRequestsCount + ",current batch TotalRequestsSize: " + Totalrequestssize + "current batch requsetcount: " + requestscount);
                    backend.AverageRequestSize = GetAverage(backend.AverageRequestSize, backend.TotalRequestsCount, Totalrequestssize, requestscount);
                    backend.LastRequestSize = Convert.ToInt32(Totalrequestssize / requestscount);
                    //updating average, last request latencies for backend
                    InsightLogger.TrackSpecificEvent("RequestUpdateWebJob ::  action:Calculate  backend(" + backendid + ") AverageRequestLatency , existing AverageRequestLatency:" + backend.AverageRequestLatency + " Exisiting Total Request Count :" + backend.TotalRequestsCount + ",current batch TotalRequestsLatency: " + TotalRequestlatency + "current batch requsetcount: " + requestscount);
                    avgRequestLatency = GetAverage(backend.AverageRequestLatency, backend.TotalRequestsCount, TotalRequestlatency, requestscount);
                    //if avgRequestLatency is lessthan or equal to zero then set current Request Latency value to AverageRequestLatency
                    backend.AverageRequestLatency = (avgRequestLatency > 0) ? avgRequestLatency : TotalRequestlatency;
                    InsightLogger.TrackSpecificEvent("RequestUpdateWebJob ::  action:Calculate  backend(" + backendid + ") AverageAllRequestsLatency, existing AverageALLRequestLatency:" + backend.AverageAllRequestsLatency + " TotalBatchRequestsCount: " + backend.TotalBatchRequestsCount + "current batch TotalRequestsLatency: " + TotalRequestlatency + "current batch requsetcount: " + requestscount);
                    backend.AverageAllRequestsLatency = GetAverage(backend.AverageAllRequestsLatency, backend.TotalBatchRequestsCount, TotalRequestlatency, requestscount);
                    backend.LastRequestLatency = Convert.ToInt32(TotalRequestlatency / requestscount);
                    backend.LastAllRequestsLatency = TotalRequestlatency;
                    //updaing total requests per userbackend and total request batches/messages per userbackend
                    backend.TotalRequestsCount = backend.TotalRequestsCount + requestscount;
                    backend.TotalBatchRequestsCount = backend.TotalBatchRequestsCount + 1;

                    InsightLogger.TrackSpecificEvent("RequestUpdateWebJob ::  action: backend(" + backendid + ") updated AverageRequestSize:" + backend.AverageRequestSize);
                    InsightLogger.TrackSpecificEvent("RequestUpdateWebJob ::  action: backend(" + backendid + ") updated LastRequestSize:" + backend.LastRequestSize);
                    InsightLogger.TrackSpecificEvent("RequestUpdateWebJob :: action: backend(" + backendid + ") Updated AverageRequestLatency:" + backend.AverageRequestLatency);
                    InsightLogger.TrackSpecificEvent("RequestUpdateWebJob :: action: backend(" + backendid + ") updated AverageAllRequestsLatency :" + backend.AverageAllRequestsLatency);
                    InsightLogger.TrackSpecificEvent("RequestUpdateWebJob ::  action: backend(" + backendid + ") updated LastRequestLatency:" + backend.LastRequestLatency);
                    InsightLogger.TrackSpecificEvent("RequestUpdateWebJob :: action:  backend(" + backendid + ") updated LastAllRequestsLatency:" + backend.LastAllRequestsLatency);

                    //calling DAL method to update backend entity
                    RequestUpdateDAL requestupdatedal = new RequestUpdateDAL();
                    requestupdatedal.UpdateBackend(backend);                   
                }
                
            }
            catch (DataAccessException DALexception)
            {
                throw DALexception;
            }
            catch (Exception exception)
            {
                //write exception into application insights
                InsightLogger.Exception(exception.Message + " - Error in BL while updating backend", exception, callerMethodName);
                throw new BusinessLogicException();
            }
        }

        /// <summary>
        /// method to add message to queue to trigger request update
        /// </summary>
        /// <param name="requestID">takes requestid as input</param>
        /// <param name="UserID">takes userid as input</param>
        /// <param name="BackendID">takes backendid as input</param>
        public void RequestUpdateTrigger(string requestID, string UserID, string BackendID)
        {
            try
            {
                UpdateTriggeringMessage updateTriggerMessage = new UpdateTriggeringMessage();
                List<RequestUpdateMsg> updatetriggerrequestlist = new List<RequestUpdateMsg>();
                RequestUpdateMsg triggerrequset = new RequestUpdateMsg();
                //adding request to message object
                RequestMsg requestobj = new RequestMsg();
                requestobj.ID = requestID;
                requestobj.UserID = UserID;
                triggerrequset.request = requestobj;
                //adding backend to queue message
                UpdateTriggerBackend backendobj = new UpdateTriggerBackend();
                backendobj.BackendID = BackendID;
                triggerrequset.request.Backend = backendobj;
                //add requests to list which will be added to message
                updatetriggerrequestlist.Add(triggerrequset);
                updateTriggerMessage.Requests = updatetriggerrequestlist;
                //set VIP message flag to true
                updateTriggerMessage.VIP = true;
                //calling DAL method to update backend entity
                RequestUpdateDAL requestupdatedal = new RequestUpdateDAL();
                requestupdatedal.AddUpdateTriggerMessageToQueue(updateTriggerMessage);

            }
            catch (DataAccessException DALexception)
            {
                throw DALexception;
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error in BL while formatting updatetriggering message : "
                       + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new BusinessLogicException();
            }
        }

    }
}
