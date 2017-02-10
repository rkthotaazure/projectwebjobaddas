//-----------------------------------------------------------
// <copyright file="RequestUpdateDAL.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using adidas.clb.job.RequestsUpdate.Exceptions;
using adidas.clb.job.RequestsUpdate.Models;
using adidas.clb.job.RequestsUpdate.Utility;
using Newtonsoft.Json;

namespace adidas.clb.job.RequestsUpdate.APP_Code.DAL
{
    /// <summary>
    /// class which implements methods for data access layer of RequestUpdate.
    /// </summary>
    public class RequestUpdateDAL
    {
        static IAppInsight InsightLogger { get { return AppInsightLogger.Instance; } }
        /// <summary>
        /// method to add/upadte Request entity to azure table
        /// </summary>
        /// <param name="request">takes request entity as input</param>
        public void AddUpdateRequest(RequsetEntity request)
        {
            //Get Caller Method name
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                //call dataprovider method to insert entity into azure table
                DataProvider.InsertReplaceEntity<RequsetEntity>(CoreConstants.AzureTables.RequestTransactions, request);
            }
            catch (Exception exception)
            {
                //write exception into application insights
                InsightLogger.Exception(exception.Message + " - Error while inserting request into requestTransactions azure table in DAL", exception, callerMethodName);                
                throw new DataAccessException();
            }
        }

