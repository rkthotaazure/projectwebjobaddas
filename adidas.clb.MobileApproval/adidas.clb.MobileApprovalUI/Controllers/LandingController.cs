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
   // [Authorize]
    public class LandingController : Controller
    {
        // GET: Landing
        string userid = SettingsHelper.UserId;
        [HttpGet]
        public ActionResult ApprovalLanding()
        {
            return View();
        }
        public ActionResult ApprovalDetails()
        {
            return View();
        }
        public ActionResult DetailTaskInfo()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> SendApprovalstatus(ApprovalQuery approvalInfo)
        {
            try
            {
                approvalInfo.UserID = userid;
                //Api Controller object initialization
                APIController apiControllerObj = new APIController();
                string stApprovalrequeststatus = await apiControllerObj.SendApprovalInfo(approvalInfo, approvalInfo.ApprovalRequesID);
                
                List<ApprovalRequestDTO> requestsDetails = new List<ApprovalRequestDTO>();
               
                return Json(requestsDetails, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error while creating client context : "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                return View("Error");
            }
        }
        [HttpPost]
        public async Task<ActionResult> GetRequestDetails(RequestDetails requestInfo)
        {
            try
            {
                SynchRequestDTO syncRequest = requestInfo.syncRequest;
                syncRequest.userId = userid;
                //Api Controller object initialization
                APIController apiControllerObj = new APIController();
                string stApprovalrequest = await apiControllerObj.GetRequestInfo(syncRequest, requestInfo.requestID);
                string strApproverList= await apiControllerObj.GetApprovers(syncRequest, requestInfo.requestID);
                UserRequestJsonData userBackendjsonResponse = JsonConvert.DeserializeObject<UserRequestJsonData>(stApprovalrequest);
                List<ApprovalRequestDTO> requestsDetails = new List<ApprovalRequestDTO>();
                if (userBackendjsonResponse != null)
                {
                    foreach (userBackendRequest userbackendRequestdetails in userBackendjsonResponse.userBackendRequestinfo)
                    {
                        ApprovalRequestDTO requestObj = new ApprovalRequestDTO();
                        requestObj.approval = new ApprovalDTO();
                        //requestObj.approval.RequestId = userbackendRequestdetails.approvalDetails.RequestId;
                        //requestObj.approval.status = userbackendRequestdetails.approvalDetails.status;
                        requestObj.request = new RequestDTO();
                        requestObj.request.id = userbackendRequestdetails.requestDetails.id;
                        requestObj.request.title = userbackendRequestdetails.requestDetails.title;
                        requestObj.request.status = userbackendRequestdetails.requestDetails.status;
                        DateTime? created = userbackendRequestdetails.requestDetails.created;
                        if (created != null)
                        {
                            requestObj.request.created = created.Value;
                        }
                        requestObj.request.requester = new RequesterDTO();
                        if(requestObj.request.requester.userID != null)
                        {
                            requestObj.request.requester.userID = userbackendRequestdetails.requestDetails.requester.userID;
                        }
                        if (requestObj.request.requester.name != null)
                        {
                            requestObj.request.requester.name = userbackendRequestdetails.requestDetails.requester.name;
                        }
                        List<FieldDTO> requestFields = new List<FieldDTO>();
                        if(userbackendRequestdetails.requestDetails.fields.overview!=null && userbackendRequestdetails.requestDetails.fields.overview.Count>0)
                        {
                            foreach (FieldDTO field in userbackendRequestdetails.requestDetails.fields.overview)
                            {
                                FieldDTO overviewFields = new FieldDTO();
                                overviewFields.name = field.name;
                                overviewFields.value = field.value;
                                requestFields.Add(overviewFields);
                            }
                        }
                        if (userbackendRequestdetails.requestDetails.fields.genericInfo != null && userbackendRequestdetails.requestDetails.fields.overview.Count > 0)
                        {
                            foreach (FieldDTO field in userbackendRequestdetails.requestDetails.fields.genericInfo)
                            {
                                FieldDTO genericInfoFields = new FieldDTO();
                                genericInfoFields.name = field.name;
                                genericInfoFields.value = field.value;
                                requestFields.Add(genericInfoFields);
                            }
                        }
                        requestObj.request.fields = requestFields;
                        requestsDetails.Add(requestObj);
                    }
                }

                return Json(requestsDetails, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error while creating client context : "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                return View("Error");
            }
        }
        [HttpPost]
        public async Task<ActionResult> GetApprovalDetails(SynchRequestDTO syncRequest)
        {
            string userbackend = string.Empty;
            try
            {
                List<string> backendId = syncRequest.parameters.filters.backends;
                foreach (string backend in backendId)
                {
                    userbackend = backend;
                }
                //SynchRequestDTO syncRequest = new SynchRequestDTO();
                syncRequest.userId = userid;
                //Api Controller object initialization
                APIController apiControllerObj = new APIController();
                string stApprovalrequestcount = await apiControllerObj.GetUserBackendTasks(syncRequest, userid, userbackend);
                UserBackendrequestJsonData userBackendjsonResponse = JsonConvert.DeserializeObject<UserBackendrequestJsonData>(stApprovalrequestcount);
                //creates list Backend model object
                List<ApprovalRequestDTO> requestsDetails = new List<ApprovalRequestDTO>();
                if (userBackendjsonResponse != null)
                {
                    foreach (userBackendRequest userbackendRequestdetails in userBackendjsonResponse.userBackendRequestResults[0].userBackendRequestinfo)
                    {
                        ApprovalRequestDTO requestObj = new ApprovalRequestDTO();
                        requestObj.approval = new ApprovalDTO();
                        //requestObj.approval.RequestId = userbackendRequestdetails.approvalDetails.RequestId;
                        //requestObj.approval.status = userbackendRequestdetails.approvalDetails.status;
                        requestObj.request = new RequestDTO();
                        requestObj.request.id = userbackendRequestdetails.requestDetails.id;
                        requestObj.request.status= userbackendRequestdetails.requestDetails.status;
                        requestsDetails.Add(requestObj);
                    }
                }
              return Json(requestsDetails, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error while creating client context : "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                return View("Error");
            }
        }
        [HttpPost]
        public async Task<ActionResult> GetBackendApprovalrequestcount(SynchRequestDTO syncRequest)
        {
            try
            {
                //SynchRequestDTO syncRequest = new SynchRequestDTO();
                syncRequest.userId = userid;
                //Api Controller object initialization
                APIController apiControllerObj = new APIController();
                string stApprovalrequestcount = await apiControllerObj.GetApprovalrequestcount(syncRequest, userid);
                UserBackendrequestJsonData userBackendjsonResponse = JsonConvert.DeserializeObject<UserBackendrequestJsonData>(stApprovalrequestcount);
                //creates list Backend model object
                List<UserBackendDTO> userBackendinfo = new List<UserBackendDTO>();
                //Checks whether the JSON response is not null
                if (userBackendjsonResponse != null)
                {
                    //Iterate user backend json format result and bind to Model
                    foreach (UserBackendRequestJsonResult UserbackendInfo in userBackendjsonResponse.userBackendRequestResults)
                    {
                        //Create Model object
                        UserBackendDTO BackendObj = new UserBackendDTO();
                        BackendObj.UserID = UserbackendInfo.UserID;
                        BackendObj.backend = new BackendDTO();
                        //setting the properties of Model
                        BackendObj.backend.BackendID = UserbackendInfo.userBackend.BackendID;
                        BackendObj.backend.DefaultUpdateFrequency = UserbackendInfo.userBackend.DefaultUpdateFrequency;
                        BackendObj.backend.AverageAllRequestsLatency = UserbackendInfo.userBackend.AverageAllRequestsLatency;
                        BackendObj.backend.AverageAllRequestsSize = UserbackendInfo.userBackend.AverageAllRequestsSize;
                        BackendObj.backend.AverageRequestLatency = UserbackendInfo.userBackend.AverageRequestLatency;
                        BackendObj.backend.AverageRequestSize = UserbackendInfo.userBackend.AverageRequestSize;
                        BackendObj.backend.ExpectedLatency = UserbackendInfo.userBackend.ExpectedLatency;
                        DateTime? expdate = UserbackendInfo.userBackend.ExpectedUpdate;
                        if (expdate != null)
                        {
                            BackendObj.backend.ExpectedUpdate = expdate.Value;
                        }
                        BackendObj.backend.LastAllRequestsLatency = UserbackendInfo.userBackend.LastAllRequestsLatency;
                        BackendObj.backend.LastAllRequestsSize = UserbackendInfo.userBackend.LastAllRequestsSize;
                        BackendObj.backend.LastRequestLatency = UserbackendInfo.userBackend.LastRequestLatency;
                        BackendObj.backend.LastRequestSize = UserbackendInfo.userBackend.LastRequestSize;
                        DateTime? lstdate = UserbackendInfo.userBackend.LastUpdate;
                        if (expdate != null)
                        {
                            BackendObj.backend.LastUpdate = lstdate.Value;
                        }
                        BackendObj.backend.OpenApprovals = UserbackendInfo.userBackend.OpenApprovals;
                        BackendObj.backend.OpenRequests = UserbackendInfo.userBackend.OpenRequests;
                        BackendObj.backend.TotalBatchRequestsCount = UserbackendInfo.userBackend.TotalBatchRequestsCount;
                        BackendObj.backend.TotalRequestsCount = UserbackendInfo.userBackend.TotalRequestsCount;
                        BackendObj.backend.UpdateTriggered = UserbackendInfo.userBackend.UpdateTriggered;
                        BackendObj.backend.UrgentApprovals = UserbackendInfo.userBackend.UrgentApprovals;

                        // BackendObj.backend.MissingConfirmationsLimit = UserbackendInfo.userBackend.MissingConfirmationsLimit;
                        //Adding the Model object to the list
                        userBackendinfo.Add(BackendObj);
                    }
                }

                //if (userBackendinfo == null)
                //{
                //    return Json(userBackendinfo, JsonRequestBehavior.AllowGet);
                //}
                return Json(userBackendinfo, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error while creating client context : "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                return View("Error");
            }
        }
        public List<UserBackendEntity> GetUserAllBackends(String UserID)
        {
            try
            {
                //get's azure table instance
                CloudTable UserDeviceConfigurationTable = GetAzureTableInstance("UserDeviceConfiguration");
                TableQuery<UserBackendEntity> query = new TableQuery<UserBackendEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, string.Concat("UB_", UserID)));
                List<UserBackendEntity> alluserbackends = UserDeviceConfigurationTable.ExecuteQuery(query).ToList();
                return alluserbackends;
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error while retrieving userbackends from userdeviceconfig azure table in DAL : "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new DataAccessException();
            }
        }
        public List<ApprovalEntity> GetUserBackendTasks(String UserID,String BackendId)
        {
            try
            {
                //get's azure table instance
                CloudTable RequestTrasactionTable = GetAzureTableInstance("RequestTransactions");
                string partitionFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, string.Concat("Approval_", UserID));
                string backendFilter = TableQuery.GenerateFilterCondition("BackendID", QueryComparisons.Equal, BackendId);
                string statusFilter = TableQuery.GenerateFilterCondition("status", QueryComparisons.Equal, "Waiting");
                string getBackend = TableQuery.CombineFilters(TableQuery.CombineFilters(partitionFilter, TableOperators.And, backendFilter), TableOperators.And, statusFilter);
                TableQuery <ApprovalEntity> query = new TableQuery<ApprovalEntity>().Where(getBackend);               
                List<ApprovalEntity> alluserbackends = RequestTrasactionTable.ExecuteQuery(query).ToList();
                return alluserbackends;
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error while retrieving userbackends from userdeviceconfig azure table in DAL : "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new DataAccessException();
            }
        }
        public static CloudTable GetAzureTableInstance(String TableName)
        {
            // Retrieve the storage account from the connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
            CloudConfigurationManager.GetSetting("GenericMobileStorageConnectionString"));
            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            // Create the CloudTable object that represents the table.
            CloudTable table = tableClient.GetTableReference(TableName);
            return table;
        }
    }
}