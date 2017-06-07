//-----------------------------------------------------------
// <copyright file="synch.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using adidas.clb.MobileApproval.Exceptions;
using adidas.clb.MobileApproval.Models;
using adidas.clb.MobileApproval.Utility;
using adidas.clb.MobileApproval.App_Code.DAL.Personalization;
using adidas.clb.MobileApproval.App_Code.DAL.Synch;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Concurrent;
using Newtonsoft.Json;
namespace adidas.clb.MobileApproval.App_Code.BL.Synch
{
    /// <summary>
    /// The class which implements methods for business logic layer of synch.
    /// </summary>
    public class Synch
    {
        //Application insights interface reference for logging the error details into Application Insight azure service.
        static IAppInsight InsightLogger { get { return AppInsightLogger.Instance; } }
        public static string azureTableRequestTransactions = ConfigurationManager.AppSettings["AzureTables.RequestTransactions"];
        //batchsize
        public static int batchsize = Convert.ToInt32(ConfigurationManager.AppSettings["ListBatchSize"]);
        public static string azureTableUserDeviceConfiguration = ConfigurationManager.AppSettings["AzureTables.UserDeviceConfiguration"];
        //read GetPDFs value from configuration
        public static bool IsGeneratePdfs = Convert.ToBoolean(ConfigurationManager.AppSettings["GetPDFs"]);
        public static bool IsGeneratePdfsForRequest = Convert.ToBoolean(ConfigurationManager.AppSettings["GetPDFsForRequest"]);
        //read VIP Flag  value from configuration
        public static bool IsVIPFlag = Convert.ToBoolean(ConfigurationManager.AppSettings["VIPFlag"]);
        private static string urgentTaskStatus = Convert.ToString(ConfigurationManager.AppSettings["UrgentTaskStatus"]);
        private static string waitingTaskStatus = Convert.ToString(ConfigurationManager.AppSettings["WaitingTaskStatus"]);
        private static string taskReadStatus = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["TaskReadStatus"]);
       
