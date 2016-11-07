//-----------------------------------------------------------
// <copyright file="UserBackendEntity.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using System;
using System.Collections.Generic;
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
        public Int32 UpdateFrequency { get; set; }
        public bool UpdateTriggered { get; set; }
        public Int32 ExpectedLatency { get; set; }
        public DateTime LastUpdate { get; set; }
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

    }
}