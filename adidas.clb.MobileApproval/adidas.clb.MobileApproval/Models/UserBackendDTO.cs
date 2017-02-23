//-----------------------------------------------------------
// <copyright file="UserBackendDTO.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace adidas.clb.MobileApproval.Models
{
    /// <summary>
    /// class which implements model for userbackend data transfer object.
    /// </summary>
    public class UserBackendDTO
    {        
        public string _type { get; set; }
        public string UserID { get; set; }
        public Backend backend { get; set; }
        public SynchDTO synch { get; set; }
        public List<ApprovalRequestDTO> requests { get; set; }
        public ApprovalsCountDTO approvalsCount { get; set; }
        public UserBackendDTO() { _type = "userBackend"; }
    }

    /// <summary>
    /// class which implements model for backend data transfer object for userbackend.
    /// </summary>
    public class Backend
    {
        public string _type { get; set; }
        public string BackendID { get; set; }
        public string BackendName { get; set; }
        public Int32 OpenApprovals { get; set; }
        public Int32 DefaultUpdateFrequency { get; set; }
        public bool UpdateTriggered { get; set; }
        public Int32 ExpectedLatency { get; set; }
        private DateTime? lastUpdate = null;
        public DateTime? LastUpdate
        {
            get
            {
                return this.lastUpdate.HasValue ? this.lastUpdate.Value : (DateTime?)null;
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
        private DateTime? expectedUpdate = null;
        public DateTime? ExpectedUpdate
        {
            get
            {
                return this.expectedUpdate.HasValue ? this.expectedUpdate.Value : (DateTime?)null;
            }

            set { this.expectedUpdate = value; }
        }
        public Int32 TotalRequestsCount { get; set; }
        public Int32 TotalBatchRequestsCount { get; set; }
        public Backend() { _type = "backend"; }
    }
}