        /// <summary>
        /// method to get list of backends associated to user
        /// </summary>
        /// <param name="userID">takes userid as input</param>
        /// <returns>returns list of userbackends</returns>
        public List<UserBackendEntity> GetUserBackendsList(string userID, List<string> userbackends)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                //if userbackendid's not provided get all associated backends to user by default
                if (userbackends != null)
                {
                    SynchDAL synchdDAL = new SynchDAL();
                    //calling data access layer method                
                    return synchdDAL.GetUserAllBackends(userID, userbackends);
                }
                else
                {
                    UserBackendDAL userbackenddal = new UserBackendDAL();
                    return userbackenddal.GetUserAllBackends(userID);
                }

            }
            catch (DataAccessException DALexception)
            {
                throw DALexception;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                //LoggerHelper.WriteToLog(exception + " - Error in BL while getting userbackend list per user : "
                //+ exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new BusinessLogicException();
            }
        }

        /// <summary>
        /// method to add message to queue to trigger userbackend update
        /// </summary>
        /// <param name="userbackend">takes userbackend as input</param>
        public void TriggerUserBackendUpdate(UserBackendEntity userbackend, bool isForceUpdate)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                UpdateTriggeringMessage updateTriggerMessage = new UpdateTriggeringMessage();
                UserUpdateMsg usermsg = new UserUpdateMsg();
                usermsg.UserID = userbackend.UserID;
                List<UpdateTriggerBackend> updatetriggerbackendlist = new List<UpdateTriggerBackend>();
                //adding backend to message object
                UpdateTriggerBackend triggerbackend = new UpdateTriggerBackend();
                triggerbackend.BackendID = userbackend.BackendID;
                updateTriggerMessage.ChangeAfter = userbackend.LastUpdate;
                updatetriggerbackendlist.Add(triggerbackend);
                usermsg.Backends = updatetriggerbackendlist;
                //creating list to add users                
                List<UserUpdateMsg> usermsglist = new List<UserUpdateMsg>();
                usermsglist.Add(usermsg);
                updateTriggerMessage.Users = usermsglist;
                //calling data access layer method to add message to queue
                PersonalizationDAL personalizationdal = new PersonalizationDAL();
                if (isForceUpdate)
                {
                    personalizationdal.ForceUpdate_AddUpdateTriggerMessageToQueue(updateTriggerMessage);
                }
                else
                {
                    personalizationdal.AddUpdateTriggerMessageToQueue(updateTriggerMessage);
                }


            }
            catch (DataAccessException DALexception)
            {
                throw DALexception;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                //LoggerHelper.WriteToLog(exception + " - Error in BL while formatting updatetriggering message : "
                //+ exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new BusinessLogicException();
            }
        }

        /// <summary>
        /// method to add message to queue to trigger request update
        /// </summary>
        /// <param name="request">takes request as input</param>
        public void TriggerRequestUpdate(RequestEntity request, string UserID)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                UpdateTriggeringMessage updateTriggerMessage = new UpdateTriggeringMessage();
                List<RequestUpdateMsg> updatetriggerrequestlist = new List<RequestUpdateMsg>();
                RequestUpdateMsg triggerrequset = new RequestUpdateMsg();
                //adding request to message object
                Request requestobj = new Request();
                requestobj.ID = request.ID;
                requestobj.UserID = UserID;
                triggerrequset.request = requestobj;

                //adding backend to queue message
                UpdateTriggerBackend backendobj = new UpdateTriggerBackend();
                backendobj.BackendID = request.BackendID;
                triggerrequset.request.Backend = backendobj;
                //add requests to list which will be added to message
                updatetriggerrequestlist.Add(triggerrequset);
                updateTriggerMessage.Requests = updatetriggerrequestlist;
                updateTriggerMessage.GetPDFs = Convert.ToBoolean(ConfigurationManager.AppSettings[CoreConstants.Config.GetPDFs]);
                //calling data access layer method to add message to queue
                PersonalizationDAL personalizationdal = new PersonalizationDAL();
                personalizationdal.AddUpdateTriggerMessageToQueue(updateTriggerMessage);

            }
            catch (DataAccessException DALexception)
            {
                throw DALexception;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                //LoggerHelper.WriteToLog(exception + " - Error in BL while formatting updatetriggering message : "
                //+ exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new BusinessLogicException();
            }
        }

        /// <summary>
        /// method to get all requets of user
        /// </summary>
        /// <param name="userID">takes userid as input</param>
        /// <returns>reurns list of requests for user</returns>
        public List<RequestEntity> GetUserRequests(string userID, string requeststatus)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                //if requeststatus is null, get requests with defaultstatus inprogress.
                if (string.IsNullOrEmpty(requeststatus))
                {
                    requeststatus = CoreConstants.AzureTables.InProgress;
                }
                SynchDAL synchDAL = new SynchDAL();
                //calling data access layer method                
                return synchDAL.GetUserRequests(userID, requeststatus);
            }
            catch (DataAccessException DALexception)
            {
                throw DALexception;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                //LoggerHelper.WriteToLog(exception + " - Error in BL while getting all requets per user  : "
                //+ exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new BusinessLogicException();
            }
        }

        /// <summary>
        /// method to get userbackend
        /// </summary>
        /// <param name="userID">takes userid as input</param>
        /// <param name="backendID">takes backendid as input</param>
        /// <returns>returns userbackend</returns>
        public UserBackendEntity GetUserBackend(string userID, string backendID)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                UserBackendDAL userbackendDAL = new UserBackendDAL();
                //calling data access layer method                
                return userbackendDAL.GetUserBackend(userID, backendID);
            }
            catch (DataAccessException DALexception)
            {
                throw DALexception;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                //LoggerHelper.WriteToLog(exception + " - Error in BL while getting userbackend : "
                //+ exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new BusinessLogicException();
            }
        }

        /// <summary>
        /// method to get Requests per userbackend
        /// </summary>
        /// <param name="userID">takes userid as input</param>
        /// <param name="backendID">takes backendid as input</param>
        /// <returns>returns Requests associated to userbackend</returns>
        public List<RequestEntity> GetUserBackendRequests(string userID, string backendID, string requeststatus)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                //if requeststatus is null, get requests with defaultstatus inprogress.
                if (string.IsNullOrEmpty(requeststatus))
                {
                    requeststatus = CoreConstants.AzureTables.InProgress;
                }
                SynchDAL synchDAL = new SynchDAL();
                //calling data access layer method                
                return synchDAL.GetUserBackendRequests(userID, backendID, requeststatus);
            }
            catch (DataAccessException DALexception)
            {
                throw DALexception;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                //LoggerHelper.WriteToLog(exception + " - Error in BL while getting requests per userbackend : "
                //+ exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new BusinessLogicException();
            }
        }

        /// <summary>
        /// method to get request with id
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
                SynchDAL synchDAL = new SynchDAL();
                //calling data access layer method                
                return synchDAL.GetRequest(userID, requestID);
            }
            catch (DataAccessException DALexception)
            {
                throw DALexception;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                //LoggerHelper.WriteToLog(exception + " - Error in BL while getting request : "
                //+ exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new BusinessLogicException();
            }
        }
        /// <summary>
        /// This method update the approval task view status fields as Read
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="taskID"></param>
        public void UpdateTaskViewStatus(string userID, string taskID)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                if (!string.IsNullOrEmpty(taskID))
                {
                    SynchDAL synchDAL = new SynchDAL();
                    //get approvalrequest object from RequestTransactions azure table based on partitionkey and rowkey(requestID)
                    ApprovalEntity apprReqEnt = DataProvider.Retrieveentity<ApprovalEntity>(CoreConstants.AzureTables.RequestTransactions, string.Concat(CoreConstants.AzureTables.ApprovalPK, userID), taskID);
                    if (apprReqEnt != null)
                    {
                        apprReqEnt.TaskViewStatus = taskReadStatus;
                        //call dataprovider method to update entity to azure table
                        DataProvider.UpdateEntity<ApprovalEntity>(CoreConstants.AzureTables.RequestTransactions, apprReqEnt);
                    }
                }

            }
            catch (DataAccessException DALexception)
            {
                throw DALexception;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                //LoggerHelper.WriteToLog(exception + " - Error in BL while getting request : "
                //+ exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new BusinessLogicException();
            }
        }

        /// <summary>
        /// method to get shared access service pdf uri with id
        /// </summary>
        /// <param name="pdfuri">takes pdfuri as input</param>
        /// <returns>returns sas pdf uri</returns>
        public Uri GetSASPdfUri(string pdfuri)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                SynchDAL synchDAL = new SynchDAL();
                //calling data access layer method                
                return synchDAL.GetSASPdfUri(pdfuri);
            }
            catch (DataAccessException DALexception)
            {
                throw DALexception;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                //LoggerHelper.WriteToLog(exception + " - Error in BL while getting shared access service pdf uri : "
                //+ exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new BusinessLogicException();
            }
        }

        /// <summary>
        /// method to get approvers list per request
        /// </summary>
        /// <param name="requestID">takes request id as input</param>
        /// <returns>returns list of approvers</returns>
        public List<ApproverDTO> GetApprovers(string requestID)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                SynchDAL synchDAL = new SynchDAL();
                //calling data access layer method 
                List<ApproverEntity> approversentitylist = synchDAL.GetApprovers(requestID);
                List<ApproverDTO> approvers = new List<ApproverDTO>();
                //loop through approvers list entity to convert to approvers dto
                foreach (ApproverEntity approver in approversentitylist)
                {
                    ApproverDTO approverdto = DataProvider.ResponseObjectMapper<ApproverDTO, ApproverEntity>(approver);
                    approvers.Add(approverdto);
                }
                return approvers;
            }
            catch (DataAccessException DALexception)
            {
                throw DALexception;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                //LoggerHelper.WriteToLog(exception + " - Error in BL while getting approvers per request : "
                //+ exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new BusinessLogicException();
            }
        }

        /// <summary>
        /// method to get fields for request
        /// </summary>
        /// <param name="requestID">takes requestid as input</param>
        /// <returns>returns list of fields</returns>
        public List<FieldDTO> GetFields(string requestID)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                SynchDAL synchDAL = new SynchDAL();
                //calling data access layer method 
                List<FieldEntity> fieldsentitylist = synchDAL.GetFields(requestID);
                List<FieldDTO> fileds = new List<FieldDTO>();
                //loop through fields list entity to convert to fields dto
                foreach (FieldEntity field in fieldsentitylist)
                {
                    FieldDTO fielddto = DataProvider.ResponseObjectMapper<FieldDTO, FieldEntity>(field);
                    fileds.Add(fielddto);
                }
                return fileds;
            }
            catch (DataAccessException DALexception)
            {
                throw DALexception;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                //LoggerHelper.WriteToLog(exception + " - Error in BL while getting fields per request : "
                //+ exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new BusinessLogicException();
            }
        }

        /// <summary>
        /// method to add or update userbackend synch
        /// </summary>
        /// <param name="userbackend"></param>

        public void AddUpdateBackendSynch(UserBackendEntity userbackend)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                SynchDAL synchDAL = new SynchDAL();
                SynchEntity backendsynch = synchDAL.GetUserBackendSynch(userbackend.UserID, userbackend.BackendID);
                //declare local int variable for last Synch frequency
                int lastSynchFrequency = 0;
                //declare local int variable for avg Synch frequency
                int avgSynchFrequency = 0;
                //backend synch available then update
                if (backendsynch != null)
                {
                    //last synch frequency
                    //updated on 07/06/2017 : update From days to seconds                   
                    lastSynchFrequency= (backendsynch.LastSynch.Value - DateTime.Now).Seconds;
                    backendsynch.lastSynchFreq = lastSynchFrequency;
                    //update best synch frequency
                    if (backendsynch.lastSynchFreq < backendsynch.bestSynchFreq)
                    {
                        backendsynch.bestSynchFreq = backendsynch.lastSynchFreq;
                    }
                    avgSynchFrequency=(backendsynch.lastSynchFreq + (backendsynch.SynchCount * backendsynch.avgSynchFreq)) / (backendsynch.SynchCount + 1);
                    //if avgSynchFrequency is zero then take lastSynchFrequency value as avgSynchFrequency value
                    backendsynch.avgSynchFreq = (avgSynchFrequency > 0) ? avgSynchFrequency : lastSynchFrequency;
                    backendsynch.SynchCount = backendsynch.SynchCount + 1;
                    backendsynch.LastSynch = DateTime.Now;
                    //calling data access layer method                
                    synchDAL.AddUpdateBackendSynch(backendsynch);
                }
                else
                {
                    SynchEntity newbackendsynch = new SynchEntity();
                    newbackendsynch.PartitionKey = string.Concat(CoreConstants.AzureTables.BackendSynchPK, userbackend.UserID);
                    newbackendsynch.RowKey = string.Concat(CoreConstants.AzureTables.BackendSynchPK, userbackend.BackendID);
                    newbackendsynch.LastSynch = DateTime.Now;
                    newbackendsynch.SynchCount = 1;
                    //calling data access layer method                
                    synchDAL.AddUpdateBackendSynch(newbackendsynch);
                }

            }
            catch (DataAccessException DALexception)
            {
                throw DALexception;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                //LoggerHelper.WriteToLog(exception + " - Error in BL while adding userbackend synch  : "
                //+ exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new BusinessLogicException();
            }
        }

        // <summary>
        /// method add or get request synch
        /// </summary>
        /// <param name="request"></param>
        public RequestSynchEntity GetRequestSynch(RequestEntity request)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                SynchDAL synchDAL = new SynchDAL();
                RequestSynchEntity requestsynch = synchDAL.GetRequestSynch(CoreConstants.AzureTables.RequestSynchPK, request.ID);
                return requestsynch;
            }
            catch (DataAccessException DALexception)
            {
                throw DALexception;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                //LoggerHelper.WriteToLog(exception + " - Error in BL while getting request synch  : "
                //+ exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new BusinessLogicException();
            }
        }

        /// <summary>
        /// method add or update request synch
        /// </summary>
        /// <param name="request"></param>
        public void AddUpdateRequestSynch(RequestEntity request, RequestSynchEntity requestsynch, string userid)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                SynchDAL synchDAL = new SynchDAL();
                if (requestsynch != null)
                {
                    //last synch frequency
                    requestsynch.LastChange = DateTime.Now;
                    //calling data access layer method                
                    synchDAL.AddUpdateRequestSynch(requestsynch);
                }
                else
                {
                    RequestSynchEntity newrequestsynch = new RequestSynchEntity();
                    newrequestsynch.PartitionKey = CoreConstants.AzureTables.RequestSynchPK;
                    newrequestsynch.RowKey = request.ID;
                    newrequestsynch.LastChange = DateTime.Now;
                    newrequestsynch.BackendID = request.BackendID;
                    newrequestsynch.UserID = userid;
                    //calling data access layer method                
                    synchDAL.AddUpdateRequestSynch(newrequestsynch);
                }
            }
            catch (DataAccessException DALexception)
            {
                throw DALexception;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                //LoggerHelper.WriteToLog(exception + " - Error in BL while adding/updating request synch  : "
                //+ exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new BusinessLogicException();
            }
        }

        /// <summary>
        /// method to get approvals per userbackend
        /// </summary>
        /// <param name="userID">takes userid as input</param>
        /// <param name="backendID">takes backendid as input</param>
        /// <returns>returns Approvals associated to userbackend</returns>
        public List<ApprovalEntity> GetUserBackendApprovals(string userID, string backendID, string approvalstatus)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                //if requeststatus is null, get approvals with defaultstatus inprogress.
                if (string.IsNullOrEmpty(approvalstatus))
                {
                    approvalstatus = waitingTaskStatus;

                }
                if (!string.IsNullOrEmpty(approvalstatus) && approvalstatus == urgentTaskStatus)
                {
                    approvalstatus = waitingTaskStatus;
                }
                SynchDAL synchDAL = new SynchDAL();
                //calling data access layer method                
                return synchDAL.GetUserBackendApprovals(userID, backendID, approvalstatus);
            }
            catch (DataAccessException DALexception)
            {
                throw DALexception;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                //LoggerHelper.WriteToLog(exception + " - Error in BL while getting approvals per userbackend : "
                //+ exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new BusinessLogicException();
            }
        }

        /// <summary>
        /// method to get all approvals of user
        /// </summary>
        /// <param name="userID">takes userid as input</param>
        /// <returns>reurns list of requests for user</returns>
        public List<ApprovalEntity> GetUserApprovalsForCount(string userID, string approvalstatus)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                //if requeststatus is null, get requests with defaultstatus inprogress.
                if (string.IsNullOrEmpty(approvalstatus))
                {
                    approvalstatus = waitingTaskStatus;
                }
                else
                {
                    if (approvalstatus == urgentTaskStatus)
                    {
                        approvalstatus = waitingTaskStatus;
                    }
                }

                SynchDAL synchDAL = new SynchDAL();
                //calling data access layer method      

                return synchDAL.GetUserApprovalsForCount(userID, approvalstatus);


            }
            catch (DataAccessException DALexception)
            {
                throw DALexception;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                //LoggerHelper.WriteToLog(exception + " - Error in BL while getting all approvals count per user  : "
                //+ exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new BusinessLogicException();
            }
        }
        /// <summary>
        /// This method returns the unread requests 
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        public int GetUserUnReadRequestsCount(string userID)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                //declare int variable which will return un read requests count
                int newRequests = 0;
                //get all pending tasks from backend
                SynchDAL synchDAL = new SynchDAL();
                //calling data access layer method     
                List<ApprovalEntity> lstNewRequests = synchDAL.GetUserApprovalsForCount(userID, waitingTaskStatus);
                if (lstNewRequests != null && lstNewRequests.Count > 0)
                {
                    //get new tasks from pending tasks
                    var lstFilter = lstNewRequests.Where(x => x.TaskViewStatus != taskReadStatus && !string.IsNullOrEmpty(x.TaskViewStatus)).ToList();
                    if (lstFilter != null && lstFilter.Count > 0)
                    {
                        //assign new requests count 
                        newRequests = lstFilter.Count;
                    }
                   
                }
                return newRequests;


            }
            catch (DataAccessException DALexception)
            {
                throw DALexception;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                //LoggerHelper.WriteToLog(exception + " - Error in BL while getting all approvals count per user  : "
                //+ exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new BusinessLogicException();
            }
        }
        public List<ApprovalEntity> GetAllUserApprovals(string userID)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();

                SynchDAL synchDAL = new SynchDAL();
                //calling data access layer method      

                return synchDAL.GetAllUserApprovalsForCount(userID);


            }
            catch (DataAccessException DALexception)
            {
                throw DALexception;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                //LoggerHelper.WriteToLog(exception + " - Error in BL while getting all approvals count per user  : "
                //+ exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new BusinessLogicException();
            }
        }

        /// <summary>
        /// method to loop through userbackend requests and add tasks to response
        /// </summary>
        /// <param name="requestslist"></param>
        /// <param name="approvalslist"></param>
        /// <param name="userbackend"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public List<ApprovalRequestDTO> AddRequsetsTasksToSynchResponse(List<RequestEntity> requestslist, List<ApprovalEntity> approvalslist, UserBackendEntity userbackend, SynchRequestDTO query)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                List<ApprovalRequestDTO> approvalrequestlist = new List<ApprovalRequestDTO>();
                InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{userID}/backends/{usrBackendID}/requests, action: Loop through all requests in backend");
                // loop through each request in the userbackend
                foreach (RequestEntity request in requestslist)
                {
                    ApprovalRequestDTO approvalrequest = new ApprovalRequestDTO();
                    RequestDTO requestdto = new RequestDTO();
                    //add approval task to response
                    ApprovalEntity approval = approvalslist.Find(x => x.BackendID == userbackend.BackendID && x.RequestId == request.ID);
                    if (approval != null)
                    {
                        ApprovalDTO approvaldto = new ApprovalDTO();
                        approvaldto = DataProvider.ResponseObjectMapper<ApprovalDTO, ApprovalEntity>(approval);
                        approvalrequest.approval = approvaldto;
                        //if request is updated
                        if (Rules.IsRequestUpdated(request, userbackend.DefaultUpdateFrequency))
                        {
                            InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{userID}/backends/{usrBackendID}/requests, action: check request update, response: true");
                            //get request synch entity
                            RequestSynchEntity requestsynch = GetRequestSynch(request);
                            //check if requests which have changed since the last synch need to send in response or all requests.
                            if (Rules.IsTargetRequest(query, request, approval, requestsynch))
                            {
                                InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{userID}/backends/{usrBackendID}/requests, action: Target request, response: true");
                                requestdto = DataProvider.ResponseObjectMapper<RequestDTO, RequestEntity>(request);
                                approvalrequest.request = requestdto;
                                InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{userID}/backends/{usrBackendID}/requests, action: add request header to response, response: success");
                                //code to populate extended depth
                                //code to update request synch timestamp
                                AddUpdateRequestSynch(request, requestsynch, query.userId);
                                InsightLogger.TrackEvent("SyncAPIController :: api/synch/users/{userID}/backends/{usrBackendID}/requests, action: Update request synch timestamp, response: success");
                                //requestsfulfilled = true;
                            }
                        }
                        else
                        {
                            InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{userID}/backends/{usrBackendID}/requests, action: check request update, response: false");
                            //check if request update is in progress in service layer then send the latency in response
                            if (Rules.IsRequestUpdateInProgress(request))
                            {
                                InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{userID}/backends/{usrBackendID}/requests, action: check in-progress request update, response: true");
                                approvalrequest.retryAfter = request.ExpectedLatency;
                                InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{userID}/backends/{usrBackendID}/requests, action: Add expected request latancy to resposne as retry time, response: Success");
                            }
                            else
                            {
                                InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{userID}/backends/{usrBackendID}/requests, action: check in-progress request update, response: false");
                                TriggerRequestUpdate(request, query.userId);
                                InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{userID}/backends/{usrBackendID}/requests, action: trigger a Request update, response: success");
                                approvalrequest.retryAfter = Convert.ToInt32(Rules.RequestRetryTime(userbackend));
                                InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{userID}/backends/{usrBackendID}/requests, action: Add request retrytime to response, response: success");
                            }
                            //requestsunfulfilled = true;
                        }
                        //add approval request to list which will be added to corresponding backend
                        approvalrequestlist.Add(approvalrequest);
                    }
                }
                return approvalrequestlist;
            }
            catch (DataAccessException DALexception)
            {
                throw DALexception;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                //LoggerHelper.WriteToLog(exception + " - Error in BL while adding all approvals tasks to synch response  : "
                //+ exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new BusinessLogicException();
            }
        }

        public UserBackendDTO AddRequsetsTasksCountToSynchResponse(List<RequestEntity> userbackendrequestslist, List<ApprovalEntity> userbackendapprovalslist, UserBackendEntity userbackend, SynchRequestDTO query, UserBackendDTO userbackenddto)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                List<ApprovalRequestDTO> approvalrequestlist = new List<ApprovalRequestDTO>();
                InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{userID}/backends, action: Loop through all requests in backend");
                //loop through each request in the userbackend
                foreach (RequestEntity request in userbackendrequestslist)
                {
                    ApprovalRequestDTO approvalrequest = new ApprovalRequestDTO();
                    RequestDTO requestdto = new RequestDTO();
                    //get approval associated to request
                    ApprovalEntity approval = userbackendapprovalslist.Find(x => x.RequestId == request.ID);
                    if (approval != null)
                    {
                        ApprovalDTO approvaldto = new ApprovalDTO();
                        approvaldto = DataProvider.ResponseObjectMapper<ApprovalDTO, ApprovalEntity>(approval);
                        approvalrequest.approval = approvaldto;
                        userbackenddto.approvalsCount.Count = userbackenddto.approvalsCount.Count + 1;
                        //if request is updated
                        if (Rules.IsRequestUpdated(request, userbackend.DefaultUpdateFrequency))
                        {
                            InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{userID}/backends, action: check request update, response: true");
                            //get request synch entity
                            RequestSynchEntity requestsynch = GetRequestSynch(request);
                            //check if requests which have changed since the last synch need to send in response or all requests.
                            if (Rules.IsTargetRequest(query, request, approval, requestsynch))
                            {
                                InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{userID}/backends, action: Target request, response: true");
                                requestdto = DataProvider.ResponseObjectMapper<RequestDTO, RequestEntity>(request);
                                approvalrequest.request = requestdto;
                                InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{userID}/backends, action: add request header to response, response: success");
                                //code here to populate extended depth
                                //code here to update request synch time stamp
                                AddUpdateRequestSynch(request, requestsynch, query.userId);
                                InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{userID}/backends, action: Update request synch timestamp, response: success");
                                //requestsfulfilled = true;
                            }
                        }
                        else
                        {
                            InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{userID}/backends, action: check request update, response: false");
                            //check if request update is in progress in service layer then send the latency in response
                            if (Rules.IsRequestUpdateInProgress(request))
                            {
                                InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{userID}/backends, action: update in-progress, response: true");
                                approvalrequest.retryAfter = request.ExpectedLatency;
                                InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{userID}/backends, action: Add expected request latancy to resposne as retry time, response: Success");
                            }
                            else
                            {
                                InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{userID}/backends, action: update in-progress, response: false");
                                TriggerRequestUpdate(request, query.userId);
                                InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{userID}/backends, action: trigger a Request update, response: success");
                                approvalrequest.retryAfter = Convert.ToInt32(Rules.RequestRetryTime(userbackend));
                                InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{userID}/backends, action: Add request retrytime to response, response: success");
                            }
                            //requestsunfulfilled = true;
                        }
                        //add approval request to list which will be added to corresponding backend
                        approvalrequestlist.Add(approvalrequest);
                    }
                }
                userbackenddto.requests = approvalrequestlist;
                return userbackenddto;
            }
            catch (DataAccessException DALexception)
            {
                throw DALexception;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                //LoggerHelper.WriteToLog(exception + " - Error in BL while adding all approvals count to response  : "
                //+ exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new BusinessLogicException();
            }
        }

        /// <summary>        
        /// method to calculate sync waiting time      
        /// </summary>
        /// <param name="Backendtouser">takes backends associated to user</param>       
        /// <returns>returns synch waiting time</returns>
        public int CalcSynchTime(IEnumerable<UserBackendEntity> Userbackends)
        {
            UserBackendDAL userBackenddal = new UserBackendDAL();
            //calling data access layer method to backend entities
            IEnumerable<BackendEntity> Backendtouser = userBackenddal.GetRequiredBackends(Userbackends);
            //calling rules to caliculate synch time 
            if (Backendtouser != null)
            {
                return Rules.SynchWaitingTime(Backendtouser); ;
            }
            else
            {
                return 0;
            }

        }
        /// <summary>
        /// This method collects missed updates as well missed update requests.
        /// </summary>
        /// <param name="muserBackend"></param>
        /// <param name="currentTimestamp"></param>
        public void CollectUsersMissedUpdatesByBackend(UserBackendEntity muserBackend, DateTime currentTimestamp)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                string backendId = muserBackend.BackendID;
                string userID = muserBackend.UserID;
                double userUpdateFrequency = Convert.ToDouble(muserBackend.DefaultUpdateFrequency);
                //checking is user backend update missing or not with the help of Updatetriggering rule R6
                if (this.IsUserUpdateMissing(muserBackend.UpdateTriggered, muserBackend.ExpectedUpdate, currentTimestamp))
                {
                    // InsightLogger.TrackEvent("UpdateTriggering, Action :: Is User [ " + muserBackend.UserID + " ] missed updates for the backend:[" + muserBackend.BackendID + " ] based on UT Rule R6 , Response :: true");
                    //parse data to UpdateTriggeringMsg class and seralize UpdateTriggeringMsg object into json string                           
                    this.TriggerUserBackendUpdate(muserBackend, true);

                }
                //check for requests ::
                this.CollectsRequestsMissedUpdateByBackendID(backendId, userID, currentTimestamp, userUpdateFrequency);

            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                throw new DataAccessException(exception.Message, exception.InnerException);
            }

        }
        /// <summary>
        /// Update Triggering Rule R6 :: checking user backend update is missing or not
        /// </summary>
        /// <param name="UserBackendUpdateTriggered"></param>
        /// <param name="UserBackendExpectedUpdate"></param>
        /// <returns></returns>
        public bool IsUserUpdateMissing(bool UserBackendUpdateTriggered, DateTime? UserBackendExpectedUpdate, DateTime now)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                bool IsUpdateMissing = false;
                //Rule R6 :: UserBackend.UpdateTriggered AND (NOW >UserBackend.ExpectedUpdate)
                if ((UserBackendUpdateTriggered) && (now > UserBackendExpectedUpdate))
                {
                    IsUpdateMissing = true;
                }
                return IsUpdateMissing;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);

                throw new BusinessLogicException(exception.Message, exception.InnerException);
            }
        }
        /// <summary>
        /// This method collects requests which have missed updates
        /// </summary>
        /// <param name="backendID"></param>
        /// <param name="userID"></param>
        /// <param name="timestamp"></param>
        /// <param name="userUpdateFrequency"></param>
        public void CollectsRequestsMissedUpdateByBackendID(string backendID, string userID, DateTime timestamp, double userUpdateFrequency)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                //InsightLogger.TrackEvent("UpdateTriggering, Action :: collecting the requests which have missed updates for the backend [" + backendID + "] and user: [" + userID + "]");

                var ctsRequests = new CancellationTokenSource();
                //get's azure table instance
                CloudTable RequestsMissedDeviceConfigurationTable = DataProvider.GetAzureTableInstance(azureTableRequestTransactions);
                //Get all the userbackends associated with the backend
                //partition key
                string partitionFilter = TableQuery.GenerateFilterCondition(CoreConstants.AzureTables.PartitionKey, QueryComparisons.Equal, CoreConstants.AzureTables.RequestsPK + userID);
                //row key
                string rowfilter = TableQuery.GenerateFilterCondition(CoreConstants.AzureTables.BackendId, QueryComparisons.Equal, backendID);
                //TableQuery<RequestEntity> tquerymissedRequests = new TableQuery<RequestEntity>().Where(TableQuery.CombineFilters(partitionFilter, TableOperators.And, rowfilter));
                //status filter
                string statusfilter = TableQuery.GenerateFilterCondition(CoreConstants.AzureTables.Status, QueryComparisons.NotEqual, Convert.ToString(ConfigurationManager.AppSettings["MissedUpdatesRequestStatus"]));
                string combinedFilter = string.Format("({0}) {1} ({2}) {3} ({4})", partitionFilter, TableOperators.And, rowfilter, TableOperators.And, statusfilter);
                //combine all the filters with And operator
                TableQuery<RequestEntity> tquerymissedRequests = new TableQuery<RequestEntity>().Where(combinedFilter);

                //create task which will parallelly read & checks the Rules from azure table
                Task[] taskRequestCollection = new Task[2];
                var entityMissedupdateRequestsCollection = new BlockingCollection<List<RequestEntity>>();
                taskRequestCollection[0] = Task.Factory.StartNew(() => this.ReadMissedUpdatesRequestsByBackend(RequestsMissedDeviceConfigurationTable, tquerymissedRequests, entityMissedupdateRequestsCollection), TaskCreationOptions.LongRunning);
                taskRequestCollection[1] = Task.Factory.StartNew(() => this.WriteMissedUpdatesRequestsIntoInputQueue(entityMissedupdateRequestsCollection, backendID, userID, timestamp, userUpdateFrequency), TaskCreationOptions.LongRunning);
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
        private void WriteMissedUpdatesRequestsIntoInputQueue(BlockingCollection<List<RequestEntity>> rsource, string rBackendID, string rUserID, DateTime CurTimestamp, double userUpdateFrequency)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                Parallel.ForEach(rsource.GetConsumingEnumerable(), requestitem =>
                {
                    if (requestitem.ToList() != null)
                    {
                        List<string> lstrmsgFormat = new List<string>();
                        List<RequestEntity> reqmissedUpdateslst = new List<RequestEntity>();
                        foreach (RequestEntity requestDetails in requestitem.ToList())
                        {
                            string reqID = requestDetails.RowKey;
                            DateTime reqLastUpdate = (DateTime)requestDetails.LastUpdate;
                            DateTime? reqExpectedUpdate = requestDetails.ExpectedUpdate;
                            bool IsrequestTriggered = requestDetails.UpdateTriggered;
                            //checking is request  update missing or not with the help of Updatetriggering rule R6
                            if (this.IsRequestUpdateMissing(requestDetails.UpdateTriggered, reqExpectedUpdate, CurTimestamp))
                            {

                                //InsightLogger.TrackEvent("UpdateTriggering, Action :: Is Request [ " + reqID + " ] missed update based on UT Rule R6 , Response :: true, Current Timestamp :" + CurTimestamp + ", Request LastUpdate : " + Convert.ToString(reqLastUpdate) + " ,User Update Frequency : " + userUpdateFrequency + " ,Expected Update:" + Convert.ToString(reqExpectedUpdate) + " ,Is request Triggered :" + Convert.ToString(IsrequestTriggered));
                                //add request details to RequestSynchEntity list
                                reqmissedUpdateslst.Add(requestDetails);


                            }
                            //checking request is update or not based on Approval Sync Rule R5
                            else if (!this.IsRequestUpdated(requestDetails.UpdateTriggered, reqLastUpdate, userUpdateFrequency, CurTimestamp))
                            {

                                //InsightLogger.TrackEvent("UpdateTriggering, Action :: Is Request [ " + reqID + " ] needs update based on Approval Sync Rule R5  , Response :: true,Current Timestamp :" + CurTimestamp + ", Request LastUpdate : " + Convert.ToString(reqLastUpdate) + " ,User Update Frequency : " + userUpdateFrequency + " ,Expected Update:" + Convert.ToString(reqExpectedUpdate) + " ,Is request Triggered :" + Convert.ToString(IsrequestTriggered));
                                //add request details to RequestSynchEntity list
                                reqmissedUpdateslst.Add(requestDetails);


                            }
                            else
                            {
                                //InsightLogger.TrackEvent("UpdateTriggering, Action :: Is Request [ " + reqID + " ] needs update[Approval Sync Rule R5]/Missed update[UT Rule R6], Response :: false, Current Timestamp :" + CurTimestamp + ", Request LastUpdate : " + Convert.ToString(reqLastUpdate) + " ,User Update Frequency : " + userUpdateFrequency + " ,Expected Update:" + Convert.ToString(reqExpectedUpdate) + " ,Is request Triggered :" + Convert.ToString(IsrequestTriggered));
                            }
                        }
                        if (reqmissedUpdateslst.Count > 0)
                        {
                            //parse data to UpdateTriggeringMsg class and seralize UpdateTriggeringMsg object into json string 
                            lstrmsgFormat = this.ConvertRequestUpdateMsgToUpdateTriggeringMsg(reqmissedUpdateslst, rBackendID, rUserID);
                            //put json string into update triggering input queue
                            SynchDAL sdal = new SynchDAL();
                            sdal.AddMessagestoInputQueue(lstrmsgFormat);

                        }
                    }

                });
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
        public bool IsRequestUpdated(bool RequestUpdateTriggered, DateTime RequestLastUpdate, double UserBackendUpdateFrequency, DateTime nowTime)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                bool isRequestUpdated = false;
                //Approval Sync Rule R5 
                if ((!RequestUpdateTriggered) && ((RequestLastUpdate.AddSeconds(ConvertMinutesToSeconds(UserBackendUpdateFrequency))) > nowTime))
                {
                    isRequestUpdated = true;
                }
                return isRequestUpdated;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);

                throw new BusinessLogicException(exception.Message, exception.InnerException);
            }
        }
        /// <summary>
        /// Update Triggering Rule R6 :: checking request update is missing or not
        /// </summary>
        /// <param name="RequestUpdateTriggered"></param>
        /// <param name="RequestExpectedUpdate"></param>
        /// <returns></returns>
        public bool IsRequestUpdateMissing(bool RequestUpdateTriggered, DateTime? RequestExpectedUpdate, DateTime nowTime)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                bool IsUpdateMissing = false;
                //Rule R6 :: UserBackend.UpdateTriggered AND (NOW >UserBackend.ExpectedUpdate)
                if ((RequestUpdateTriggered) && (nowTime > RequestExpectedUpdate))
                {
                    IsUpdateMissing = true;
                }
                return IsUpdateMissing;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);

                throw new BusinessLogicException(exception.Message, exception.InnerException);
            }
        }

        /// <summary>
        /// This method converts request details into Update Triggering Message Format
        /// </summary>
        /// <param name="objrequestSynch"></param>
        /// <param name="rBackendName"></param>
        /// <returns></returns>
        private List<string> ConvertRequestUpdateMsgToUpdateTriggeringMsg(List<RequestEntity> reqlst, string rBackendName, string userID)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                //add RequestUpdateMsg to list
                List<RequestUpdateMsg> lstRequestUpdateMsg = null;
                string requpdatetriggeringmsg = string.Empty;
                //split the request list into number of child lists based on batch size and put the child lists into ienumerable
                IEnumerable<List<RequestEntity>> lstbatchRequests = this.splitList<RequestEntity>(reqlst, batchsize);
                List<string> lstUtMsgs = new List<string>();
                //foeach list in IEnumerable
                foreach (List<RequestEntity> lstentites in lstbatchRequests)
                {
                    requpdatetriggeringmsg = string.Empty;
                    lstRequestUpdateMsg = new List<RequestUpdateMsg>();
                    //for each request in list
                    foreach (RequestEntity objrequestSynch in lstentites)
                    {

                        //create object and assign values to properties for Backend class 
                        UpdateTriggerBackend objBackend = new UpdateTriggerBackend()
                        {
                            BackendID = objrequestSynch.BackendID,
                            BackendName = rBackendName
                        };
                        //create object and assign values to properties for Request class 
                        Request objRequest = new Request()
                        {
                            ID = objrequestSynch.ID,
                            UserID = userID,
                            // Title = objrequestSynch.Title,
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
                    UpdateTriggeringMessage ObjUTMsg = new UpdateTriggeringMessage()
                    {
                        Users = null,
                        Requests = lstRequestUpdateMsg,
                        VIP = IsVIPFlag,
                        GetPDFs = IsGeneratePdfsForRequest

                    };
                    //Serialize UpdateTriggeringMsg Object into json string
                    requpdatetriggeringmsg = JsonConvert.SerializeObject(ObjUTMsg);
                    //add json msg to string list
                    lstUtMsgs.Add(requpdatetriggeringmsg);
                }


                return lstUtMsgs;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                throw new DataAccessException(exception.Message, exception.InnerException);

            }
        }
        /// <summary>
        /// This method retuns the given list of entities into number of lists based on given batch size
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="locations"></param>
        /// <param name="nSize"></param>
        /// <returns></returns>
        public IEnumerable<List<T>> splitList<T>(List<T> lstreqs, int nSize)
        {
            for (int i = 0; i < lstreqs.Count; i += nSize)
            {
                yield return lstreqs.GetRange(i, Math.Min(nSize, lstreqs.Count - i));
            }
        }
        public static double ConvertMinutesToSeconds(double minutes)
        {
            return TimeSpan.FromMinutes(minutes).TotalSeconds;
        }


    }
}