//-----------------------------------------------------------
// <copyright file="PersonalizationAPIController.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using adidas.clb.MobileApproval.Exceptions;
using adidas.clb.MobileApproval.Models;
using adidas.clb.MobileApproval.Utility;
using adidas.clb.MobileApproval.App_Code.BL.Synch;

namespace adidas.clb.MobileApproval.Controllers
{
    /// <summary>
    /// The controller class which implements action methods for Synchapi
    /// </summary>
    [Authorize]
    public class SyncAPIController : ApiController
    {
        //Application insights interface reference for logging the error details into Application Insight azure service.
        static IAppInsight InsightLogger { get { return AppInsightLogger.Instance; } }
        public static string urgentTaskCalcColumnForCar = Convert.ToString(ConfigurationManager.AppSettings["UrgentTaskConditionForCAR_Column"]);
        public static string urgentTaskCalcColumnForBPM = Convert.ToString(ConfigurationManager.AppSettings["UrgentTaskConditionForBPMOnline_Column"]);
        public static string carDBName = Convert.ToString(ConfigurationManager.AppSettings["CARDBName"]);
        public static string bpmDBName = Convert.ToString(ConfigurationManager.AppSettings["BPMDBName"]);
        public static int urgentTaskCalcDaysForCAR = Convert.ToInt32(ConfigurationManager.AppSettings["UrgentTaskConditionForCAR_NumberOfDays"]);
        public static int urgentTaskCalcDaysForBPM = Convert.ToInt32(ConfigurationManager.AppSettings["UrgentTaskConditionForBPMOnline_NumberOfDays"]);
        public static int defaultCompletedTaskSyncDays = Convert.ToInt32(ConfigurationManager.AppSettings["DefaultCompletedTaskSyncDays"]);
        public static string taskArrStr = Convert.ToString(ConfigurationManager.AppSettings["TaskStatusArr"]);        //
        public static string requestStatusArr = Convert.ToString(ConfigurationManager.AppSettings["RequestStatusArr"]);
        public static int timeoutperiod = Convert.ToInt32(ConfigurationManager.AppSettings["timeoutperiod"]);
        private static string approvedTaskStatus = Convert.ToString(ConfigurationManager.AppSettings["ApprovedTaskStatus"]);
        private static string rejectedTaskStatus = Convert.ToString(ConfigurationManager.AppSettings["RejectedTaskStatus"]);
        private static string waitingTaskStatus = Convert.ToString(ConfigurationManager.AppSettings["WaitingTaskStatus"]);
        private static string urgentTaskStatus = Convert.ToString(ConfigurationManager.AppSettings["UrgentTaskStatus"]);
        private static string completedTaskStatus = Convert.ToString(ConfigurationManager.AppSettings["CompletedTaskStatus"]);
        private static string pendingTaskStatus = Convert.ToString(ConfigurationManager.AppSettings["PendingTaskStatus"]);

