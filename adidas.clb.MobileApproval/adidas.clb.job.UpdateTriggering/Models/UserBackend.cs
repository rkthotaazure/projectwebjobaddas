//-----------------------------------------------------------
// <copyright file="UserBackend.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
namespace adidas.clb.job.UpdateTriggering.Models
{
    public class UserBackend : TableEntity
    {
        /// <summary>
        /// class which implements model for userbackendentity.
        /// </summary>
        public UserBackend(string UserRowKey, string BackendRowKey)
        {
            this.PartitionKey = UserRowKey;
            this.RowKey = BackendRowKey;
        }

        public UserBackend() { }

        public string UserID { get; set; }
        public string BackendID { get; set; }
        public int OpenApprovals { get; set; }
        public int DefaultUpdateFrequency { get; set; }
        public bool UpdateTriggered { get; set; }
        public int ExpectedLatency { get; set; }
        public DateTime LastUpdate { get; set; }
        public int OpenRequests { get; set; }
        public int UrgentApprovals { get; set; }
        public int AverageRequestSize { get; set; }
        public int LastRequestSize { get; set; }
        public int AverageAllRequestsSize { get; set; }
        public int LastAllRequestsSize { get; set; }
        public int AverageAllRequestsLatency { get; set; }
        public int LastAllRequestsLatency { get; set; }
        public int AverageRequestLatency { get; set; }
        public int LastRequestLatency { get; set; }
        public DateTime ExpectedUpdate { get; set; }
    }
}
