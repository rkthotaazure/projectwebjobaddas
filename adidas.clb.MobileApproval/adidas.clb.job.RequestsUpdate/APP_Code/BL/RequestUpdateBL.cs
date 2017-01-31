//-----------------------------------------------------------
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
        static IAppInsight InsightLogger { get { return AppInsightLogger.Instance; } }
        /// <summary>
        /// BL method to add request entity into azure table
        /// </summary>
        /// <param name="request">takes request as input</param>
        public void AddUpdateRequest(BackendRequest backendrequest, string UserID, string backendId)
        {
            //Get Caller Method name
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                RequestUpdateDAL requestupdatedal = new RequestUpdateDAL();
                //get the request to update
                RequsetEntity existingrequest = requestupdatedal.GetRequest(string.Concat(CoreConstants.AzureTables.RequestsPK, UserID), backendrequest.RequestsList.ID);
                //if request exists update otherwise craete new request
                if(existingrequest!=null)
                {
                    existingrequest.Created = backendrequest.RequestsList.Created;
                    existingrequest.LastUpdate = DateTime.Now;
                    existingrequest.Status = backendrequest.RequestsList.Status;
                    existingrequest.Title = backendrequest.RequestsList.Title;
                    existingrequest.UpdateTriggered = false;
                    //calling DAL method to update request entity
                    requestupdatedal.AddUpdateRequest(existingrequest);
                }
                else
                {
                    //generating request entity from input request obj by adding partitionkey and rowkey
                    RequsetEntity requestentity = DataProvider.ResponseObjectMapper<RequsetEntity, Request>(backendrequest.RequestsList);
                    requestentity.PartitionKey = string.Concat(CoreConstants.AzureTables.RequestsPK, UserID);
                    requestentity.RowKey = backendrequest.RequestsList.ID;
                    //adding service layer requestid to entity                
                    requestentity.ServiceLayerReqID = backendrequest.ServiceLayerReqID;
                    requestentity.BackendID = backendId;
                    requestentity.UpdateTriggered = false;
                    requestentity.LastUpdate = DateTime.Now;
                    //add requester deatils to request entity
                    if (backendrequest.RequestsList.Requester != null)
                    {
                        requestentity.RequesterID = backendrequest.RequestsList.Requester.UserID;
                        requestentity.RequesterName = backendrequest.RequestsList.Requester.Name;
                    }
                    //calling DAL method to add request entity
                    requestupdatedal.AddUpdateRequest(requestentity);
                }                
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
        public void AddUpdateApproval(List<Approvers> approverslist, string requestid, string UserID, string backendId, int missingconfirmationlimit)
        {
            //Get Caller Method name
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                RequestUpdateDAL requestupdatedal = new RequestUpdateDAL();
                string partitionkey = string.Concat(CoreConstants.AzureTables.ApprovalPK, UserID);
                //get approval from service layer
                ApprovalEntity ServiceLayerApproval = requestupdatedal.GetApproval(partitionkey, requestid);
                //get the current approver from list of approvers got from backend
                Approvers approver = approverslist.Find(x => x.User.UserID == UserID);
                //check if approval exists or not
                if (ServiceLayerApproval != null)
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
                        ServiceLayerApproval.Missingconfirmations = ServiceLayerApproval.Missingconfirmations + 1;
                        ServiceLayerApproval.Backendoverwritten = false;
                        ServiceLayerApproval.BackendConfirmed = true;
                        //check for Missing confirmation limit
                        if (ServiceLayerApproval.Missingconfirmations > missingconfirmationlimit)
                        {
                            ServiceLayerApproval.Backendoverwritten = true;
                            ServiceLayerApproval.BackendConfirmed = true;
                            ServiceLayerApproval.Missingconfirmations = 0;
                            ServiceLayerApproval.Status = approver.Status;
                        }
                    }
                    //calling DAL method to add request entity
                    requestupdatedal.AddUpdateApproval(ServiceLayerApproval);
                }
                else
                {
                    //generating approval entity from input approver,request obj by adding partitionkey and rowkey
                    ApprovalEntity approvalentity = new ApprovalEntity();
                    approvalentity.PartitionKey = partitionkey;
                    approvalentity.RowKey = requestid;
                    approvalentity.RequestId = requestid;
                    string status = approver.Status;
                    if (string.IsNullOrEmpty(status))
                    {
                        status = CoreConstants.AzureTables.Waiting;
                    }
                    approvalentity.Status = status;
                    approvalentity.BackendID = backendId;
                    //calling DAL method to add request entity
                    requestupdatedal.AddUpdateApproval(approvalentity);
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
        public void UpdateUserBackend(string userId, string BackendId, int Totalrequestssize, int TotalRequestlatency, int requestscount)
        {
            //Get Caller Method name
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                //calling DAL method to add update userbackend entity
                RequestUpdateDAL requestupdatedal = new RequestUpdateDAL();
                UserBackendEntity userbackend = requestupdatedal.GetUserBackend(userId, BackendId);
                //calling methods to update Open requests, open approvals and urgent approvals
                userbackend.OpenRequests = requestupdatedal.GetOpenRequestsCount(userId, BackendId);
                userbackend.OpenApprovals = requestupdatedal.GetOpenApprovalsCount(userId, BackendId);
                userbackend.UrgentApprovals = requestupdatedal.GetUrgentApprovalsCount(userId, BackendId);
                //if request count more than one.
                if (requestscount > 0)
                {
                    //updating average, last request sizes for user backend 
                    userbackend.AverageRequestSize = GetAverage(userbackend.AverageRequestSize, userbackend.TotalRequestsCount, Totalrequestssize, requestscount);
                    userbackend.AverageAllRequestsSize = GetAverage(userbackend.AverageAllRequestsSize, userbackend.TotalBatchRequestsCount, Totalrequestssize, requestscount);
                    userbackend.LastRequestSize = Convert.ToInt32(Totalrequestssize / requestscount);
                    userbackend.LastAllRequestsSize = Totalrequestssize;
                    //updating average, last request latencies for user backend
                    userbackend.AverageRequestLatency = GetAverage(userbackend.AverageRequestLatency, userbackend.TotalRequestsCount, TotalRequestlatency, requestscount);
                    userbackend.AverageAllRequestsLatency = GetAverage(userbackend.AverageAllRequestsLatency, userbackend.TotalBatchRequestsCount, TotalRequestlatency, requestscount);
                    userbackend.LastRequestLatency = Convert.ToInt32(TotalRequestlatency / requestscount);
                    userbackend.LastAllRequestsLatency = TotalRequestlatency;
                    //updaing total requests per userbackend and total request batches/messages per userbackend
                    userbackend.TotalRequestsCount = userbackend.TotalRequestsCount + requestscount;
                    userbackend.TotalBatchRequestsCount = userbackend.TotalBatchRequestsCount + 1;
                }
                userbackend.LastUpdate = DateTime.Now;
                userbackend.UpdateTriggered = false;
                //calling DAL method to update userbackend
                requestupdatedal.UpdateUserBackend(userbackend);
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
                //if request count more than one.
                if (requestscount > 0)
                {
                    //updating average, last request sizes for backend
                    backend.AverageRequestSize = GetAverage(backend.AverageRequestSize, backend.TotalRequestsCount, Totalrequestssize, requestscount);
                    backend.LastRequestSize = Convert.ToInt32(Totalrequestssize / requestscount);
                    //updating average, last request latencies for backend
                    backend.AverageRequestLatency = GetAverage(backend.AverageRequestLatency, backend.TotalRequestsCount, TotalRequestlatency, requestscount);
                    backend.AverageAllRequestsLatency = GetAverage(backend.AverageAllRequestsLatency, backend.TotalBatchRequestsCount, TotalRequestlatency, requestscount);
                    backend.LastRequestLatency = Convert.ToInt32(TotalRequestlatency / requestscount);
                    backend.LastAllRequestsLatency = TotalRequestlatency;
                    //updaing total requests per userbackend and total request batches/messages per userbackend
                    backend.TotalRequestsCount = backend.TotalRequestsCount + requestscount;
                    backend.TotalBatchRequestsCount = backend.TotalBatchRequestsCount + 1;
                }
                //calling DAL method to update backend entity
                RequestUpdateDAL requestupdatedal = new RequestUpdateDAL();
                requestupdatedal.UpdateBackend(backend);
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

    }
}