        public static List<string> lstTaskStatus = SharedData.ReturnTaskStatus(taskArrStr);
        /// <summary>
        /// action method to get backends associated to user, each one indicating the count of current open requests
        /// </summary>
        /// <returns>Returns list of backends</returns>        
        [Route("api/synch/users/{userID}/backends")]
        public HttpResponseMessage PostBackends(SynchRequestDTO query, string userID)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{userID}/backends, action: starting method");
                int syncTime = 0;
                DateTime curTimeStamp = DateTime.Now;
                //check null for input query
                if (query != null)
                {
                    Synch synch = new Synch();
                    List<string> userbackends = query.parameters.filters.backends;
                    //get userbackends associated to user
                    List<UserBackendEntity> allUserBackends = synch.GetUserBackendsList(userID, userbackends);
                    //get requests associated to user
                    //List<RequestEntity> requestslist = synch.GetUserRequests(userID, query.parameters.filters.reqStatus);
                    //get approvals associated to user based on approval status
                    int completedTaskSyncDays;
                    //get completed Tasks configuration value from request
                    //InsightLogger.TrackEvent(Convert.ToString(query.parameters.filters.CompletedRequestsSync));
                    if (!string.IsNullOrEmpty(Convert.ToString(query.parameters.filters.CompletedRequestsSync)) && query.parameters.filters.CompletedRequestsSync != 0)
                    {
                        completedTaskSyncDays = query.parameters.filters.CompletedRequestsSync;
                    }
                    else
                    {
                        //if it is null get default value from configuration file
                        completedTaskSyncDays = defaultCompletedTaskSyncDays;
                    }
                    string apprStatus = query.parameters.filters.apprStatus;
                    List<ApprovalEntity> approvalslist = synch.GetUserApprovalsForCount(userID, apprStatus);
                    Boolean requestsunfulfilled = false;
                    Boolean requestsfulfilled = false;
                    bool IsForceUpdate = query.parameters.forceUpdate;
                    bool IsUrgentTask = query.parameters.filters.IsUrgent;

                    List<UserBackendDTO> userbackendlist = new List<UserBackendDTO>();
                    //check extended depth flag
                    if (Rules.ExtendedDepthperAllBackends(query, allUserBackends, Convert.ToInt32(ConfigurationManager.AppSettings[CoreConstants.Config.MaxSynchReplySize])))
                    {
                        InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{userID}/backends, action: check extended Depth, response: true");
                        InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{userID}/backends, action:looping through backends");
                        //loop all backends to check updated or not for synch
                        foreach (UserBackendEntity userbackend in allUserBackends)
                        {
                            //convert userbackend entity response userbackend dto object
                            UserBackendDTO userbackenddto = DataProvider.ResponseObjectMapper<UserBackendDTO, UserBackendEntity>(userbackend);
                            Backend backenddto = DataProvider.ResponseObjectMapper<Backend, UserBackendEntity>(userbackend);
                            userbackenddto.backend = backenddto;
                            //approvals list associated to userbackend
                            List<ApprovalEntity> userbackendapprovalslist = new List<ApprovalEntity>();
                            if (approvalslist != null && approvalslist.Count > 0)
                            {
                                //get urgent tasks based on condition
                                if (IsUrgentTask && apprStatus == CoreConstants.AzureTables.Urgent)
                                {
                                    if (userbackend.BackendID == carDBName)
                                    {
                                        // if the backend is car then pull the urgent tasks based on createdDate + { No. of days fro config} <= DateTime.Now                           
                                        userbackendapprovalslist = approvalslist.Where(x => x.BackendID == userbackend.BackendID && x.Created != null && (x.Created.Value.Date.AddDays(urgentTaskCalcDaysForCAR).Date < DateTime.Now.Date)).ToList();
                                    }
                                    else if (userbackend.BackendID == bpmDBName)
                                    {
                                        // if the backend is store then pull the urgent tasks based on Duedate + { No. of days fro config} <= DateTime.Now
                                        userbackendapprovalslist = approvalslist.Where(x => x.BackendID == userbackend.BackendID && x.DueDate != null && (x.DueDate.Value.Date.AddDays(urgentTaskCalcDaysForBPM).Date < DateTime.Now.Date)).ToList();
                                    }
                                }
                                else
                                {
                                    //pull completed tasks based on config value
                                    if (apprStatus == CoreConstants.AzureTables.Approved || apprStatus == CoreConstants.AzureTables.Rejected)
                                    {
                                        // InsightLogger.TrackEvent("completedTaskSyncDays : " + completedTaskSyncDays);
                                        userbackendapprovalslist = approvalslist.Where(x => x.BackendID == userbackend.BackendID && x.DecisionDate != null && (x.DecisionDate.Value.Date >= DateTime.Now.AddDays(-(completedTaskSyncDays)).Date)).ToList();

                                    }
                                    else//pull pending tasks
                                    {
                                        userbackendapprovalslist = approvalslist.Where(x => x.BackendID == userbackend.BackendID).ToList();
                                    }

                                }
                            }

                            int requestsCount = 0;
                            if (userbackendapprovalslist != null && userbackendapprovalslist.Count > 0)
                            {
                                requestsCount = userbackendapprovalslist.Count;
                            }
                            ApprovalsCountDTO approvalcountdto = new ApprovalsCountDTO();
                            InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{userID}/backends, action: Adding approval count to response, response: success, backendID:" + userbackend.BackendID);
                            approvalcountdto.BackendID = userbackend.BackendID;
                            approvalcountdto.Status = query.parameters.filters.apprStatus;
                            approvalcountdto.Count = requestsCount;
                            userbackenddto.approvalsCount = approvalcountdto;
                            // List<ApprovalRequestDTO> approvalrequestlist = new List<ApprovalRequestDTO>();
                            //get requests associated to each user backend
                            //List<RequestEntity> userbackendrequestslist = requestslist.Where(x => x.BackendID == userbackend.BackendID).ToList();
                            //check if backend updated
                            if (Rules.IsBackendUpdated(userbackend, query))
                            {
                                InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{userID}/backends, action: check backend update, response: true");
                                //if extended depth is userbackend level
                                if (Rules.ExtendedDepthperBackend(userbackend, Convert.ToInt32(ConfigurationManager.AppSettings[CoreConstants.Config.MaxSynchReplySize])))
                                {
                                    InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{userID}/backends, action: check extended depth, response: true");
                                    //commented code related to adding requests and tasks to response
                                    //userbackenddto = synch.AddRequsetsTasksCountToSynchResponse(userbackendrequestslist, userbackendapprovalslist, userbackend, query, userbackenddto);

                                }
                                //check requestsfullfilled flag
                                if (!requestsfulfilled)
                                {
                                    //code to clear extended depth flags
                                }
                                //check requestsunfullfilled flag
                                if (!requestsunfulfilled)
                                {
                                    //code to update backend synch timestamp                                    ;
                                }
                                synch.AddUpdateBackendSynch(userbackend);
                            }
                            else
                            {
                                InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{userID}/backends, action: check backend update, response: false");
                                SynchDTO synchdto = new SynchDTO();
                                //check if backend update is in progress in service layer then send the latency in response
                                if (Rules.IsBackendUpdateInProgress(userbackend))
                                {
                                    InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{userID}/backends, action: check in-progress backend update, response: true");
                                    synchdto.retryAfter = userbackend.ExpectedLatency;
                                    InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{userID}/backends, action: add expected backend latency to response as retry time, response: success");
                                    userbackenddto.synch = synchdto;
                                }
                                else
                                {
                                    InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{userID}/backends, action: check in-progress backend update, response: false");
                                    //if update is not in progress, trigger it
                                    if (IsForceUpdate)
                                    {
                                        synch.TriggerUserBackendUpdate(userbackend, true);
                                        //call missing update function in async
                                        Task.Factory.StartNew(() =>
                                        {
                                            synch.CollectUsersMissedUpdatesByBackend(userbackend, curTimeStamp);
                                        });

                                    }
                                    else
                                    {
                                        synch.TriggerUserBackendUpdate(userbackend, false);
                                    }

                                    InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{userID}/backends, action: trigger a backend update, response: success");
                                    synchdto.retryAfter = Convert.ToInt32(Rules.BackendRetryTime(userbackend));
                                    InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{userID}/backends, action: add backend retry time to response, response: success");
                                    userbackenddto.synch = synchdto;
                                }
                                //commented code related to adding requests and tasks to response
                                //userbackenddto = synch.AddRequsetsTasksCountToSynchResponse(userbackendrequestslist, userbackendapprovalslist, userbackend, query, userbackenddto);

                            }
                            //add each userbackend to list
                            userbackendlist.Add(userbackenddto);
                        }

                    }
                    // if it is force update then need to calculate sync time
                    if (IsForceUpdate)
                    {
                        //calculate sync time
                        syncTime = synch.CalcSynchTime(allUserBackends);
                        //add sync time value to SynchTimeResponseDTO object
                        SynchTimeResponseDTO response = new SynchTimeResponseDTO();
                        response.SyncTime = syncTime;
                        //call missing update function

                        InsightLogger.TrackEvent("SyncAPIController ::" + callerMethodName + " method execution has been Completed.");
                        return Request.CreateResponse(HttpStatusCode.OK, response);
                    }
                    else
                    {
                        SynchResponseDTO<UserBackendDTO> response = new SynchResponseDTO<UserBackendDTO>();
                        response.result = userbackendlist;
                        response.SyncTime = syncTime;
                        InsightLogger.TrackEvent("SyncAPIController ::" + callerMethodName + " method execution has been Completed.");
                        return Request.CreateResponse(HttpStatusCode.OK, response);
                    }


                }
                else
                {
                    InsightLogger.TrackEvent("SyncAPIController ::" + callerMethodName + " method execution has been Completed.");
                    return Request.CreateResponse(HttpStatusCode.BadRequest, DataProvider.SynchResponseError<UserBackendDTO>("400", "input query is not sent to get user associated backends", ""));
                }
            }
            catch (DataAccessException dalexception)
            {
                InsightLogger.Exception(dalexception.Message, dalexception, callerMethodName);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.SynchResponseError<UserBackendDTO>("400", dalexception.Message, dalexception.Message));
            }
            catch (BusinessLogicException blexception)
            {
                InsightLogger.Exception(blexception.Message, blexception, callerMethodName);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.SynchResponseError<UserBackendDTO>("400", blexception.Message, blexception.Message));
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                //LoggerHelper.WriteToLog(exception + " - exception in controller action while retrieving associated user backends : "
                //      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.SynchResponseError<UserBackendDTO>("400", exception.Message, exception.StackTrace));
            }
        }

        /// <summary>
        /// action method to get list of requests from one of the backends associated to a user
        /// </summary>
        /// <returns>Returns list of requets for userbackend</returns>        
        [Route("api/synch/users/{userID}/backends/{usrBackendID}/requests")]
        public HttpResponseMessage PostUserBackendRequests(SynchRequestDTO query, string userID, string usrBackendID)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{userID}/backends/{usrBackendID}/requests, action: method start");
                //check null for input query
                if (query != null)
                {
                    Synch synch = new Synch();
                    //get completed tasks sync count
                    int completedTaskSyncDays;
                    //get completed Tasks configuration value from request                    
                    if (!string.IsNullOrEmpty(Convert.ToString(query.parameters.filters.CompletedRequestsSync)) && query.parameters.filters.CompletedRequestsSync != 0)
                    {
                        completedTaskSyncDays = query.parameters.filters.CompletedRequestsSync;
                    }
                    else
                    {
                        //if it is null get default value from configuration file
                        completedTaskSyncDays = defaultCompletedTaskSyncDays;
                    }
                    //get approval status
                    string apprStatus = query.parameters.filters.apprStatus;
                    //get userbackend associated to user with backendid
                    UserBackendEntity userbackend = synch.GetUserBackend(userID, usrBackendID);
                    //get requests associated to userbackend
                    // List<RequestEntity> requestslist = synch.GetUserBackendRequests(userID, usrBackendID, query.parameters.filters.reqStatus);
                    //get approvals associated to userbackend
                    List<ApprovalEntity> approvalslist = synch.GetUserBackendApprovals(userID, usrBackendID, apprStatus);

                    if (approvalslist != null && approvalslist.Count > 0)
                    {
                        //filter approvalist based on status
                        //Completed tasks(either approved or rejected)
                        if (apprStatus == approvedTaskStatus || apprStatus == rejectedTaskStatus)
                        {

                            approvalslist = approvalslist.Where(x => (x.DecisionDate != null && x.DecisionDate.Value.Date >= DateTime.Now.AddDays(-(completedTaskSyncDays)).Date)).ToList();
                            //update task status property as compelted
                            approvalslist.ForEach(apptask => apptask.TaskStatus = completedTaskStatus);

                        }
                        //urgent tasks
                        else if (apprStatus == urgentTaskStatus)
                        {
                            if (userbackend.BackendID == carDBName)
                            {
                                // if the backend is car then pull the urgent tasks based on createdDate + { No. of days fro config} <= DateTime.Now                           
                                approvalslist = approvalslist.Where(x => x.BackendID == userbackend.BackendID && x.Created != null && (x.Created.Value.Date.AddDays(urgentTaskCalcDaysForCAR).Date < DateTime.Now.Date)).ToList();

                            }
                            else if (userbackend.BackendID == bpmDBName)
                            {
                                // if the backend is store then pull the urgent tasks based on Duedate + { No. of days fro config} <= DateTime.Now
                                approvalslist = approvalslist.Where(x => x.BackendID == userbackend.BackendID && x.DueDate != null && (x.DueDate.Value.Date.AddDays(urgentTaskCalcDaysForBPM).Date < DateTime.Now.Date)).ToList();
                            }
                            //update task status property as Urgent
                            approvalslist.ForEach(apptask => apptask.TaskStatus = urgentTaskStatus);
                        }
                        //pending tasks
                        else if (apprStatus == waitingTaskStatus)
                        {
                            if (userbackend.BackendID == carDBName)
                            {
                                // if the backend is car then pull the pending tasks based on createdDate + { No. of days fro config} >= DateTime.Now                           
                                approvalslist = approvalslist.Where(x => x.BackendID == userbackend.BackendID && x.Created != null && (x.Created.Value.Date.AddDays(urgentTaskCalcDaysForCAR).Date >= DateTime.Now.Date)).ToList();
                            }
                            else if (userbackend.BackendID == bpmDBName)
                            {
                                // if the backend is store then pull the pending tasks based on Duedate + { No. of days fro config} >= DateTime.Now
                                approvalslist = approvalslist.Where(x => x.BackendID == userbackend.BackendID && x.DueDate != null && (x.DueDate.Value.Date.AddDays(urgentTaskCalcDaysForBPM).Date >= DateTime.Now.Date)).ToList();
                            }
                            //update task status property as Pending
                            approvalslist.ForEach(apptask => apptask.TaskStatus = pendingTaskStatus);
                        }
                    }
                    //use mmapper to convert userbackend entity to userbackend data transfer object
                    UserBackendDTO userbackenddto = DataProvider.ResponseObjectMapper<UserBackendDTO, UserBackendEntity>(userbackend);
                    Backend backenddto = DataProvider.ResponseObjectMapper<Backend, UserBackendEntity>(userbackend);
                    userbackenddto.backend = backenddto;
                    List<ApprovalRequestDTO> approvalrequestlist = new List<ApprovalRequestDTO>();
                    Boolean requestsunfulfilled = false;
                    Boolean requestsfulfilled = false;
                    //adding approval tasks list to response
                    //loop through each approval in the userbackend
                    foreach (ApprovalEntity approval in approvalslist)
                    {
                        ApprovalRequestDTO approvalrequest = new ApprovalRequestDTO();
                        ApprovalDTO approvaldto = new ApprovalDTO();
                        approvaldto = DataProvider.ResponseObjectMapper<ApprovalDTO, ApprovalEntity>(approval);
                        approvalrequest.approval = approvaldto;
                        //add approval request to list which will be added to corresponding backend
                        approvalrequestlist.Add(approvalrequest);
                    }

                    //check if backend updated
                    if (Rules.IsBackendUpdated(userbackend, query))
                    {
                        InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{userID}/backends/{usrBackendID}/requests, action: check backend update, response: true,");
                        //if extended depth is userbackend level
                        if (Rules.ExtendedDepthperBackend(userbackend, Convert.ToInt32(ConfigurationManager.AppSettings[CoreConstants.Config.MaxSynchReplySize])))
                        {
                            InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{userID}/backends/{usrBackendID}/requests, action: check extended depth, response: true");
                            InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{userID}/backends/{usrBackendID}/requests, action:Loop through all requests in backend");
                            //approvalrequestlist=synch.AddRequsetsTasksToSynchResponse(requestslist,approvalslist,userbackend,query);                            
                        }
                        //check requestsfullfilled flag
                        if (requestsfulfilled)
                        {
                            //code to clear extended depth flags
                        }
                        //check requestsunfullfilled flag
                        if (!requestsunfulfilled)
                        {
                            //code to update backend synch timestamp                           
                        }
                        synch.AddUpdateBackendSynch(userbackend);
                    }
                    else
                    {
                        InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{userID}/backends/{usrBackendID}/requests, action: check backend update, response: false");
                        SynchDTO synchdto = new SynchDTO();
                        //check if backend update is in progress in service layer then send the latency in response
                        if (Rules.IsBackendUpdateInProgress(userbackend))
                        {
                            InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{userID}/backends/{usrBackendID}/requests, action: check in-progress backend update , response: true");
                            synchdto.retryAfter = userbackend.ExpectedLatency;
                            InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{userID}/backends/{usrBackendID}/requests, action: add expected backend latency to response as retrytime, response: success");
                            userbackenddto.synch = synchdto;
                        }
                        else
                        {
                            InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{userID}/backends/{usrBackendID}/requests, action: check backend update in-progress, response: false");
                            //if update is not in progress, trigger it
                            synch.TriggerUserBackendUpdate(userbackend, false);
                            InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{userID}/backends/{usrBackendID}/requests, action: trigger a backend update, response: succses");
                            synchdto.retryAfter = Convert.ToInt32(Rules.BackendRetryTime(userbackend));
                            InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{userID}/backends/{usrBackendID}/requests, action: add backend retry time to response, response: success");
                            userbackenddto.synch = synchdto;
                        }
                        //approvalrequestlist = synch.AddRequsetsTasksToSynchResponse(requestslist, approvalslist, userbackend, query);
                    }
                    //add requests list to user backend
                    userbackenddto.requests = approvalrequestlist;
                    List<UserBackendDTO> userbackendlist = new List<UserBackendDTO>();
                    //add userbackend to userbackend list to add to response
                    userbackendlist.Add(userbackenddto);
                    SynchResponseDTO<UserBackendDTO> response = new SynchResponseDTO<UserBackendDTO>();
                    response.result = userbackendlist;
                    InsightLogger.TrackEvent("SyncAPIController ::" + callerMethodName + " method execution has been Completed.");
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                else
                {
                    InsightLogger.TrackEvent("SyncAPIController ::" + callerMethodName + " method execution has been Completed.");
                    return Request.CreateResponse(HttpStatusCode.BadRequest, DataProvider.SynchResponseError<UserBackendDTO>("400", "input query is not sent to get requests for user associated backend", ""));
                }
            }
            catch (DataAccessException dalexception)
            {
                InsightLogger.Exception(dalexception.Message, dalexception, callerMethodName);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.SynchResponseError<UserBackendDTO>("400", dalexception.Message, dalexception.Message));
            }
            catch (BusinessLogicException blexception)
            {
                InsightLogger.Exception(blexception.Message, blexception, callerMethodName);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.SynchResponseError<UserBackendDTO>("400", blexception.Message, blexception.Message));
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                //LoggerHelper.WriteToLog(exception + " - exception in controller action while retrieving requests per userbackend : "
                //      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.SynchResponseError<UserBackendDTO>("400", exception.Message, exception.StackTrace));
            }
        }

        /// <summary>
        /// action method to get specific request
        /// </summary>
        /// <returns>Returns requet</returns>        
        [Route("api/synch/requests/{apprReqID}")]
        public HttpResponseMessage PostRequest(SynchRequestDTO query, string apprReqID)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                InsightLogger.TrackEvent("SyncAPIController ::endpoint - api/synch/requests/{apprReqID}, action: method start");
                //check null for input query
                if (query != null)
                {
                    Synch synch = new Synch();
                    //get requests with requestid
                    RequestEntity requestentity = synch.GetRequest(query.userId, apprReqID);
                    //get fileds associated to request
                    List<FieldDTO> fields = synch.GetFields(apprReqID);
                    ApprovalRequestDTO approvalrequest = new ApprovalRequestDTO();
                    //get userbackend
                    UserBackendEntity userbackend = synch.GetUserBackend(query.userId, requestentity.BackendID);
                    //check request for update
                    if (Rules.IsRequestUpdated(requestentity, userbackend.DefaultUpdateFrequency))
                    {
                        InsightLogger.TrackEvent("SyncAPIController ::endpoint - api/synch/requests/{apprReqID}, action: is request upto date, response:true");
                        //get request synch entity
                        RequestSynchEntity requestsynch = synch.GetRequestSynch(requestentity);
                        //check if requests which have changed since the last synch need to send in response or all requests.
                        if (Rules.IsRequestATarget(query, requestentity, requestsynch))
                        {
                            InsightLogger.TrackEvent("SyncAPIController ::endpoint - api/synch/requests/{apprReqID}, action: is request target, response:true");
                            InsightLogger.TrackEvent("SyncAPIController ::endpoint - api/synch/requests/{apprReqID}, action: add request details to response, response:success, RequsetID:" + apprReqID);
                            RequestDTO requestdto = DataProvider.ResponseObjectMapper<RequestDTO, RequestEntity>(requestentity);
                            //fill requester details to request dto
                            if (!string.IsNullOrEmpty(requestentity.RequesterName))
                            {
                                Requester requesterdetails = new Requester();
                                requesterdetails.UserID = requestentity.RequesterID;
                                requesterdetails.Name = requestentity.RequesterName;
                                requestdto.Requester = requesterdetails;
                            }
                            //add fields to request
                            Fields fielddto = new Fields();
                            fielddto.Overview = fields;
                            requestdto.Fields = fielddto;
                            approvalrequest.request = requestdto;
                            //if extended depth is request level
                            if (Rules.ExtendedDepth(userbackend, Convert.ToInt32(ConfigurationManager.AppSettings[CoreConstants.Config.MaxSynchReplySize])))
                            {
                                //code to populate extended depth
                            }
                            else
                            {
                                //code to clear extended depth flags
                            }
                            InsightLogger.TrackEvent("SyncAPIController ::endpoint - api/synch/requests/{apprReqID}, action: update request last synch time, response:success");
                            //code update request synch timestamp
                            synch.AddUpdateRequestSynch(requestentity, requestsynch, query.userId);
                        }
                    }
                    else
                    {
                        InsightLogger.TrackEvent("SyncAPIController ::endpoint - api/synch/requests/{apprReqID}, action: is request up to date, response:false");
                        //check if backend update is in progress in service layer then send the latency in response
                        if (Rules.IsRequestUpdateInProgress(requestentity))
                        {
                            InsightLogger.TrackEvent("SyncAPIController ::endpoint - api/synch/requests/{apprReqID}, action: update in progress, response:true");
                            approvalrequest.retryAfter = requestentity.ExpectedLatency;
                        }
                        else
                        {
                            InsightLogger.TrackEvent("SyncAPIController ::endpoint - api/synch/requests/{apprReqID}, action: update in progress, response:false");
                            //if update is not in progress, trigger it
                            synch.TriggerRequestUpdate(requestentity, query.userId);
                            InsightLogger.TrackEvent("SyncAPIController ::endpoint - api/synch/requests/{apprReqID}, action: trigger request update, response:success");
                            approvalrequest.retryAfter = Convert.ToInt32(Rules.RequestRetryTime(userbackend));
                        }
                        //code to clear extended depth flags

                        InsightLogger.TrackEvent("SyncAPIController ::endpoint - api/synch/requests/{apprReqID}, action: add request details to response, response:success, RequsetID:" + apprReqID);
                        RequestDTO requestdto = DataProvider.ResponseObjectMapper<RequestDTO, RequestEntity>(requestentity);
                        //fill requester details to request dto
                        if (!string.IsNullOrEmpty(requestentity.RequesterName))
                        {
                            Requester requesterdetails = new Requester();
                            requesterdetails.UserID = requestentity.RequesterID;
                            requesterdetails.Name = requestentity.RequesterName;
                            requestdto.Requester = requesterdetails;
                        }
                        //add fields to request
                        Fields fielddto = new Fields();
                        fielddto.Overview = fields;
                        requestdto.Fields = fielddto;
                        approvalrequest.request = requestdto;
                    }
                    //add request to list to send it in response
                    List<ApprovalRequestDTO> approvalrequestlist = new List<ApprovalRequestDTO>();
                    approvalrequestlist.Add(approvalrequest);
                    SynchResponseDTO<ApprovalRequestDTO> response = new SynchResponseDTO<ApprovalRequestDTO>();
                    response.result = approvalrequestlist;
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                else
                {
                    InsightLogger.TrackEvent("SyncAPIController :: input query is null");
                    return Request.CreateResponse(HttpStatusCode.BadRequest, DataProvider.SynchResponseError<UserBackendDTO>("400", "input query is not sent to get request", ""));
                }
            }
            catch (DataAccessException dalexception)
            {
                InsightLogger.Exception(dalexception.Message, dalexception, callerMethodName);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.SynchResponseError<UserBackendDTO>("400", dalexception.Message, dalexception.Message));
            }
            catch (BusinessLogicException blexception)
            {
                InsightLogger.Exception(blexception.Message, blexception, callerMethodName);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.SynchResponseError<UserBackendDTO>("400", blexception.Message, blexception.Message));
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                //LoggerHelper.WriteToLog(exception + " - exception in controller action while retrieving request with requsetID: "
                //      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.SynchResponseError<UserBackendDTO>("400", exception.Message, exception.StackTrace));
            }
        }

        /// <summary>
        /// action method to get list of approvers for a specific request
        /// </summary>
        /// <returns>Returns list of approvers for request</returns>        
        [Route("api/synch/requests/{apprReqID}/approvers")]
        public HttpResponseMessage PostApprovers(SynchRequestDTO query, string apprReqID)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                InsightLogger.TrackEvent("SyncAPIController ::endpoint - api/synch/requests/{apprReqID}/approvers, action: method start");
                //check null for input query
                if (query != null)
                {
                    Synch synch = new Synch();
                    SynchResponseDTO<ApproverDTO> SynchResponse = new SynchResponseDTO<ApproverDTO>();
                    //get requests with requestid
                    RequestEntity requestentity = synch.GetRequest(query.userId, apprReqID);
                    //get userbackend
                    UserBackendEntity userbackend = synch.GetUserBackend(query.userId, requestentity.BackendID);
                    //get approvers associated to request
                    List<ApproverDTO> approvers = synch.GetApprovers(apprReqID);
                    double retrytime = 0;
                    //check request for update
                    if (Rules.IsRequestUpdated(requestentity, userbackend.DefaultUpdateFrequency))
                    {
                        InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/requests/{apprReqID}/approvers, action: check request update, response:true");
                        //get request synch entity
                        RequestSynchEntity requestsynch = synch.GetRequestSynch(requestentity);
                        //check if requests which have changed since the last synch need to send in response or all requests.
                        if (Rules.IsRequestATarget(query, requestentity, requestsynch))
                        {
                            InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/requests/{apprReqID}/approvers, action: checking is request target, response:true");
                            InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/requests/{apprReqID}/approvers, action: add approvers to response, response: succuess");
                            SynchResponse.query = query;
                            SynchResponse.result = approvers;
                            InsightLogger.TrackEvent("SyncAPIController ::endpoint - api/synch/requests/{apprReqID}/approvers action:updating synch timestamp");
                            //update request synch timestamp
                            synch.AddUpdateRequestSynch(requestentity, requestsynch, query.userId);
                        }
                    }
                    else
                    {
                        InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/requests/{apprReqID}/approvers, action:request   update progress, responser: true");
                        //check if backend update is in progress in service layer then send the latency in response
                        if (Rules.IsRequestUpdateInProgress(requestentity))
                        {
                            InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/requests/{apprReqID}/approvers action:adding expected latency , response: success ");
                            retrytime = requestentity.ExpectedLatency;
                        }
                        else
                        {
                            InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/requests/{apprReqID}/approvers, action:request update progress, response:false");
                            InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/requests/{apprReqID}/approvers, action:trigger update for request, response:success");
                            //if update is not in progress, trigger it
                            //commented updatetriggering to remove duplicate triggering as get requestdeatils, get approvers end points calling at same time from client.
                            //synch.TriggerRequestUpdate(requestentity, query.userId);
                            retrytime = Rules.RequestRetryTime(userbackend);
                            InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/requests/{apprReqID}/approvers, action:add retry time, response:success");
                        }
                        InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/requests/{apprReqID}/approvers, action: add approvers to response, response: succuess");
                        SynchResponse.result = approvers;
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, SynchResponse);
                }
                else
                {
                    InsightLogger.TrackEvent("SyncAPIController ::input query is null");
                    return Request.CreateResponse(HttpStatusCode.BadRequest, DataProvider.SynchResponseError<UserBackendDTO>("400", "input query is not sent to get approvers for request", ""));
                }
            }
            catch (DataAccessException dalexception)
            {
                InsightLogger.Exception(dalexception.Message, dalexception, callerMethodName);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.SynchResponseError<UserBackendDTO>("400", dalexception.Message, dalexception.Message));
            }
            catch (BusinessLogicException blexception)
            {
                InsightLogger.Exception(blexception.Message, blexception, callerMethodName);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.SynchResponseError<UserBackendDTO>("400", blexception.Message, blexception.Message));
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                //LoggerHelper.WriteToLog(exception + " - exception in controller action while retrieving approvers per request : "
                //      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.SynchResponseError<UserBackendDTO>("400", exception.Message, exception.StackTrace));
            }
        }

        /// <summary>
        /// action method to get PDF document of a specific request for request details
        /// </summary>
        /// <returns>Returns pdf with request details</returns>        
        [Route("api/synch/requests/{apprReqID}/details")]
        public HttpResponseMessage PostRequestDetails(SynchRequestDTO query, string apprReqID)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/requests/{apprReqID}/details, action: start method");
                Synch synch = new Synch();

                //get requests with requestid
                RequestEntity requestentity = synch.GetRequest(query.userId, apprReqID);
                if (!string.IsNullOrEmpty(requestentity.PDFUri))
                {
                    InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/requests/{apprReqID}/details, action:getting request pdfuri ,response:success, RequestID:" + apprReqID);
                    Uri saspdfuri = synch.GetSASPdfUri(requestentity.PDFUri);
                    return Request.CreateResponse(HttpStatusCode.OK, saspdfuri.ToString());
                }
                else
                {
                    InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/requests/{apprReqID}/details, action:getting request pdfuri ,response:failed");
                    return null;
                }
            }
            catch (DataAccessException dalexception)
            {
                InsightLogger.Exception(dalexception.Message, dalexception, callerMethodName);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.SynchResponseError<UserBackendDTO>("400", dalexception.Message, dalexception.Message));
            }
            catch (BusinessLogicException blexception)
            {
                InsightLogger.Exception(blexception.Message, blexception, callerMethodName);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.SynchResponseError<UserBackendDTO>("400", blexception.Message, blexception.Message));
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                //LoggerHelper.WriteToLog(exception + " - exception in controller action while retrieving request details as pdf : "
                //      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.SynchResponseError<UserBackendDTO>("400", exception.Message, exception.StackTrace));
            }
        }

        /// <summary>
        /// action method to get list of approvals from one of the backends associated to a user
        /// </summary>
        /// <returns>Returns list of approvals for userbackend</returns>        
        [Route("api/synch/users/{userID}/backends/{usrBackendID}/approvals")]
        public HttpResponseMessage PostUserBackendApprovals(SynchRequestDTO query, string userID, string usrBackendID)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{userID}/backends/{usrBackendID}/requests, action: method start");
                //check null for input query
                if (query != null)
                {
                    Synch synch = new Synch();
                    //get userbackend associated to user with backendid
                    UserBackendEntity userbackend = synch.GetUserBackend(userID, usrBackendID);
                    //get approvals associated to userbackend
                    List<ApprovalEntity> approvalslist = synch.GetUserBackendApprovals(userID, usrBackendID, query.parameters.filters.apprStatus);
                    //use mmapper to convert userbackend entity to userbackend data transfer object
                    UserBackendDTO userbackenddto = DataProvider.ResponseObjectMapper<UserBackendDTO, UserBackendEntity>(userbackend);
                    Backend backenddto = DataProvider.ResponseObjectMapper<Backend, UserBackendEntity>(userbackend);
                    userbackenddto.backend = backenddto;
                    List<ApprovalRequestDTO> approvalrequestlist = new List<ApprovalRequestDTO>();
                    //check if backend updated
                    //if (Rules.IsBackendUpdated(userbackend, query))
                    //{
                    InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{userID}/backends/{usrBackendID}/requests, action: check backend update, response: true,");
                    //if extended depth is userbackend level
                    //if (Rules.ExtendedDepthperBackend(userbackend, Convert.ToInt32(ConfigurationManager.AppSettings[CoreConstants.Config.MaxSynchReplySize])))
                    //{
                    InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{userID}/backends/{usrBackendID}/requests, action: check extended depth, response: true");

                    //loop through each approval in the userbackend
                    foreach (ApprovalEntity approval in approvalslist)
                    {
                        ApprovalRequestDTO approvalrequest = new ApprovalRequestDTO();
                        ApprovalDTO approvaldto = new ApprovalDTO();
                        approvaldto = DataProvider.ResponseObjectMapper<ApprovalDTO, ApprovalEntity>(approval);
                        approvalrequest.approval = approvaldto;
                        //add approval request to list which will be added to corresponding backend
                        approvalrequestlist.Add(approvalrequest);
                    }
                    InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{userID}/backends/{usrBackendID}/requests, action: adding approval tasks list, response: success");
                    //    }
                    //}
                    if (!Rules.IsBackendUpdated(userbackend, query))
                    {
                        InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{userID}/backends/{usrBackendID}/requests, action: check backend update, response: false");
                        SynchDTO synchdto = new SynchDTO();
                        //check if backend update is in progress in service layer then send the latency in response
                        if (Rules.IsBackendUpdateInProgress(userbackend))
                        {
                            InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{userID}/backends/{usrBackendID}/requests, action: check backend update in-progress, response: true");
                            synchdto.retryAfter = userbackend.ExpectedLatency;
                            InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{userID}/backends/{usrBackendID}/requests, action: add expected latency to response, response: success");
                            userbackenddto.synch = synchdto;
                        }
                        else
                        {
                            InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{userID}/backends/{usrBackendID}/requests, action: check backend update in-progress, response: false");
                            //if update is not in progress, trigger it
                            synch.TriggerUserBackendUpdate(userbackend, false);
                            InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{userID}/backends/{usrBackendID}/requests, action: trigger update backend, response: succses");
                            synchdto.retryAfter = Convert.ToInt32(Rules.BackendRetryTime(userbackend));
                            InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{userID}/backends/{usrBackendID}/requests, action: add retry time to response, response: success");
                            userbackenddto.synch = synchdto;
                        }
                    }
                    //add requests list to user backend
                    userbackenddto.requests = approvalrequestlist;
                    List<UserBackendDTO> userbackendlist = new List<UserBackendDTO>();
                    //add userbackend to userbackend list to add to response
                    userbackendlist.Add(userbackenddto);
                    SynchResponseDTO<UserBackendDTO> response = new SynchResponseDTO<UserBackendDTO>();
                    response.result = userbackendlist;
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                else
                {
                    InsightLogger.TrackEvent("SyncAPIController ::" + callerMethodName + " method execution has been Completed.");
                    return Request.CreateResponse(HttpStatusCode.BadRequest, DataProvider.SynchResponseError<UserBackendDTO>("400", "input query is not sent to get requests for user associated backend", ""));
                }
            }
            catch (DataAccessException dalexception)
            {
                InsightLogger.Exception(dalexception.Message, dalexception, callerMethodName);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.SynchResponseError<UserBackendDTO>("400", dalexception.Message, dalexception.Message));
            }
            catch (BusinessLogicException blexception)
            {
                InsightLogger.Exception(blexception.Message, blexception, callerMethodName);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.SynchResponseError<UserBackendDTO>("400", blexception.Message, blexception.Message));
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                //LoggerHelper.WriteToLog(exception + " - exception in controller action while retrieving approvals per userbackend : "
                //      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.SynchResponseError<UserBackendDTO>("400", exception.Message, exception.StackTrace));
            }
        }


        /// <summary>
        /// action method to get backends associated to user, each one indicating the count of current open requests
        /// </summary>
        /// <returns>Returns list of backends</returns>        
        [Route("api/synch/users/{userID}/approvalscount")]
        public HttpResponseMessage PostApprovalsCount(SynchRequestDTO query, string userID)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{userID}/backends, action: starting method");
                //check null for input query
                if (query != null)
                {
                    Synch synch = new Synch();
                    List<string> userbackends = query.parameters.filters.backends;
                    bool IsUrgentTask = query.parameters.filters.IsUrgent;
                    string apprStatus = query.parameters.filters.apprStatus;
                    int completedTaskSyncDays;
                    if (!string.IsNullOrEmpty(Convert.ToString(query.parameters.filters.CompletedRequestsSync)) && query.parameters.filters.CompletedRequestsSync != 0)
                    {
                        completedTaskSyncDays = query.parameters.filters.CompletedRequestsSync;
                    }
                    else
                    {
                        //if it is null get default value from configuration file
                        completedTaskSyncDays = defaultCompletedTaskSyncDays;
                    }
                    //get userbackends associated to user
                    List<UserBackendEntity> allUserBackends = synch.GetUserBackendsList(userID, userbackends);
                    //get approvals associated to user absed on approval status
                    List<ApprovalEntity> approvalslist = synch.GetUserApprovalsForCount(userID, apprStatus);
                    List<ApprovalsCountDTO> approvalcountlist = new List<ApprovalsCountDTO>();
                    //check extended depth flag
                    if (Rules.ExtendedDepthperAllBackends(query, allUserBackends, Convert.ToInt32(ConfigurationManager.AppSettings[CoreConstants.Config.MaxSynchReplySize])))
                    {
                        InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{userID}/backends, action: check backend depth, response: true");
                        InsightLogger.TrackEvent("SyncAPIController :: looping through backends");
                        //loop all backends to check updated or not for synch
                        foreach (UserBackendEntity userbackend in allUserBackends)
                        {
                            //check if backend updated
                            //if (Rules.IsBackendUpdated(userbackend, query))
                            //{
                            InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{userID}/backends, action: check backend update, response: true");
                            List<ApprovalEntity> userbackendapprovalslist = new List<ApprovalEntity>();
                            if (approvalslist != null && approvalslist.Count > 0)
                            {
                                if (IsUrgentTask && apprStatus == CoreConstants.AzureTables.Urgent)
                                {

                                    if (userbackend.BackendID == carDBName)
                                    {
                                        userbackendapprovalslist = approvalslist.Where(x => x.BackendID == userbackend.BackendID && x.Created != null && (x.Created.Value.Date.AddDays(urgentTaskCalcDaysForCAR).Date < DateTime.Now.Date)).ToList();
                                    }
                                    else if (userbackend.BackendID == bpmDBName)
                                    {
                                        userbackendapprovalslist = approvalslist.Where(x => x.BackendID == userbackend.BackendID && x.DueDate != null && (x.DueDate.Value.Date.AddDays(urgentTaskCalcDaysForBPM).Date < DateTime.Now.Date)).ToList();
                                    }
                                }
                                else
                                {
                                    //pull completed tasks based on config value
                                    if (apprStatus == CoreConstants.AzureTables.Approved || apprStatus == CoreConstants.AzureTables.Rejected)
                                    {
                                        //InsightLogger.TrackEvent("completedTaskSyncDays : " + completedTaskSyncDays);
                                        userbackendapprovalslist = approvalslist.Where(x => x.BackendID == userbackend.BackendID && x.DecisionDate != null && (x.DecisionDate.Value.Date >= DateTime.Now.AddDays(-(completedTaskSyncDays)).Date)).ToList();

                                    }
                                    else//pull pending tasks
                                    {
                                        userbackendapprovalslist = approvalslist.Where(x => x.BackendID == userbackend.BackendID).ToList();
                                    }

                                }
                            }

                            int requestsCount = 0;
                            if (userbackendapprovalslist != null && userbackendapprovalslist.Count > 0)
                            {
                                requestsCount = userbackendapprovalslist.Count;
                            }
                            ApprovalsCountDTO approvalcountdto = new ApprovalsCountDTO();
                            InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{userID}/backends, action: Adding approval count to response, response: success, backendID:" + userbackend.BackendID);
                            approvalcountdto.BackendID = userbackend.BackendID;
                            approvalcountdto.Status = query.parameters.filters.apprStatus;

                            approvalcountdto.Count = requestsCount;
                            approvalcountlist.Add(approvalcountdto);
                            //
                            if (!Rules.IsBackendUpdated(userbackend, query))
                            {
                                InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{userID}/backends, action: check backend update, response: false");
                                SynchDTO synchdto = new SynchDTO();
                                //check if backend update is in progress in service layer then send the latency in response
                                if (Rules.IsBackendUpdateInProgress(userbackend))
                                {
                                    InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{userID}/backends, action: check backend update in-progress, response: true");
                                    synchdto.retryAfter = userbackend.ExpectedLatency;
                                    InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{userID}/backends, action: add expected latency to response, response: success");
                                    //userbackenddto.synch = synchdto;
                                }
                                else
                                {
                                    InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{userID}/backends, action: check backend update in-progress, response: false");
                                    //if update is not in progress, trigger it
                                    synch.TriggerUserBackendUpdate(userbackend, false);
                                    InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{userID}/backends, action: trigger a backend update, response: success");
                                    synchdto.retryAfter = Convert.ToInt32(Rules.BackendRetryTime(userbackend));
                                    InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{userID}/backends, action: add retry time to response, response: success");
                                    //userbackenddto.synch = synchdto;
                                }
                            }

                        }
                    }
                    SynchResponseDTO<ApprovalsCountDTO> response = new SynchResponseDTO<ApprovalsCountDTO>();
                    response.result = approvalcountlist;
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                else
                {
                    InsightLogger.TrackEvent("SyncAPIController :: input query is null");
                    return Request.CreateResponse(HttpStatusCode.BadRequest, DataProvider.SynchResponseError<UserBackendDTO>("400", "input query is not sent to get Approval Count for userbackends", ""));
                }
            }
            catch (DataAccessException dalexception)
            {
                InsightLogger.Exception(dalexception.Message, dalexception, callerMethodName);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.SynchResponseError<UserBackendDTO>("400", dalexception.Message, dalexception.Message));
            }
            catch (BusinessLogicException blexception)
            {
                InsightLogger.Exception(blexception.Message, blexception, callerMethodName);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.SynchResponseError<UserBackendDTO>("400", blexception.Message, blexception.Message));
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                //LoggerHelper.WriteToLog(exception + " - exception in controller action while retrieving approval count associated user backends : "
                //      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.SynchResponseError<UserBackendDTO>("400", exception.Message, exception.StackTrace));
            }
        }

        [Route("api/synch/users/{userID}/backendscount")]
        public HttpResponseMessage PostBackendsCount(SynchRequestDTO query, string userID)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{" +  userID +  "}/backendscount, action: starting method");
                int syncTime = 0;
                DateTime curTimeStamp = DateTime.Now;
                //check null for input query
                if (query != null)
                {
                    Synch synch = new Synch();
                    List<string> userbackends = query.parameters.filters.backends;
                    //get userbackends associated to user
                    List<UserBackendEntity> allUserBackends = synch.GetUserBackendsList(userID, userbackends);
                    //get requests associated to user
                    //List<RequestEntity> requestslist = synch.GetUserRequests(userID, query.parameters.filters.reqStatus);
                    //get approvals associated to user based on approval status
                    int completedTaskSyncDays;
                    //get completed Tasks configuration value from request
                    //InsightLogger.TrackEvent(Convert.ToString(query.parameters.filters.CompletedRequestsSync));
                    if (!string.IsNullOrEmpty(Convert.ToString(query.parameters.filters.CompletedRequestsSync)) && query.parameters.filters.CompletedRequestsSync != 0)
                    {
                        completedTaskSyncDays = query.parameters.filters.CompletedRequestsSync;
                    }
                    else
                    {
                        //if it is null get default value from configuration file
                        completedTaskSyncDays = defaultCompletedTaskSyncDays;
                    }
                    string apprStatus = query.parameters.filters.apprStatus;
                    //get all the approval tasks from service layer
                    List<ApprovalEntity> approvalslist = synch.GetAllUserApprovals(userID);
                    Boolean requestsunfulfilled = false;
                    Boolean requestsfulfilled = false;
                    bool IsForceUpdate = query.parameters.forceUpdate;
                    bool IsUrgentTask = query.parameters.filters.IsUrgent;
                    List<ApprovalEntity> pendingApprovalslist = null;
                    List<ApprovalEntity> approvedApprovalslist = null;
                    List<ApprovalEntity> rejectedApprovalslist = null;
                    int pendingTaskCount = 0;
                    int urgentTaskCount = 0;
                    int approvedTaskCount = 0;
                    int rejectedTaskCount = 0;
                    //get tasks based on status
                    if (approvalslist != null && approvalslist.Count > 0)
                    {
                        //get pending tasks
                        pendingApprovalslist = approvalslist.Where(x => x.Status == waitingTaskStatus).ToList();
                        //get Approved tasks
                        approvedApprovalslist = approvalslist.Where(x => x.Status == approvedTaskStatus).ToList();
                        //get rejected tasks                       
                        rejectedApprovalslist = approvalslist.Where(x => x.Status == rejectedTaskStatus).ToList();
                    }
                    List<UserBackendDTO> userbackendlist = new List<UserBackendDTO>();
                    List<ApprovalCountDTO> lstapprovalCounts = new List<ApprovalCountDTO>();
                    //check extended depth flag
                    if (Rules.ExtendedDepthperAllBackends(query, allUserBackends, Convert.ToInt32(ConfigurationManager.AppSettings[CoreConstants.Config.MaxSynchReplySize])))
                    {
                        InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{" + userID + "}/backendscount, action: check extended Depth, response: true");
                        InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{" + userID + "}/backendscount, action:looping through backends");
                        //loop all backends to check updated or not for synch
                        foreach (UserBackendEntity userbackend in allUserBackends)
                        {
                            pendingTaskCount = 0;
                            urgentTaskCount = 0;
                            approvedTaskCount = 0;
                            rejectedTaskCount = 0;
                            //convert userbackend entity response userbackend dto object
                            UserBackendDTO userbackenddto = DataProvider.ResponseObjectMapper<UserBackendDTO, UserBackendEntity>(userbackend);
                            Backend backenddto = DataProvider.ResponseObjectMapper<Backend, UserBackendEntity>(userbackend);
                            userbackenddto.backend = backenddto;
                            //approvals list associated to userbackend
                            List<ApprovalEntity> userbackendapprovalslist = new List<ApprovalEntity>();
                            if (approvalslist != null && approvalslist.Count > 0)
                            {
                                //get urgent tasks based on condition
                                if (pendingApprovalslist != null && pendingApprovalslist.Count > 0)
                                {
                                    if (userbackend.BackendID == carDBName)
                                    {
                                        // if the backend is car then pull the urgent tasks based on createdDate + { No. of days fro config} <= DateTime.Now                           
                                        urgentTaskCount = pendingApprovalslist.Where(x => x.BackendID == userbackend.BackendID && x.Created != null && (x.Created.Value.Date.AddDays(urgentTaskCalcDaysForCAR).Date < DateTime.Now.Date)).Count();
                                        pendingTaskCount = pendingApprovalslist.Where(x => x.BackendID == userbackend.BackendID && x.Created != null && (x.Created.Value.Date.AddDays(urgentTaskCalcDaysForCAR).Date >= DateTime.Now.Date)).Count();
                                    }
                                    else if (userbackend.BackendID == bpmDBName)
                                    {
                                        // if the backend is store then pull the urgent tasks based on Duedate + { No. of days fro config} <= DateTime.Now
                                        urgentTaskCount = pendingApprovalslist.Where(x => x.BackendID == userbackend.BackendID && x.DueDate != null && (x.DueDate.Value.Date.AddDays(urgentTaskCalcDaysForBPM).Date < DateTime.Now.Date)).Count();
                                        pendingTaskCount = pendingApprovalslist.Where(x => x.BackendID == userbackend.BackendID && x.DueDate != null && (x.DueDate.Value.Date.AddDays(urgentTaskCalcDaysForBPM).Date >= DateTime.Now.Date)).Count();
                                    }
                                }
                                if (approvedApprovalslist != null && approvedApprovalslist.Count > 0)
                                {
                                    //pull approved tasks based on config value                                   
                                    approvedTaskCount = approvedApprovalslist.Where(x => x.BackendID == userbackend.BackendID && x.DecisionDate != null && (x.DecisionDate.Value.Date >= DateTime.Now.AddDays(-(completedTaskSyncDays)).Date)).Count();
                                }
                                if (rejectedApprovalslist != null && rejectedApprovalslist.Count > 0)
                                {
                                    //pull approved tasks based on config value                                   
                                    rejectedTaskCount = rejectedApprovalslist.Where(x => x.BackendID == userbackend.BackendID && x.DecisionDate != null && (x.DecisionDate.Value.Date >= DateTime.Now.AddDays(-(completedTaskSyncDays)).Date)).Count();
                                }
                            }
                            ApprovalCountDTO approvalcount = new Models.ApprovalCountDTO();
                            approvalcount.BackendID = userbackend.BackendID;
                            approvalcount.BackendName = userbackend.BackendName;
                            approvalcount.WaitingCount = pendingTaskCount;
                            approvalcount.UrgentPendingCount = urgentTaskCount;
                            approvalcount.ApprovedCount = approvedTaskCount;
                            approvalcount.RejectedCount = rejectedTaskCount;
                            lstapprovalCounts.Add(approvalcount);
                            int requestsCount = 0;
                            if (userbackendapprovalslist != null && userbackendapprovalslist.Count > 0)
                            {
                                requestsCount = userbackendapprovalslist.Count;
                            }

                            ApprovalsCountDTO approvalcountdto = new ApprovalsCountDTO();
                            InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{" + userID + "}/backendscount, action: Adding approval count to response, response: success, backendID:" + userbackend.BackendID);
                            approvalcountdto.BackendID = userbackend.BackendID;
                            approvalcountdto.Status = query.parameters.filters.apprStatus;
                            approvalcountdto.Count = requestsCount;
                            userbackenddto.approvalsCount = approvalcountdto;
                            // List<ApprovalRequestDTO> approvalrequestlist = new List<ApprovalRequestDTO>();
                            //get requests associated to each user backend
                            //List<RequestEntity> userbackendrequestslist = requestslist.Where(x => x.BackendID == userbackend.BackendID).ToList();
                            //check if backend updated
                            if (Rules.IsBackendUpdated(userbackend, query))
                            {
                                InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{" + userID + "}/backendscount, action: check backend update, response: true");
                                //if extended depth is userbackend level
                                if (Rules.ExtendedDepthperBackend(userbackend, Convert.ToInt32(ConfigurationManager.AppSettings[CoreConstants.Config.MaxSynchReplySize])))
                                {
                                    InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{" + userID + "}/backendscount, action: check extended depth, response: true");
                                    //commented code related to adding requests and tasks to response
                                    //userbackenddto = synch.AddRequsetsTasksCountToSynchResponse(userbackendrequestslist, userbackendapprovalslist, userbackend, query, userbackenddto);

                                }
                                //check requestsfullfilled flag
                                if (!requestsfulfilled)
                                {
                                    //code to clear extended depth flags
                                }
                                //check requestsunfullfilled flag
                                if (!requestsunfulfilled)
                                {
                                    //code to update backend synch timestamp                                    ;
                                }
                                synch.AddUpdateBackendSynch(userbackend);
                            }
                            else
                            {
                                InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{" + userID + "}/backendscount, action: check backend update, response: false");
                                SynchDTO synchdto = new SynchDTO();
                                //check if backend update is in progress in service layer then send the latency in response
                                if (Rules.IsBackendUpdateInProgress(userbackend))
                                {
                                    InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{" + userID + "}/backendscount, action: check in-progress backend update, response: true");
                                    synchdto.retryAfter = userbackend.ExpectedLatency;
                                    InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{" + userID + "}/backendscount, action: add expected backend latency to response as retry time, response: success");
                                    userbackenddto.synch = synchdto;
                                }
                                else
                                {
                                    InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{" + userID + "}/backendscount, action: check in-progress backend update, response: false");
                                    //if update is not in progress, trigger it
                                    if (IsForceUpdate)
                                    {
                                        synch.TriggerUserBackendUpdate(userbackend, true);
                                        //call missing update function in async
                                        Task.Factory.StartNew(() =>
                                        {
                                            synch.CollectUsersMissedUpdatesByBackend(userbackend, curTimeStamp);
                                        });

                                    }
                                    else
                                    {
                                        synch.TriggerUserBackendUpdate(userbackend, false);
                                    }

                                    InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{" + userID + "}/backendscount, action: trigger a backend update, response: success");
                                    synchdto.retryAfter = Convert.ToInt32(Rules.BackendRetryTime(userbackend));
                                    InsightLogger.TrackEvent("SyncAPIController :: endpoint - api/synch/users/{" + userID + "}/backendscount, action: add backend retry time to response, response: success");
                                    userbackenddto.synch = synchdto;
                                }
                                //commented code related to adding requests and tasks to response
                                //userbackenddto = synch.AddRequsetsTasksCountToSynchResponse(userbackendrequestslist, userbackendapprovalslist, userbackend, query, userbackenddto);

                            }
                            //add each userbackend to list
                            userbackendlist.Add(userbackenddto);
                        }

                    }
                    // if it is force update then need to calculate sync time
                    if (IsForceUpdate)
                    {
                        //calculate sync time
                        syncTime = synch.CalcSynchTime(allUserBackends);
                        //add sync time value to SynchTimeResponseDTO object
                        SynchTimeResponseDTO response = new SynchTimeResponseDTO();
                        response.SyncTime = syncTime;                      

                        InsightLogger.TrackEvent("SyncAPIController ::" + callerMethodName + " method execution has been Completed.");
                        return Request.CreateResponse(HttpStatusCode.OK, response);
                    }
                    else
                    {
                        //SynchResponseDTO<UserBackendDTO> response = new SynchResponseDTO<UserBackendDTO>();
                        //response.result = userbackendlist;
                        //response.SyncTime = syncTime;
                        SyncResponseResultDTO<ApprovalCountDTO> response = new SyncResponseResultDTO<ApprovalCountDTO>();
                        response.result = lstapprovalCounts;
                        response.SyncTime = syncTime;
                        InsightLogger.TrackEvent("SyncAPIController ::" + callerMethodName + " method execution has been Completed.");
                        return Request.CreateResponse(HttpStatusCode.OK, response);
                    }


                }
                else
                {
                    InsightLogger.TrackEvent("SyncAPIController ::" + callerMethodName + " method execution has been Completed.");
                    return Request.CreateResponse(HttpStatusCode.BadRequest, DataProvider.SynchResponseError<UserBackendDTO>("400", "input query is not sent to get user associated backends", ""));
                }
            }
            catch (DataAccessException dalexception)
            {
                InsightLogger.Exception(dalexception.Message, dalexception, callerMethodName);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.SynchResponseError<UserBackendDTO>("400", dalexception.Message, dalexception.Message));
            }
            catch (BusinessLogicException blexception)
            {
                InsightLogger.Exception(blexception.Message, blexception, callerMethodName);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.SynchResponseError<UserBackendDTO>("400", blexception.Message, blexception.Message));
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                //LoggerHelper.WriteToLog(exception + " - exception in controller action while retrieving associated user backends : "
                //      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.SynchResponseError<UserBackendDTO>("400", exception.Message, exception.StackTrace));
            }
        }

        //public Expression<Func<ApprovalEntity, bool>> GetWhereLamda(string backendID,string dynamicColumn,DateTime? dateVal)
        //{
        //    ParameterExpression parExp = Expression.Parameter(typeof(ApprovalEntity), "app");
        //    Expression backendExpr = GetEqualsExpr(parExp, "BackendID", backendID);
        //    Expression DateExpr = GetLessthanEqualsExpr(parExp, dynamicColumn, dateVal);
        //    Expression cond = Expression.And(backendExpr, DateExpr);
        //    return Expression.Lambda<Func<ApprovalEntity, bool>>(cond, parExp);
        //}
        //private Expression GetEqualsExpr(ParameterExpression param,
        //                         string property,
        //                         string value)
        //{
        //    Expression prop = Expression.Property(param, property);
        //    Expression val = Expression.Constant(value);
        //    return Expression.Equal(prop, val);
        //}
        //private Expression GetLessthanEqualsExpr(ParameterExpression param,
        //                         string property,
        //                         DateTime? value)
        //{
        //    Expression column = Expression.PropertyOrField(param, property);
        //   // Expression prop = Expression.Property(param, property);
        //   // Expression val = Expression.Constant(value,typeof(DateTime ?));
        //    return Expression.LessThanOrEqual(column, Expression.Constant(value, column.Type));
        //}
    }
}
