//-----------------------------------------------------------
// <copyright file="synch.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web;
using adidas.clb.MobileApproval.Exceptions;
using adidas.clb.MobileApproval.Models;
using adidas.clb.MobileApproval.Utility;
using adidas.clb.MobileApproval.App_Code.DAL.Personalization;
using adidas.clb.MobileApproval.App_Code.DAL.Synch;

namespace adidas.clb.MobileApproval.App_Code.BL.Synch
{
    /// <summary>
    /// The class which implements methods for business logic layer of synch.
    /// </summary>
    public class Synch
    {
        //Application insights interface reference for logging the error details into Application Insight azure service.
        static IAppInsight InsightLogger { get { return AppInsightLogger.Instance; } }
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
        public void TriggerUserBackendUpdate(UserBackendEntity userbackend,bool isForceUpdate)
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
                updateTriggerMessage.GetPDFs= Convert.ToBoolean(ConfigurationManager.AppSettings[CoreConstants.Config.GetPDFs]);
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
                //backend synch available then update
                if (backendsynch != null)
                {
                    //last synch frequency
                    backendsynch.lastSynchFreq = (backendsynch.LastSynch.Value - DateTime.Now).Days;
                    //update best synch frequency
                    if (backendsynch.lastSynchFreq < backendsynch.bestSynchFreq)
                    {
                        backendsynch.bestSynchFreq = backendsynch.lastSynchFreq;
                    }
                    backendsynch.avgSynchFreq = (backendsynch.lastSynchFreq + (backendsynch.SynchCount * backendsynch.avgSynchFreq)) / (backendsynch.SynchCount + 1);
                    backendsynch.SynchCount = backendsynch.SynchCount + 1;
                    backendsynch.LastSynch = DateTime.Now;
                    //calling data access layer method                
                    synchDAL.AddUpdateBackendSynch(backendsynch);
                }
                else
                {
                    SynchEntity newbackendsynch = new SynchEntity();
                    newbackendsynch.PartitionKey = string.Concat(CoreConstants.AzureTables.BackendSynchPK, userbackend.UserID);
                    newbackendsynch.RowKey = string.Concat(CoreConstants.AzureTables.BackendSynchPK,userbackend.BackendID);
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
                    approvalstatus = CoreConstants.AzureTables.Waiting;
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
                    approvalstatus = CoreConstants.AzureTables.Waiting;
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
    }
}