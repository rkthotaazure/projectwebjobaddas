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
        /// <summary>
        /// BL method to add request entity into azure table
        /// </summary>
        /// <param name="request">takes request as input</param>
        public void AddUpdateRequest(BackendRequest backendrequest, string UserID, string backendId)
        {
            try
            {
                //generating request entity from input request obj by adding partitionkey and rowkey
                RequsetEntity requestentity = DataProvider.ResponseObjectMapper<RequsetEntity, Request>(backendrequest.requset);
                requestentity.PartitionKey = string.Concat(CoreConstants.AzureTables.RequestsPK, UserID);
                requestentity.RowKey = backendrequest.requset.id;
                //adding service layer requestid to entity                
                requestentity.serviceLayerReqID = backendrequest.serviceLayerReqID;
                requestentity.BackendID = backendId;
                //calling DAL method to add request entity
                RequestUpdateDAL requestupdatedal = new RequestUpdateDAL();                
                requestupdatedal.AddUpdateRequest(requestentity);
            }
            catch (DataAccessException DALexception)
            {
                throw DALexception;
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error in BL while inserting request : "
                       + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new BusinessLogicException();
            }
        }

        /// <summary>
        /// BL method to add approval entity into azure table
        /// </summary>
        /// <param name="request">takes request as input</param>
        public void AddUpdateApproval(Request request, string UserID, string backendId)
        {
            try
            {
                //generating approval entity from input request obj by adding partitionkey and rowkey
                ApprovalEntity approvalentity = new ApprovalEntity();
                approvalentity.PartitionKey = string.Concat(CoreConstants.AzureTables.ApprovalPK,UserID);
                approvalentity.RowKey = request.id;
                approvalentity.RequestId = request.id;
                approvalentity.status = CoreConstants.AzureTables.Waiting;
                approvalentity.BackendID = backendId;
                RequestUpdateDAL requestupdatedal = new RequestUpdateDAL();
                //calling DAL method to add request entity
                requestupdatedal.AddUpdateApproval(approvalentity);
            }
            catch (DataAccessException DALexception)
            {
                throw DALexception;
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error in BL while inserting Approval : "
                       + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
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
            try
            {
                List<ApproverEntity> approversListEntity = new List<ApproverEntity>();
                //generating approvers list entities by adding partitionkey and rowkey
                foreach (Approvers approver in approvers)
                {
                    ApproverEntity approverentity = DataProvider.ResponseObjectMapper<ApproverEntity, Approvers>(approver);
                    approverentity.PartitionKey = string.Concat(CoreConstants.AzureTables.ApproverPK, requsetid);
                    approverentity.RowKey = string.Concat(requsetid, CoreConstants.AzureTables.UnderScore, approver.order);
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
                LoggerHelper.WriteToLog(exception + " - Error in BL while inserting approvers : "
                       + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
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
            try
            {
                List<FieldEntity> listfiledsentity = new List<FieldEntity>();
                //generating fields list entities by adding partitionkey and rowkey
                foreach (Field field in genericInfoFields)
                {
                    FieldEntity fieldentity = DataProvider.ResponseObjectMapper<FieldEntity, Field>(field);
                    fieldentity.PartitionKey = string.Concat(CoreConstants.AzureTables.FieldPK, requestid);
                    fieldentity.RowKey = string.Concat(requestid, field.name);
                    listfiledsentity.Add(fieldentity);
                }
                //generating fields list entities by adding partitionkey and rowkey
                foreach (Field field in overviewFields)
                {
                    FieldEntity fieldentity = DataProvider.ResponseObjectMapper<FieldEntity, Field>(field);
                    fieldentity.PartitionKey = CoreConstants.AzureTables.FieldPK;
                    fieldentity.RowKey = string.Concat(requestid, field.name);
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
                LoggerHelper.WriteToLog(exception + " - Error in BL while inserting fields : "
                       + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new BusinessLogicException();
            }
        }

        /// <summary>
        /// method to add Request PDF to Blob
        /// </summary>
        /// <param name="urivalue">takes temp blob uri as input</param>
        /// <returns>returns uri of pdf stored in blob</returns>
        public Uri AddRequestPDFToBlob(Uri urivalue, string RequestID)
        {
            try
            {
                RequestUpdateDAL requestupdatedal = new RequestUpdateDAL();
                ////calling DAL method to add Requset PDF to blob
                Uri PDFuri = requestupdatedal.AddRequestPDFToBlob(urivalue, RequestID);
                return PDFuri;
            }
            catch (DataAccessException DALexception)
            {
                throw DALexception;
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error in BL while inserting pdf in blob : "
                       + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new BusinessLogicException();
            }
        }

        /// <summary>
        /// method to add Request PDF uri to Request entity
        /// </summary>
        /// <param name="urivalue">takes temp blob uri as input</param>        
        public void AddPDFUriToRequest(Uri urivalue, string UserId,string RequestID)
        {
            try
            {
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
                LoggerHelper.WriteToLog(exception + " - Error in BL while updating pdf uri in request entity : "
                       + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
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
            try
            {
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
                LoggerHelper.WriteToLog(exception + " - Error in BL while caliculating request size: "
                       + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
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
            try
            {
                //calling DAL method to add update userbackend entity
                RequestUpdateDAL requestupdatedal = new RequestUpdateDAL();
                UserBackendEntity userbackend = requestupdatedal.GetUserBackend(userId, BackendId);
                //calling methods to update Open requests, open approvals and urgent approvals
                userbackend.OpenRequests = requestupdatedal.GetOpenRequestsCount(userId, BackendId);
                userbackend.OpenApprovals = requestupdatedal.GetOpenApprovalsCount(userId, BackendId);
                userbackend.UrgentApprovals = requestupdatedal.GetUrgentApprovalsCount(userId, BackendId);
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
                LoggerHelper.WriteToLog(exception + " - Error in BL while updating userbackend: "
                       + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
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
        /// method to caliculate average sizes and latencies to update in userbackend
        /// </summary>
        /// <param name="BackendId">takes backendid as input</param>
        /// <param name="Totalrequestssize">takes totalrequestsize as input</param>
        /// <param name="TotalRequestlatency">takes total request latency as input</param>
        /// <param name="requestscount">takes request count as input</param>
        public void UpdateBackend(string BackendId, int Totalrequestssize, int TotalRequestlatency, int requestscount)
        {
            try
            {
                RequestUpdateDAL requestupdatedal = new RequestUpdateDAL();
                //calling dal method to get backend entity to be updated
                BackendEntity backend = requestupdatedal.GetBackend(BackendId);
                //updating average, last request sizes for backend
                backend.AverageRequestSize = GetAverage(backend.AverageRequestSize, backend.TotalRequestsCount, Totalrequestssize, requestscount);
                backend.LastRequestSize = Convert.ToInt32(Totalrequestssize / requestscount);
                //updating average, last request latencies for user backend
                backend.AverageRequestLatency = GetAverage(backend.AverageRequestLatency, backend.TotalRequestsCount, TotalRequestlatency, requestscount);
                backend.AverageAllRequestsLatency = GetAverage(backend.AverageAllRequestsLatency, backend.TotalBatchRequestsCount, TotalRequestlatency, requestscount);
                backend.LastRequestLatency = Convert.ToInt32(TotalRequestlatency / requestscount);
                backend.LastAllRequestsLatency = TotalRequestlatency;
                //updaing total requests per userbackend and total request batches/messages per userbackend
                backend.TotalRequestsCount = backend.TotalRequestsCount + requestscount;
                backend.TotalBatchRequestsCount = backend.TotalBatchRequestsCount + 1;
                //calling DAL method to update backend entity
                requestupdatedal.UpdateBackend(backend);
            }
            catch (DataAccessException DALexception)
            {
                throw DALexception;
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error in BL while updating userbackend: "
                       + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new BusinessLogicException();
            }
        }

    }
}
