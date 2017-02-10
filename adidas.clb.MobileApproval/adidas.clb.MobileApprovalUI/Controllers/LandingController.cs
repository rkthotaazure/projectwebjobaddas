//-----------------------------------------------------------
// <copyright file="LandingController.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using adidas.clb.MobileApprovalUI.Models;
using Microsoft.WindowsAzure.Storage.Table;
using adidas.clb.MobileApprovalUI.Exceptions;
using Microsoft.WindowsAzure.Storage;
using Microsoft.Azure;
using adidas.clb.MobileApprovalUI.Utility;
using System.Threading.Tasks;
using adidas.clb.MobileApprovalUI.Models.JSONHelper;
using Newtonsoft.Json;

namespace adidas.clb.MobileApprovalUI.Controllers
{
    /// <summary>
    /// The controller that will handle requests for the Landing page.
    /// </summary>

    //[Authorize]
    public class LandingController : Controller
    {
        // Initialize the userid
        string userid = SettingsHelper.UserId;
        // Return Approval Landing page
        [HttpGet]
        public ActionResult ApprovalLanding()
        {
            return View();
        }
        // Return Pending Approval Details page
        public ActionResult ApprovalDetails()
        {
            return View();
        }
        // Return Pending Approval Details page
        public ActionResult DetailTaskInfo()
        {
            return View();
        }

