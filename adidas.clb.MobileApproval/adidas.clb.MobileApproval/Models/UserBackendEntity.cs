//-----------------------------------------------------------
// <copyright file="UserBackendEntity.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace adidas.clb.MobileApproval.Models
{
    public class UserBackendEntity : TableEntity
    {
        /// <summary>
        /// class which implements model for userbackendentity.
        /// </summary>
        public UserBackendEntity(string UserRowKey, string BackendRowKey)
        {
            this.PartitionKey = UserRowKey;
            this.RowKey = BackendRowKey;
        }

        public UserBackendEntity() { }

        public string UserID { get; set; }
        public string BackendID { get; set; }
        public Int32 OpenApprovals { get; set; }
        public Int32 DefaultUpdateFrequency { get; set; }
        public bool UpdateTriggered { get; set; }
        public Int32 ExpectedLatency { get; set; }
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
    }
}