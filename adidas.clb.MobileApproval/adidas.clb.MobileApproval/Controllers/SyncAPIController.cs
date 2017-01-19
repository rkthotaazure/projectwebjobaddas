//-----------------------------------------------------------
// <copyright file="PersonalizationAPIController.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
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
    //[Authorize]   
    public class SyncAPIController : ApiController
    {
        //Application insights interface reference for logging the error details into Application Insight azure service.
        static IAppInsight InsightLogger { get { return AppInsightLogger.Instance; } }
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
                InsightLogger.TrackEvent("adidas.clb.MobileApproval:: SyncAPIController ::" + callerMethodName + " method execution has been started.");
                //check null for input query
                if (query != null)
                {
                    Synch synch = new Synch();
                    List<string> userbackends = query.parameters.filters.backends;
                    //get userbackends associated to user
                    List<UserBackendEntity> allUserBackends = synch.GetUserBackendsList(userID, userbackends);
                    //get requests associated to user
                    List<RequestEntity> requestslist = synch.GetUserRequests(userID, query.parameters.filters.reqStatus);
                    Boolean requestsunfulfilled = false;
                    Boolean requestsfulfilled = false;
                    List<UserBackendDTO> userbackendlist = new List<UserBackendDTO>();
                    //check extended depth flag
                    if (Rules.ExtendedDepthperAllBackends(query, allUserBackends, query.client.device.maxSynchReplySize))
                    {
                        //loop all backends to check updated or not for synch
                        foreach (UserBackendEntity userbackend in allUserBackends)
                        {
                            //convert userbackend entity response userbackend dto object
                            UserBackendDTO userbackenddto = DataProvider.ResponseObjectMapper<UserBackendDTO, UserBackendEntity>(userbackend);
                            Backend backenddto = DataProvider.ResponseObjectMapper<Backend, UserBackendEntity>(userbackend);
                            userbackenddto.backend = backenddto;
                            List<ApprovalRequestDTO> approvalrequestlist = new List<ApprovalRequestDTO>();
                            //check if backend updated
                            if (Rules.IsBackendUpdated(userbackend))
                            {
                                //if extended depth is userbackend level
                                if (Rules.ExtendedDepthperBackend(userbackend, query.client.device.maxSynchReplySize))
                                {
                                    List<RequestEntity> userbackendrequestslist = requestslist.Where(x => x.BackendID == userbackend.BackendID).ToList();
                                    //loop through each request in the userbackend
                                    foreach (RequestEntity request in userbackendrequestslist)
                                    {
                                        ApprovalRequestDTO approvalrequest = new ApprovalRequestDTO();
                                        RequestDTO requestdto = new RequestDTO();
                                        //if request is updated
                                        if (Rules.IsRequestUpdated(request, userbackend.DefaultUpdateFrequency))
                                        {
                                            //check if requests which have changed since the last synch need to send in response or all requests.
                                            if (Rules.IsTargetRequest(query, request))
                                            {
                                                requestdto = DataProvider.ResponseObjectMapper<RequestDTO, RequestEntity>(request);
                                                approvalrequest.request = requestdto;
                                                //code here to populate extended depth
                                                //code here to update request synch time stamp
                                                synch.AddUpdateRequestSynch(request, query.userId);
                                                requestsfulfilled = true;
                                            }
                                        }
                                        else
                                        {
                                            //check if request update is in progress in service layer then send the latency in response
                                            if (Rules.IsRequestUpdateInProgress(request))
                                            {
                                                approvalrequest.retryAfter = request.ExpectedLatency;
                                            }
                                            else
                                            {
                                                synch.TriggerRequestUpdate(request, query.userId);
                                                approvalrequest.retryAfter = Convert.ToInt32(Rules.RequestRetryTime(userbackend));
                                            }
                                            requestsunfulfilled = true;
                                        }
                                        //add approval request to list which will be added to corresponding backend
                                        approvalrequestlist.Add(approvalrequest);
                                    }
                                    userbackenddto.requests = approvalrequestlist;
                                }
                                //check requestsfullfilled flag
                                if (!requestsfulfilled)
                                {
                                    //code to clear extended depth flags
                                }
                                //check requestsunfullfilled flag
                                if (!requestsunfulfilled)
                                {
                                    //code to update backend synch timestamp
                                    synch.AddUpdateBackendSynch(userbackend);
                                }
                            }
                            else
                            {
                                SynchDTO synchdto = new SynchDTO();
                                //check if backend update is in progress in service layer then send the latency in response
                                if (Rules.IsBackendUpdateInProgress(userbackend))
                                {
                                    synchdto.retryAfter = userbackend.ExpectedLatency;
                                    userbackenddto.synch = synchdto;
                                }
                                else
                                {
                                    //if update is not in progress, trigger it
                                    synch.TriggerUserBackendUpdate(userbackend);
                                    synchdto.retryAfter = Convert.ToInt32(Rules.BackendRetryTime(userbackend));
                                    userbackenddto.synch = synchdto;
                                }
                            }
                            //add each userbackend to list
                            userbackendlist.Add(userbackenddto);
                        }
                    }
                    SynchResponseDTO<UserBackendDTO> response = new SynchResponseDTO<UserBackendDTO>();
                    response.result = userbackendlist;
                    InsightLogger.TrackEvent("adidas.clb.MobileApproval:: SyncAPIController ::" + callerMethodName + " method execution has been Completed.");
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                else
                {
                    InsightLogger.TrackEvent("adidas.clb.MobileApproval:: SyncAPIController ::" + callerMethodName + " method execution has been Completed.");
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
                InsightLogger.TrackEvent("adidas.clb.MobileApproval:: SyncAPIController ::" + callerMethodName + " method execution has been started.");
                //check null for input query
                if (query != null)
                {
                    Synch synch = new Synch();
                    //get userbackend associated to user with backendid
                    UserBackendEntity userbackend = synch.GetUserBackend(userID, usrBackendID);
                    //get requests associated to userbackend
                    List<RequestEntity> requestslist = synch.GetUserBackendRequests(userID, usrBackendID, query.parameters.filters.reqStatus);
                    //use mmapper to convert userbackend entity to userbackend data transfer object
                    UserBackendDTO userbackenddto = DataProvider.ResponseObjectMapper<UserBackendDTO, UserBackendEntity>(userbackend);
                    Backend backenddto = DataProvider.ResponseObjectMapper<Backend, UserBackendEntity>(userbackend);
                    userbackenddto.backend = backenddto;
                    List<ApprovalRequestDTO> approvalrequestlist = new List<ApprovalRequestDTO>();
                    Boolean requestsunfulfilled = false;
                    Boolean requestsfulfilled = false;
                    //check if backend updated
                    if (Rules.IsBackendUpdated(userbackend))
                    {
                        //if extended depth is userbackend level
                        if (Rules.ExtendedDepthperBackend(userbackend, query.client.device.maxSynchReplySize))
                        {
                            //loop through each request in the userbackend
                            foreach (RequestEntity request in requestslist)
                            {
                                ApprovalRequestDTO approvalrequest = new ApprovalRequestDTO();
                                RequestDTO requestdto = new RequestDTO();
                                //if request is updated
                                if (Rules.IsRequestUpdated(request, userbackend.DefaultUpdateFrequency))
                                {
                                    //check if requests which have changed since the last synch need to send in response or all requests.
                                    if (Rules.IsTargetRequest(query, request))
                                    {
                                        requestdto = DataProvider.ResponseObjectMapper<RequestDTO, RequestEntity>(request);
                                        approvalrequest.request = requestdto;
                                        //code to populate extended depth
                                        //code to update request synch timestamp
                                        synch.AddUpdateRequestSynch(request, query.userId);
                                        requestsfulfilled = true;
                                    }
                                }
                                else
                                {
                                    //check if request update is in progress in service layer then send the latency in response
                                    if (Rules.IsRequestUpdateInProgress(request))
                                    {
                                        approvalrequest.retryAfter = request.ExpectedLatency;
                                    }
                                    else
                                    {
                                        synch.TriggerRequestUpdate(request, query.userId);
                                        approvalrequest.retryAfter = Convert.ToInt32(Rules.RequestRetryTime(userbackend));
                                    }
                                    requestsunfulfilled = true;
                                }
                                //add approval request to list which will be added to corresponding backend
                                approvalrequestlist.Add(approvalrequest);
                            }
                        }
                        //check requestsfullfilled flag
                        if (!requestsfulfilled)
                        {
                            //code to clear extended depth flags
                        }
                        //check requestsunfullfilled flag
                        if (!requestsunfulfilled)
                        {
                            //code to update backend synch timestamp
                            synch.AddUpdateBackendSynch(userbackend);
                        }
                    }
                    else
                    {
                        SynchDTO synchdto = new SynchDTO();
                        //check if backend update is in progress in service layer then send the latency in response
                        if (Rules.IsBackendUpdateInProgress(userbackend))
                        {
                            synchdto.retryAfter = userbackend.ExpectedLatency;
                            userbackenddto.synch = synchdto;
                        }
                        else
                        {
                            //if update is not in progress, trigger it
                            synch.TriggerUserBackendUpdate(userbackend);
                            synchdto.retryAfter = Convert.ToInt32(Rules.BackendRetryTime(userbackend));
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
                    InsightLogger.TrackEvent("adidas.clb.MobileApproval:: SyncAPIController ::" + callerMethodName + " method execution has been Completed.");
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                else
                {
                    InsightLogger.TrackEvent("adidas.clb.MobileApproval:: SyncAPIController ::" + callerMethodName + " method execution has been Completed.");
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
                InsightLogger.TrackEvent("adidas.clb.MobileApproval:: SyncAPIController ::" + callerMethodName + " method execution has been started.");
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
                        //check if requests which have changed since the last synch need to send in response or all requests.
                        if (Rules.IsRequestATarget(query, requestentity))
                        {
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
                            if (Rules.ExtendedDepth(userbackend, query.client.device.maxSynchReplySize))
                            {
                                //code to populate extended depth
                            }
                            else
                            {
                                //code to clear extended depth flags
                            }
                            //code update request synch timestamp
                            synch.AddUpdateRequestSynch(requestentity, query.userId);
                        }
                    }
                    else
                    {
                        //check if backend update is in progress in service layer then send the latency in response
                        if (Rules.IsRequestUpdateInProgress(requestentity))
                        {
                            approvalrequest.retryAfter = requestentity.ExpectedLatency;
                        }
                        else
                        {
                            //if update is not in progress, trigger it
                            synch.TriggerRequestUpdate(requestentity, query.userId);
                            approvalrequest.retryAfter = Convert.ToInt32(Rules.RequestRetryTime(userbackend));
                        }
                        //code to clear extended depth flags
                    }
                    //add request to list to send it in response
                    List<ApprovalRequestDTO> approvalrequestlist = new List<ApprovalRequestDTO>();
                    approvalrequestlist.Add(approvalrequest);
                    SynchResponseDTO<ApprovalRequestDTO> response = new SynchResponseDTO<ApprovalRequestDTO>();
                    response.result = approvalrequestlist;
                    InsightLogger.TrackEvent("adidas.clb.MobileApproval:: SyncAPIController ::" + callerMethodName + " method execution has been Completed.");
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                else
                {
                    InsightLogger.TrackEvent("adidas.clb.MobileApproval:: SyncAPIController ::" + callerMethodName + " method execution has been Completed.");
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
                InsightLogger.TrackEvent("adidas.clb.MobileApproval:: SyncAPIController ::" + callerMethodName + " method execution has been started.");
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
                        //check if requests which have changed since the last synch need to send in response or all requests.
                        if (Rules.IsRequestATarget(query, requestentity))
                        {
                            SynchResponse.query = query;
                            SynchResponse.result = approvers;
                            //code update request synch timestamp
                            synch.AddUpdateRequestSynch(requestentity, query.userId);
                        }
                    }
                    else
                    {
                        //check if backend update is in progress in service layer then send the latency in response
                        if (Rules.IsRequestUpdateInProgress(requestentity))
                        {
                            retrytime = requestentity.ExpectedLatency;
                        }
                        else
                        {
                            //if update is not in progress, trigger it
                            //commented updatetriggering to remove duplicate triggering as get requestdeatils, get approvers end points calling at same time from client.
                            //synch.TriggerRequestUpdate(requestentity, query.userId);
                            retrytime = Rules.RequestRetryTime(userbackend);
                        }
                    }
                    InsightLogger.TrackEvent("adidas.clb.MobileApproval:: SyncAPIController ::" + callerMethodName + " method execution has been Completed.");
                    return Request.CreateResponse(HttpStatusCode.OK, SynchResponse);
                }
                else
                {
                    InsightLogger.TrackEvent("adidas.clb.MobileApproval:: SyncAPIController ::" + callerMethodName + " method execution has been Completed.");
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
                InsightLogger.TrackEvent("adidas.clb.MobileApproval:: SyncAPIController ::" + callerMethodName + " method execution has been started.");
                Synch synch = new Synch();
                //get requests with requestid
                RequestEntity requestentity = synch.GetRequest(query.userId, apprReqID);
                InsightLogger.TrackEvent("adidas.clb.MobileApproval:: SyncAPIController ::" + callerMethodName + " method execution has been Completed.");
                return Request.CreateResponse(HttpStatusCode.OK, requestentity.PDFUri);
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
                InsightLogger.TrackEvent("adidas.clb.MobileApproval:: SyncAPIController ::" + callerMethodName + " method execution has been started.");
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
                    if (Rules.IsBackendUpdated(userbackend))
                    {
                        //if extended depth is userbackend level
                        if (Rules.ExtendedDepthperBackend(userbackend, query.client.device.maxSynchReplySize))
                        {
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
                        }
                    }
                    else
                    {
                        SynchDTO synchdto = new SynchDTO();
                        //check if backend update is in progress in service layer then send the latency in response
                        if (Rules.IsBackendUpdateInProgress(userbackend))
                        {
                            synchdto.retryAfter = userbackend.ExpectedLatency;
                            userbackenddto.synch = synchdto;
                        }
                        else
                        {
                            //if update is not in progress, trigger it
                            synch.TriggerUserBackendUpdate(userbackend);
                            synchdto.retryAfter = Convert.ToInt32(Rules.BackendRetryTime(userbackend));
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
                    InsightLogger.TrackEvent("adidas.clb.MobileApproval:: SyncAPIController ::" + callerMethodName + " method execution has been Completed.");
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                else
                {
                    InsightLogger.TrackEvent("adidas.clb.MobileApproval:: SyncAPIController ::" + callerMethodName + " method execution has been Completed.");
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
                InsightLogger.TrackEvent("adidas.clb.MobileApproval:: SyncAPIController ::" + callerMethodName + " method execution has been started.");
                //check null for input query
                if (query != null)
                {
                    Synch synch = new Synch();
                    List<string> userbackends = query.parameters.filters.backends;
                    //get approvals associated to user absed on approval status
                    List<ApprovalEntity> approvalslist = synch.GetUserApprovalsForCount(userID, query.parameters.filters.apprStatus);
                    List<ApprovalsCountDTO> approvalcountlist = new List<ApprovalsCountDTO>();
                    //loop all backends to ad count to response
                    foreach (string userbackend in userbackends)
                    {
                        List<ApprovalEntity> userbackendapprovalslist = approvalslist.Where(x => x.BackendID == userbackend).ToList();
                        ApprovalsCountDTO approvalcountdto = new ApprovalsCountDTO();
                        approvalcountdto.BackendID = userbackend;
                        approvalcountdto.Status = query.parameters.filters.apprStatus;
                        approvalcountdto.Count = userbackendapprovalslist.Count;
                        approvalcountlist.Add(approvalcountdto);
                    }

                    SynchResponseDTO<ApprovalsCountDTO> response = new SynchResponseDTO<ApprovalsCountDTO>();
                    response.result = approvalcountlist;
                    InsightLogger.TrackEvent("adidas.clb.MobileApproval:: SyncAPIController ::" + callerMethodName + " method execution has been Completed.");
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                else
                {
                    InsightLogger.TrackEvent("adidas.clb.MobileApproval:: SyncAPIController ::" + callerMethodName + " method execution has been Completed.");
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
    }
}