        // Send Approval status to service object
        [HttpPost]
        public async Task<ActionResult> SendApprovalstatus(ApprovalQuery approvalInfo)
        {
            try
            {
                approvalInfo.UserID = userid;
                //Api Controller object initialization
                APIController apiControllerObj = new APIController();
                //Send approval details to Approval API
                string stApprovalrequeststatus = await apiControllerObj.SendApprovalInfo(approvalInfo, approvalInfo.ApprovalRequestID);
                //creates list request details object
                List<ApprovalRequestDTO> requestsDetails = new List<ApprovalRequestDTO>();
                // Return Json Formate object and pass to UI
                return Json(requestsDetails, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error while creating client context : "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                return View("Error");
            }
        }
        // Get Request details from service object
        [HttpPost]
        public async Task<ActionResult> GetRequestDetails(RequestDetails requestInfo)
        {
            try
            {
                //Assign UI synch request details to SynchRequestDTO model
                SynchRequestDTO syncRequest = requestInfo.syncRequest;
                //Assign user id to SynchRequestDTO model
                syncRequest.userId = userid;
                //Api Controller object initialization
                APIController apiControllerObj = new APIController();
                //Get request details from Synch API
                string stApprovalrequest = await apiControllerObj.GetRequestInfo(syncRequest, requestInfo.requestID);
                //Get Approval List details from Synch API
                string strApproverList = await apiControllerObj.GetApprovers(syncRequest, requestInfo.requestID);                
                //Deseralize the result returned by the API
                UserRequestJsonData userBackendjsonResponse = JsonConvert.DeserializeObject<UserRequestJsonData>(stApprovalrequest);
                UserApprovalJsonData userApprovaljsonResponse = JsonConvert.DeserializeObject<UserApprovalJsonData>(strApproverList);
                //creates list request details object
                List<ApprovalRequestDTO> requestsDetails = new List<ApprovalRequestDTO>();
                //Checks whether the JSON response is not null
                if (userBackendjsonResponse != null && userApprovaljsonResponse != null)
                {
                    //Create ApprovalRequestDTO Model object
                    ApprovalRequestDTO requestObj = new ApprovalRequestDTO();
                    //Iterate json format result and bind to Model
                    foreach (userBackendRequest userbackendRequestdetails in userBackendjsonResponse.userBackendRequestinfo)
                    {
                        //Create ApprovalDTO Model object
                        requestObj.approval = new ApprovalDTO();
                        //requestObj.approval.RequestId = userbackendRequestdetails.approvalDetails.RequestId;
                        //requestObj.approval.status = userbackendRequestdetails.approvalDetails.status;
                        //Create RequestDTO Model object
                        requestObj.request = new RequestDTO();
                        if (userbackendRequestdetails.requestDetails != null)
                        {
                            //Get Request ID from Json reuslt
                            requestObj.request.ID = userbackendRequestdetails.requestDetails.ID;
                            //Get Request Title from Json reuslt
                            requestObj.request.Title = userbackendRequestdetails.requestDetails.Title;
                            //Get Request Status from Json reuslt
                            requestObj.request.Status = userbackendRequestdetails.requestDetails.Status;
                            //Get Request Created from Json reuslt
                            DateTime? created = userbackendRequestdetails.requestDetails.Created;
                            if (created != null)
                            {
                                requestObj.request.Created = created.Value;
                            }
                            //Get Request Requester details from Json reuslt
                            requestObj.request.Requester = new RequesterDTO();
                            //Get Requester UserID from Json reuslt
                            if (requestObj.request.Requester.UserID == null)
                            {
                                requestObj.request.Requester.UserID = userbackendRequestdetails.requestDetails.Requester.UserID;
                            }
                            //Get Requester UserID from Json reuslt
                            if (requestObj.request.Requester.Name == null)
                            {
                                requestObj.request.Requester.Name = userbackendRequestdetails.requestDetails.Requester.Name;
                            }
                            //Creates list request fields object
                            List<FieldDTO> requestFields = new List<FieldDTO>();
                            //Checks whether the JSON response is not null
                            if (userbackendRequestdetails.requestDetails.Fields.Overview != null && userbackendRequestdetails.requestDetails.Fields.Overview.Count > 0)
                            {
                                //Iterate json format result and bind to Model
                                foreach (FieldDTO field in userbackendRequestdetails.requestDetails.Fields.Overview)
                                {
                                    //Create FieldDTO Model object for Overview fields
                                    FieldDTO overviewFields = new FieldDTO();
                                    //Get Overview fields Name from Json reuslt
                                    overviewFields.Name = field.Name;
                                    //Get Overview fields Value from Json reuslt
                                    overviewFields.Value = field.Value;
                                    //Get Overview fields Group from Json reuslt
                                    overviewFields.Group = field.Group;
                                    //Add to FieldDTO Model object
                                    requestFields.Add(overviewFields);
                                }
                            }
                            //Checks whether the JSON response is not null
                            if (userbackendRequestdetails.requestDetails.Fields.GenericInfo != null && userbackendRequestdetails.requestDetails.Fields.Overview.Count > 0)
                            {
                                //Iterate json format result and bind to Model
                                foreach (FieldDTO field in userbackendRequestdetails.requestDetails.Fields.GenericInfo)
                                {
                                    //Create FieldDTO Model object for Generic fields
                                    FieldDTO genericInfoFields = new FieldDTO();
                                    //Get Generic fields Name/Value pair from Json reuslt
                                    genericInfoFields.Name = field.Name;
                                    genericInfoFields.Value = field.Value;
                                    //Get Generic fields Group from Json reuslt
                                    genericInfoFields.Group = field.Group;
                                    //Add to FieldDTO Model object
                                    requestFields.Add(genericInfoFields);
                                }
                            }
                            //Creates list approval list object
                            List<Approvers> approverList = new List<Approvers>();
                            //Iterate json format result and bind to Model
                            foreach (ApproversJson userApprovalJsondetails in userApprovaljsonResponse.userApprovalinfo)
                            {
                                //Create Approvers Model object for Approval details
                                Approvers userApprovaldetails = new Approvers();
                                //Get Approval Order info
                                userApprovaldetails.Order = userApprovalJsondetails.Order;
                                //Get Approval Order UserID
                                userApprovaldetails.UserID = userApprovalJsondetails.UserID;
                                //Get Approval Order UserName
                                userApprovaldetails.UserName = userApprovalJsondetails.UserName;
                                //Get Approval Order Status
                                userApprovaldetails.Status = userApprovalJsondetails.Status;
                                //Get Approval Order Created
                                userApprovaldetails.Created = userApprovalJsondetails.Created;
                                //Get Approval Order DueDate
                                userApprovaldetails.DueDate = userApprovalJsondetails.DueDate;
                                //Get Approval Order DecisionDate
                                userApprovaldetails.DecisionDate = userApprovalJsondetails.DecisionDate;
                                //Add to Approvers Model object for Approval details
                                approverList.Add(userApprovaldetails);
                            }
                            //Add approval list to ApprovalRequestDTO Model object
                            requestObj.request.Approvers = approverList;
                            //Add Overview /Generic fields to ApprovalRequestDTO Model object
                            requestObj.request.Fields = requestFields;                            
                        }
                        else
                        {
                            requestObj.request = null;
                            requestObj.approval = null;
                            requestObj.retryAfter = userbackendRequestdetails.retryAfter;
                        }
                        //Add all info to ApprovalRequestDTO Model object List
                        requestsDetails.Add(requestObj);
                    }
                }
                // Return Json formate object and pass to UI
                return Json(requestsDetails, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error while creating client context : "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                return View("Error");
            }
        }
        // Get Pending approvals details from service object
        [HttpPost]
        public async Task<ActionResult> GetApprovalDetails(SynchRequestDTO syncRequest)
        {
            string userbackend = string.Empty;
            try
            {
                //Assign UI synch request backends to list
                List<string> backendId = syncRequest.parameters.filters.backends;
                //Iterate backends and assign the each backend to userbackend string
                foreach (string backend in backendId)
                {
                    userbackend = backend;
                }
                //SynchRequestDTO syncRequest = new SynchRequestDTO();
                syncRequest.userId = userid;
                //Api Controller object initialization
                APIController apiControllerObj = new APIController();
                //creates list request details object for waiting
                List<ApprovalRequestDTO> requestsDetails = new List<ApprovalRequestDTO>();
                // Get the waiting approval status details
                if (syncRequest.parameters.filters.apprStatus == "Waiting")
                {
                    //Get request details from Synch API
                    string stApprovalPendingDetails = await apiControllerObj.GetUserBackendTasks(syncRequest, userid, userbackend);
                    //Deseralize the result returned by the API
                    UserBackendrequestJsonData userBackendjsonResponse = JsonConvert.DeserializeObject<UserBackendrequestJsonData>(stApprovalPendingDetails);
                    //Bind the Json result data to list 
                    requestsDetails = ApprovalTasks(userBackendjsonResponse);
                }
                else
                {
                    //creates list request details object for approval and reject
                    List<ApprovalRequestDTO> requestsRejectedDetails = new List<ApprovalRequestDTO>();
                    // Get the Approved approval status details
                    syncRequest.parameters.filters.apprStatus = "Approved";
                    //Get request details from Synch API
                    string stApprovalApprovedDetails = await apiControllerObj.GetUserBackendTasks(syncRequest, userid, userbackend);
                    //Deseralize the result returned by the API
                    UserBackendrequestJsonData userBackendjsonResponse = JsonConvert.DeserializeObject<UserBackendrequestJsonData>(stApprovalApprovedDetails);
                    //Bind the Json result data to list 
                    requestsDetails = ApprovalTasks(userBackendjsonResponse);
                    // Get the Rejected approval status details
                    syncRequest.parameters.filters.apprStatus = "Rejected";
                    //Get request details from Synch API
                    string stApprovalRejectedDetails = await apiControllerObj.GetUserBackendTasks(syncRequest, userid, userbackend);
                    //Deseralize the result returned by the API
                    UserBackendrequestJsonData userBackendjsonRejectResponse = JsonConvert.DeserializeObject<UserBackendrequestJsonData>(stApprovalRejectedDetails);
                    //Bind the Json result data to list 
                    requestsRejectedDetails = ApprovalTasks(userBackendjsonRejectResponse);
                    //Add approval and rejected details tasks
                    requestsDetails.AddRange(requestsRejectedDetails);
                }
                // Return Json formate object and pass to UI
                return Json(requestsDetails, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error while creating client context : "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                return View("Error");
            }
        }

        // Get Pending approvals details from service object
        [HttpPost]
        public async Task<ActionResult> GetRequestPDF(RequestDetails requestInfo)
        {
            try
            { 
            //Assign UI synch request details to SynchRequestDTO model
            SynchRequestDTO syncRequest = requestInfo.syncRequest;
            //Assign user id to SynchRequestDTO model
            syncRequest.userId = userid;
            //Api Controller object initialization
            APIController apiControllerObj = new APIController();            
            //Get pdf uri details from Synch API
            string strpdfuri = await apiControllerObj.GetPDFUri(syncRequest, requestInfo.requestID);
                if(!string.IsNullOrEmpty(strpdfuri))
                {
                    string pdfuri = JsonConvert.DeserializeObject<string>(strpdfuri);
                    //var fileContent = new System.Net.WebClient().DownloadData(new Uri(pdfuri)); //byte[]
                    //FileContentResult filecontent = File(fileContent, "application/pdf", requestInfo.requestID+".pdf");
                    // Return Json formate object and pass to UI
                    return Json(new Uri(pdfuri), JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(string.Empty, JsonRequestBehavior.AllowGet);
                }
            
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error while creating client context : "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                return View("Error");
            }
        }
        // Json Convert Pending approvals task as list
        public List<ApprovalRequestDTO> ApprovalTasks(UserBackendrequestJsonData userBackendjsonResponse)
        {
            //creates list request details object
            List<ApprovalRequestDTO> requestsDetails = new List<ApprovalRequestDTO>();
            //Checks whether the JSON response is not null
            if (userBackendjsonResponse != null)
            {
                //Iterate json format result and bind to Model
                foreach (userBackendRequest userbackendRequestdetails in userBackendjsonResponse.userBackendRequestResults[0].userBackendRequestinfo)
                {
                    //Create ApprovalRequestDTO Model object
                    ApprovalRequestDTO requestObj = new ApprovalRequestDTO();
                    requestObj.approval = new ApprovalDTO();
                    //Get the approval task Request ID
                    requestObj.approval.RequestId = userbackendRequestdetails.approvalDetails.RequestId;
                    //Get the approval task Request Status
                    requestObj.approval.Status = userbackendRequestdetails.approvalDetails.Status;
                    //requestObj.request = new RequestDTO();
                    //requestObj.request.ID = userbackendRequestdetails.requestDetails.ID;
                    //requestObj.request.Status= userbackendRequestdetails.requestDetails.Status;
                    requestsDetails.Add(requestObj);
                }
            }
            //Return request details list
            return requestsDetails;
        }
        // Get Backend count details from service object
        [HttpPost]
        public async Task<ActionResult> GetBackendApprovalrequestcount(SynchRequestDTO syncRequest)
        {
            try
            {
                //SynchRequestDTO syncRequest = new SynchRequestDTO();
                //Assign user id to SynchRequestDTO model
                syncRequest.userId = userid;
                //Api Controller object initialization
                APIController apiControllerObj = new APIController();
                //string stApprovalrequestcount = await apiControllerObj.GetApprovalrequestcount(syncRequest, userid);
                //Get Pending approval count from Synch API
                string stApprovalPendingCount = await apiControllerObj.GetApprovalcompletedcount(syncRequest, userid);
                //Get approved count details from API
                syncRequest.parameters.filters.apprStatus = "Approved";
                string stApprovalApprovedCount = await apiControllerObj.GetApprovalcompletedcount(syncRequest, userid);
                // Get Rejected count details from API
                syncRequest.parameters.filters.apprStatus = "Rejected";
                string stApprovalRejectedCount = await apiControllerObj.GetApprovalcompletedcount(syncRequest, userid);
                //Deseralize the result returned by the API
                UserTaskcountJsonData userTaskPendingResponse = JsonConvert.DeserializeObject<UserTaskcountJsonData>(stApprovalPendingCount);
                UserTaskcountJsonData userTaskApprovedResponse = JsonConvert.DeserializeObject<UserTaskcountJsonData>(stApprovalApprovedCount);
                UserTaskcountJsonData userTaskRejectedResponse = JsonConvert.DeserializeObject<UserTaskcountJsonData>(stApprovalRejectedCount);
                //creates list Backend model object
                List<string> userBackends = new List<string>();
                userBackends = syncRequest.parameters.filters.backends;
                List<string> userBackendName = syncRequest.parameters.filters.backendName;
                //creates lists for Pending/Approval/Reject Count for json object result
                List<UserTaskcountJsonResult> userTaskPendingCount = userTaskPendingResponse.userTaskcountJsonResult;
                List<UserTaskcountJsonResult> userTaskApprovedCount = userTaskApprovedResponse.userTaskcountJsonResult;
                List<UserTaskcountJsonResult> userTaskRejectedCount = userTaskRejectedResponse.userTaskcountJsonResult;
                List<ApprovalCountDTO> approvalCountobj = new List<ApprovalCountDTO>();
                //Checks whether the JSON response is not null
                if (userTaskPendingResponse != null&& userBackends != null)
                {
                    int i = 0;
                    //Iterate json format result and bind to Model
                    foreach (string backendID in userBackends)
                    {
                        //Create ApprovalCountDTO Model object
                        ApprovalCountDTO approvalCount = new ApprovalCountDTO();
                        //Get approval backend Id
                        approvalCount.BackendID = backendID;
                        //Get approval backend Name
                        approvalCount.BackendName = userBackendName[i];
                        //Get Pending approval count
                        if (userTaskPendingCount != null && userTaskPendingCount.Count > 0)
                        {
                            List<UserTaskcountJsonResult> taskcountlist = userTaskPendingCount.Where(x => x.BackendID == backendID).ToList();
                            if (taskcountlist.Count > 0)
                            {
                                approvalCount.WaitingCount = taskcountlist.First().Count;
                            }                           
                        }
                        //Get Approved approval count
                        if (userTaskApprovedCount != null && userTaskApprovedCount.Count > 0)
                        {                            
                            List<UserTaskcountJsonResult> taskcountlist = userTaskApprovedCount.Where(x => x.BackendID == backendID).ToList();
                            if (taskcountlist.Count > 0)
                            {
                                approvalCount.ApprovedCount = taskcountlist.First().Count;
                            }
                        }
                        //Get Rejected approval count
                        if (userTaskRejectedCount != null && userTaskRejectedCount.Count > 0)
                        {                            
                            List<UserTaskcountJsonResult> taskcountlist = userTaskRejectedCount.Where(x => x.BackendID == backendID).ToList();
                            if (taskcountlist.Count > 0)
                            {
                                approvalCount.RejectedCount = taskcountlist.First().Count;
                            }
                        }
                        //Add ApprovalCountDTO Model object
                        approvalCountobj.Add(approvalCount);
                        i++;
                    }
                }
                // Return Json formate object and pass to UI
                return Json(approvalCountobj, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error while creating client context : "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                return View("Error");
            }
        }
        // Get Backend count details from service object
        [HttpPost]
        public async Task<ActionResult> ForceUpdate(SynchRequestDTO syncRequest)
        {
            try
            {
                //SynchRequestDTO syncRequest = new SynchRequestDTO();
                //Assign user id to SynchRequestDTO model
                syncRequest.userId = userid;
                //Api Controller object initialization
                APIController apiControllerObj = new APIController();
                //string stApprovalrequestcount = await apiControllerObj.GetApprovalrequestcount(syncRequest, userid);
                //do force update from Synch API
                string stApprovalPendingCount = await apiControllerObj.ForceUpdate(syncRequest, userid);

                //Deseralize the result returned by the API
                UserTaskcountJsonData userTaskPendingResponse = JsonConvert.DeserializeObject<UserTaskcountJsonData>(stApprovalPendingCount);

                //creates list Backend model object
                List<string> userBackends = new List<string>();
                userBackends = syncRequest.parameters.filters.backends;
                List<string> userBackendName = syncRequest.parameters.filters.backendName;
                //creates lists for Pending/Approval/Reject Count for json object result
                List<UserTaskcountJsonResult> userTaskPendingCount = userTaskPendingResponse.userTaskcountJsonResult;

                List<ApprovalCountDTO> approvalCountobj = new List<ApprovalCountDTO>();

                // Return Json formate object and pass to UI
                return Json(approvalCountobj, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error while creating client context : "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                return View("Error");
            }
        }
    }
}