        /// <summary>
        /// method to get existing request
        /// </summary>
        /// <param name="partitionkey">takes partionkey as input</param>
        /// <param name="rowkey">takes row key as input</param>
        /// <returns>returns request entity</returns>
        public RequsetEntity GetRequest(string partitionkey,string rowkey)
        {
            //Get Caller Method name
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                //call dataprovider method to insert entity into azure table
                return DataProvider.Retrieveentity<RequsetEntity>(CoreConstants.AzureTables.RequestTransactions, partitionkey, rowkey);
            }
            catch (Exception exception)
            {
                //write exception into application insights
                InsightLogger.Exception(exception.Message + " - Error while retrieving request from requestTransactions azure table in DAL", exception, callerMethodName);
                throw new DataAccessException();
            }
        }

        /// <summary>
        /// method to get Approval entity
        /// </summary>
        /// <param name="partitionkey">takes partitionkey for approval entity as input</param>
        /// <param name="rowkey">takes row key for approval entity as input</param>
        /// <returns>returns approval entity</returns>
        public ApprovalEntity GetApproval(string partitionkey, string rowkey)
        {
            //Get Caller Method name
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                //call dataprovider method to insert entity into azure table
                return DataProvider.Retrieveentity<ApprovalEntity>(CoreConstants.AzureTables.RequestTransactions, partitionkey,rowkey);
            }
            catch (Exception exception)
            {
                //write exception into application insights
                InsightLogger.Exception(exception.Message + " - Error while getting approval from requestTransactions azure table in DAL ", exception, callerMethodName);
                throw new DataAccessException();
            }
        }

        /// <summary>
        /// method to add/upadte Request entity to azure table
        /// </summary>
        /// <param name="approval">takes approval entity as input</param>
        public void AddUpdateApproval(ApprovalEntity approval)
        {
            //Get Caller Method name
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                //call dataprovider method to insert entity into azure table
                DataProvider.InsertReplaceEntity<ApprovalEntity>(CoreConstants.AzureTables.RequestTransactions, approval);
            }
            catch (Exception exception)
            {
                //write exception into application insights
                InsightLogger.Exception(exception.Message + " - Error while inserting approval into requestTransactions azure table in DAL ", exception, callerMethodName);                
                throw new DataAccessException();
            }
        }

        /// <summary>
        /// method to add approver entities to azure table
        /// </summary>
        /// <param name="approvers">takes approver entities as input</param>
        public void AddApprovers(List<ApproverEntity> approvers)
        {
            //Get Caller Method name
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                //call dataprovider method to add entities to azure table
                DataProvider.AddEntities(CoreConstants.AzureTables.RequestTransactions, approvers);
            }
            catch (Exception exception)
            {
                //write exception into application insights
                InsightLogger.Exception(exception.Message + " - Error while inserting approvers into requestTransactions azure table in DAL ", exception, callerMethodName);                
                throw new DataAccessException();
            }
        }

        /// <summary>
        /// method to remove existing approvers
        /// </summary>
        /// <param name="requestid"></param>
        public void RemoveExistingApprovers(string requestid)
        {
            //Get Caller Method name
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                //generate query to retrive approvers 
                TableQuery<ApproverEntity> query = new TableQuery<ApproverEntity>().Where(TableQuery.GenerateFilterCondition(CoreConstants.AzureTables.PartitionKey, QueryComparisons.Equal, string.Concat(CoreConstants.AzureTables.ApproverPK,requestid)));
                //call dataprovider method to get entities from azure table
                List<ApproverEntity> existingapprovers = DataProvider.GetEntitiesList<ApproverEntity>(CoreConstants.AzureTables.RequestTransactions, query);
                //call dataprovider method to remove entities from azure table
                if(existingapprovers != null && existingapprovers.Count>0)
                {
                    DataProvider.RemoveEntities(CoreConstants.AzureTables.RequestTransactions, existingapprovers);
                }                
            }
            catch (Exception exception)
            {
                //write exception into application insights
                InsightLogger.Exception(exception.Message + " - Error while remove existing approvers into requestTransactions azure table in DAL ", exception, callerMethodName);                
                throw new DataAccessException();
            }
        }

        /// <summary>
        /// method to add fields entities to azure table
        /// </summary>
        /// <param name="fields">takes fields entities as input</param>
        public void AddFields(List<FieldEntity> fields)
        {
            //Get Caller Method name
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                //call dataprovider method to add entities to azure table
                DataProvider.AddEntities(CoreConstants.AzureTables.RequestTransactions, fields);
            }
            catch (Exception exception)
            {
                //write exception into application insights
                InsightLogger.Exception(exception.Message + " - Error while inserting fields into requestTransactions azure table in DAL", exception, callerMethodName);                
                throw new DataAccessException();
            }
        }

        public void RemoveExistingFields(string requestid)
        {
            //Get Caller Method name
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                //generate query to retrive existing fileds 
                TableQuery<FieldEntity> query = new TableQuery<FieldEntity>().Where(TableQuery.GenerateFilterCondition(CoreConstants.AzureTables.PartitionKey, QueryComparisons.Equal, string.Concat(CoreConstants.AzureTables.FieldPK, requestid)));
                //call dataprovider method to get entities from azure table
                List<FieldEntity> existingfields = DataProvider.GetEntitiesList<FieldEntity>(CoreConstants.AzureTables.RequestTransactions, query);
                //call dataprovider method to remove entities from azure table
                if (existingfields != null && existingfields.Count > 0)
                {
                    DataProvider.RemoveEntities(CoreConstants.AzureTables.RequestTransactions, existingfields);
                }                
            }
            catch (Exception exception)
            {
                //write exception into application insights
                InsightLogger.Exception(exception.Message + " - Error while removing existing fields into requestTransactions azure table in DAL", exception, callerMethodName);                
                throw new DataAccessException();
            }
        }        

        /// <summary>
        /// method to add Request PDF uri to Request entity
        /// </summary>
        /// <param name="urivalue">takes temp blob uri as input</param>        
        public void AddPDFUriToRequest(Uri urivalue, string userID,string RequestID)
        {
            //Get Caller Method name
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                //call dataprovider method to get entities from azure table
                RequsetEntity updateEntity = DataProvider.Retrieveentity<RequsetEntity>(CoreConstants.AzureTables.RequestTransactions, string.Concat(CoreConstants.AzureTables.RequestsPK, userID), RequestID);
                //check for null
                if (updateEntity != null)
                {
                    // Add the PDFUri.
                    updateEntity.PDFUri = urivalue.ToString();
                    //call dataprovider method to update entity to azure table
                    DataProvider.UpdateEntity<RequsetEntity>(CoreConstants.AzureTables.RequestTransactions, updateEntity);
                }

            }
            catch (Exception exception)
            {
                //write exception into application insights
                InsightLogger.Exception(exception.Message + " - Error while updating request update PDF uri into request entitty in DAL", exception, callerMethodName);                
                throw new DataAccessException();
            }
        }

        /// <summary>
        /// method to get get user backend
        /// </summary>
        /// <param name="userid">takes userid as input</param>
        /// <param name="backendid">takes backendid as input</param>
        /// <returns>returns user backend</returns>
        public UserBackendEntity GetUserBackend(string userid, string backendid)
        {
            //Get Caller Method name
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                //call dataprovider method to get entities from azure table
                UserBackendEntity userbackend = DataProvider.Retrieveentity<UserBackendEntity>(CoreConstants.AzureTables.UserDeviceConfiguration, string.Concat(CoreConstants.AzureTables.UserBackendPK, userid), backendid);                
                return userbackend;
            }
            catch (Exception exception)
            {
                //write exception into application insights
                InsightLogger.Exception(exception.Message + " - Error while getting userbackend entity in DAL", exception, callerMethodName);                
                throw new DataAccessException();
            }
        }

        /// <summary>
        /// method to get open requests count for user backend
        /// </summary>
        /// <param name="userid">takes userid as input</param>
        /// <param name="backendid">takes backendid as input</param>
        /// <returns>returns count of open requets per user backend</returns>
        public int GetOpenRequestsCount(string userid, string backendid)
        {
            //Get Caller Method name
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                //adding filters to get count of open requests
                string partitionFilter = TableQuery.GenerateFilterCondition(CoreConstants.AzureTables.PartitionKey, QueryComparisons.Equal, string.Concat(CoreConstants.AzureTables.RequestsPK, userid));
                string rowfilter = TableQuery.GenerateFilterCondition(CoreConstants.AzureTables.BackendId, QueryComparisons.Equal, backendid);
                string statusfilter = TableQuery.GenerateFilterCondition(CoreConstants.AzureTables.Status, QueryComparisons.Equal, CoreConstants.AzureTables.InProgress);
                string finalFilter = TableQuery.CombineFilters(TableQuery.CombineFilters(partitionFilter, TableOperators.And, rowfilter), TableOperators.And, statusfilter);
                //selecting only few columnas as we are just caliculating count of open requests
                TableQuery<RequsetEntity> query = new TableQuery<RequsetEntity>()
                {
                    SelectColumns = new List<string>()
                    {
                        CoreConstants.AzureTables.PartitionKey, CoreConstants.AzureTables.BackendId
                    }
                }.Where(finalFilter);
                //call dataprovider method to get entities from azure table
                List<RequsetEntity> openequests = DataProvider.GetEntitiesList<RequsetEntity>(CoreConstants.AzureTables.RequestTransactions, query);                                
                return openequests.Count();
            }
            catch (Exception exception)
            {
                //write exception into application insights
                InsightLogger.Exception(exception.Message + " - Error while getting open requets count in DAL", exception, callerMethodName);                
                throw new DataAccessException();
            }

        }

        /// <summary>
        /// method to get count of open approvals per userbackend
        /// </summary>
        /// <param name="userid">takes userid as input</param>
        /// <param name="backendid">takes backendid as input</param>
        /// <returns>returns count of open approvals per user backend</returns>
        public int GetOpenApprovalsCount(string userid, string backendid)
        {
            //Get Caller Method name
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                //adding filters to get count of open approvals
                string partitionFilter = TableQuery.GenerateFilterCondition(CoreConstants.AzureTables.PartitionKey, QueryComparisons.Equal, string.Concat(CoreConstants.AzureTables.ApprovalPK,userid));
                string rowfilter = TableQuery.GenerateFilterCondition(CoreConstants.AzureTables.BackendId, QueryComparisons.Equal, backendid);
                string statusfilter = TableQuery.GenerateFilterCondition(CoreConstants.AzureTables.Status, QueryComparisons.Equal, CoreConstants.AzureTables.Waiting);
                string finalFilter = TableQuery.CombineFilters(TableQuery.CombineFilters(partitionFilter, TableOperators.And, rowfilter), TableOperators.And, statusfilter);
                //selecting only few columnas as we are just caliculating count of open approvals
                TableQuery<ApprovalEntity> query = new TableQuery<ApprovalEntity>()
                {
                    SelectColumns = new List<string>()
                    {
                        CoreConstants.AzureTables.PartitionKey, CoreConstants.AzureTables.BackendId
                    }
                }.Where(finalFilter);
                //call dataprovider method to get entities from azure table
                List<ApprovalEntity> openaprovals = DataProvider.GetEntitiesList<ApprovalEntity>(CoreConstants.AzureTables.RequestTransactions, query);
                return openaprovals.Count();
            }
            catch (Exception exception)
            {
                //write exception into application insights
                InsightLogger.Exception(exception.Message + " - Error while getting open approvals count in DAL", exception, callerMethodName);                
                throw new DataAccessException();
            }
        }

        /// <summary>
        /// method to get count of urgent approvals per userbackend
        /// </summary>
        /// <param name="userid">takes userid as input</param>
        /// <param name="backendid">takes backendid as input</param>
        /// <returns>returns count of urgent approvals per user backend</returns>
        public int GetUrgentApprovalsCount(string userid, string backendid)
        {
            //Get Caller Method name
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                //adding filters to get count of open approvals
                string partitionFilter = TableQuery.GenerateFilterCondition(CoreConstants.AzureTables.PartitionKey, QueryComparisons.Equal, string.Concat(CoreConstants.AzureTables.ApprovalPK, userid));
                string rowfilter = TableQuery.GenerateFilterCondition(CoreConstants.AzureTables.BackendId, QueryComparisons.Equal, backendid);
                string statusfilter = TableQuery.GenerateFilterConditionForDate(CoreConstants.AzureTables.DueDate, QueryComparisons.LessThanOrEqual, DateTime.Today);
                string finalFilter = TableQuery.CombineFilters(TableQuery.CombineFilters(partitionFilter, TableOperators.And, rowfilter), TableOperators.And, statusfilter);
                //selecting only few columnas as we are just caliculating count of urgent approvals
                TableQuery<ApprovalEntity> query = new TableQuery<ApprovalEntity>()
                {
                    SelectColumns = new List<string>()
                    {
                        CoreConstants.AzureTables.PartitionKey, CoreConstants.AzureTables.BackendId
                    }
                }.Where(finalFilter);
                //call dataprovider method to get entities from azure table
                List<ApprovalEntity> urgentaprovals = DataProvider.GetEntitiesList<ApprovalEntity>(CoreConstants.AzureTables.RequestTransactions, query);
                return urgentaprovals.Count;
            }
            catch (Exception exception)
            {
                //write exception into application insights
                InsightLogger.Exception(exception.Message + " - Error while getting urgent approvals count in DAL", exception, callerMethodName);                
                throw new DataAccessException();
            }
        }

        /// <summary>
        /// method to update userbackend
        /// </summary>
        /// <param name="userbackend">takes userbackend entity as input</param>
        public void UpdateUserBackend(UserBackendEntity userbackend)
        {
            //Get Caller Method name
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                //call dataprovider method to update entity to azure table
                DataProvider.UpdateEntity<UserBackendEntity>(CoreConstants.AzureTables.UserDeviceConfiguration, userbackend);
            }
            catch (Exception exception)
            {
                //write exception into application insights
                InsightLogger.Exception(exception.Message + " - Error while updating userbackend to azure table in DAL", exception, callerMethodName);                
                throw new DataAccessException();
            }
        }

        /// <summary>
        /// method to get backend entity
        /// </summary>
        /// <param name="backendid">tackes backendId as input</param>
        /// <returns>returns backend entity</returns>
        public BackendEntity GetBackend(string backendid)
        {
            //Get Caller Method name
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                //call dataprovider method to retrieve entity from azure table
                BackendEntity backend = DataProvider.Retrieveentity<BackendEntity>(CoreConstants.AzureTables.ReferenceData, CoreConstants.AzureTables.BackendPK, backendid);
                return backend;
            }
            catch (Exception exception)
            {
                //write exception into application insights
                InsightLogger.Exception(exception.Message + " - Error while getting backend from azure table in DAL", exception, callerMethodName);                
                throw new DataAccessException();
            }
        }

        /// <summary>
        /// method to update backend
        /// </summary>
        /// <param name="backend">takes backend entity as input</param>
        public void UpdateBackend(BackendEntity backend)
        {
            //Get Caller Method name
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                //call dataprovider method to update entity to azure table
                DataProvider.UpdateEntity<BackendEntity>(CoreConstants.AzureTables.ReferenceData, backend);
            }
            catch (Exception exception)
            {
                //write exception into application insights
                InsightLogger.Exception(exception.Message + " - Error while updating backend to azure table in DAL", exception, callerMethodName);                
                throw new DataAccessException();
            }
        }

        /// <summary>
        /// method to add user to be updated into updatetrigger queue
        /// </summary>
        /// <param name="updateTriggeringMessage">takes message object as input</param>
        public void AddUpdateTriggerMessageToQueue(UpdateTriggeringMessage updateTriggeringMessage)
        {
            try
            {
                string message = JsonConvert.SerializeObject(updateTriggeringMessage);
                //call dataprovider method to add message to azure queue
                DataProvider.AddMessagetoQueue(CoreConstants.AzureQueues.UpdateTriggerVIPQueueName, message);
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error while adding message to updatetriggering queue in DAL : "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new DataAccessException();
            }
        }
    }
}
