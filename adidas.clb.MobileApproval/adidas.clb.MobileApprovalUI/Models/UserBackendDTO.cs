//-----------------------------------------------------------
// <copyright file="UserBackendDTO.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace adidas.clb.MobileApprovalUI.Models
{/// <summary>
 /// class which implements model for userbackend data transfer object.
 /// </summary>
    public class UserBackendDTO
    {
        public string _type { get; set; }
        public string UserID { get; set; }
        public BackendDTO backend { get; set; }
        public SynchDTO synch { get; set; }
        public List<ApprovalRequestDTO> requests { get; set; }
        public UserBackendDTO() { _type = "userBackend"; }
    }
    public class BackendDTO
    {
        public string _type { get; set; }
        public string BackendID { get; set; }
        public Int32 OpenApprovals { get; set; }
        public Int32 DefaultUpdateFrequency { get; set; }
        public bool UpdateTriggered { get; set; }
        public Int32 ExpectedLatency { get; set; }
        //public DateTime? LastUpdate { get; set; }
        private DateTime? lastUpdate = null;
        public DateTime LastUpdate
        {
            get
            {
                return this.lastUpdate.HasValue ? this.lastUpdate.Value : DateTime.Now;
            }

            set { this.lastUpdate = value; }
        }
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
        //public DateTime? ExpectedUpdate { get; set; }
        private DateTime? expectedUpdate = null;
        public DateTime ExpectedUpdate
        {
            get
            {
                return this.expectedUpdate.HasValue ? this.expectedUpdate.Value : DateTime.Now;
            }

            set { this.expectedUpdate = value; }
        }
        public Int32 TotalRequestsCount { get; set; }
        public Int32 TotalBatchRequestsCount { get; set; }
        public BackendDTO() { _type = "backend"; }
    }
    public class SynchDTO
    {
        public int avgSynchFreq { get; set; }
        public int lastSynchFreq { get; set; }
        public int bestSynchFreq { get; set; }
        public int retryAfter { get; set; }
        public int totalReqCount { get; set; }
        public int urgentReqCount { get; set; }
    }
}