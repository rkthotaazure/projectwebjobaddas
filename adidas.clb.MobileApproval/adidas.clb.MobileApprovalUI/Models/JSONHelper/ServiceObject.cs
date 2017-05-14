//-----------------------------------------------------------
// <copyright file="ChannelDetails.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace adidas.clb.MobileApprovalUI.Models.JSONHelper
{
    /// <summary>
    /// The model used for serializing the Service Object details from JSON format.
    /// </summary>
    public class ServiceObject
    {

    }
    public class BackendJsonData
    {
        [JsonProperty(PropertyName = "result")]
        public BackendJsonResult[] Results { get; set; }
    }
    public class BackendJsonResult
    {
       public string BackendID { get; set; }
        public string BackendName { get; set; }
        public int MissingConfirmationsLimit { get; set; }

    }
    public class NewuserJsonData
    {
        [JsonProperty(PropertyName = "result")]
        public NewUserJsonResult userResults { get; set; }
        
        
    }
    public class UserBackendJsonData
    {
        [JsonProperty(PropertyName = "result")]
        public UserBackendJsonResult userBackendResults { get; set; }

    }
    public class UserTaskcountJsonData
    {
        [JsonProperty(PropertyName = "result")]
        public List<UserTaskcountJsonResult> userTaskcountJsonResult { get; set; }

    }
    public class UserTaskcountJsonResult
    {
        public string BackendID { get; set; }
        public string Status { get; set; }
        public int Count { get; set; }

    }
        public class UserBackendrequestJsonData
    {
        [JsonProperty(PropertyName = "result")]
        public UserBackendRequestJsonResult[] userBackendRequestResults { get; set; }

    }
    public class UserBackendRequestJsonResult
    {
        public string UserID { get; set; }

        [JsonProperty(PropertyName = "backend")]
        public userBackend userBackend { get; set; }

        [JsonProperty(PropertyName = "requests")]
        public userBackendRequest[] userBackendRequestinfo { get; set; }
        [JsonProperty(PropertyName = "approvalsCount")]
        public UserTaskcountJsonResult userTaskcountJsonResult { get; set; }
    }
    public class UserRequestJsonData
    {
        [JsonProperty(PropertyName = "result")]
        public userBackendRequest[] userBackendRequestinfo { get; set; }

    }
    public class userBackendRequest
    {
        public string serviceLayerReqID { get; set; }
        public string BackendID { get; set; }

        [JsonProperty(PropertyName = "approval")]
        public Approval approvalDetails { get; set; }

        [JsonProperty(PropertyName = "request")]
        public Request requestDetails { get; set; }
        public int retryAfter { get; set; }
    }
    public class Request
    {
        public string ID { get; set; }
        public string Title { get; set; }
        public DateTime Created { get; set; }
        public string Status { get; set; }
        public int Latency { get; set; }
        public Requester Requester { get; set; }
        public Fields Fields { get; set; }
        public List<ApproversJson> Approvers { get; set; }
    }

    public class UserApprovalJsonData
    {
        [JsonProperty(PropertyName = "result")]
        public ApproversJson[] userApprovalinfo { get; set; }

    }
    public class Approval
    {
        public string RequestId { get; set; }
        public string TaskTitle { get; set; }
        public string BackendID { get; set; }
        public string Status { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? DecisionDate { get; set; }        
        public Boolean BackendConfirmed { get; set; }
        public int Missingconfirmations { get; set; }
        public Boolean Backendoverwritten { get; set; }
        public string ServiceLayerTaskID { get; set; }
    }
    public class Requester
    {
        public string UserID { get; set; }
        public string Name { get; set; }
    }

    public class Fields
    {
        public List<FieldDTO> Overview { get; set; }
        public List<FieldDTO> GenericInfo { get; set; }
    }

    public class ApproversJson
    {
        public int Order { get; set; }
        public string UserID { get; set; }
        public string UserName { get; set; }
        public DateTime? Created { get; set; }
        public string Status { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? DecisionDate { get; set; }
        public string Comment { get; set; }
    }

    public class UserDeviceJsonData
    {
        [JsonProperty(PropertyName = "result")]
        public UserDevicesJsonResult userDevicesResults { get; set; }

    }
    public class UserBackendJsonResult
    {
        [JsonProperty(PropertyName = "userbackends")]

        public userBackenddetails[] userBackenddetails { get; set; }
    }
    public class userBackenddetails
    {
        public string UserID { get; set; }

        [JsonProperty(PropertyName = "backend")]
        public userBackend userBackend { get; set; }
    }
    public class NewUserJsonResult
    {
        public string UserID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Fullname { get; set; }
        public string Email { get; set; }
        public string Domain { get; set; }
        public string DeviceName { get; set; }
        public string DeviceOS { get; set; }
        public int CompletedRequestsSync { get; set; }
        public int AutoRefreshValue { get; set; }
    }
    public class userBackend
    {
        public string BackendID { get; set; }
        public string BackendName { get; set; }
        public Int32 OpenApprovals { get; set; }
        public Int32 DefaultUpdateFrequency { get; set; }
        public bool UpdateTriggered { get; set; }
        public Int32 ExpectedLatency { get; set; }
        public DateTime? LastUpdate { get; set; }
        public Int32 OpenRequests { get; set; }
        public Int32 UrgentApprovals { get; set; }
        public Int32 AverageRequestSize { get; set; }
        public Int32 LastRequestSize { get; set; }
        public Int32 AverageAllRequestsSize { get; set; }
        public Int32 LastAllRequestsSize { get; set; }
        public Int32 AverageAllRequestsLatency { get; set; }
        public Int32 LastAllRequestsLatency { get; set; }
        public Int32 AverageRequestLatency { get; set; }
        public Int32 LastRequestLatency { get; set; }
        public DateTime? ExpectedUpdate { get; set; }
        public Int32 TotalRequestsCount { get; set; }
        public Int32 TotalBatchRequestsCount { get; set; }

    }
    public class UserDevicesJsonResult
    {
        [JsonProperty(PropertyName = "userdevices")]
        public userDevicedetails[] userDevicedetails { get; set; }
        
    }
    public class userDevicedetails
    {
        public string UserID { get; set; }

        [JsonProperty(PropertyName = "device")]
        public userDevices userDevices { get; set; }
    }
    public class userDevices
    {
        public string DeviceID { get; set; }
        public string DeviceName { get; set; }
        public string DeviceBrand { get; set; }
        public string DeviceModel { get; set; }
        public int maxSynchReplySize { get; set; }
    }
    public class SynchTimeResponseDTO
    {
        public string _type { get; set; }
        public ErrorDTO error { get; set; }
        public int SyncTime { get; set; }
        public SynchTimeResponseDTO()
        {
            _type = "synchResponse";
        }
    }
}