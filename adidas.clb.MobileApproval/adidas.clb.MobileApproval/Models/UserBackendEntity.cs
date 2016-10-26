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
        public UserBackendEntity(string UserRowKey, string BackendRowKey)
        {
            this.PartitionKey = UserRowKey;
            this.RowKey = BackendRowKey;
        }

        public UserBackendEntity() { }
        public int UpdateFrequency { get; set; }
        public bool UpdateTriggered { get; set; }
        public int ExpectedLatency { get; set; }
        public DateTime LastUpdate { get; set; }
        public int OpenRequests { get; set; }
        public int OpenApprovals { get; set; }
        public int UrgentApprovals { get; set; }
        public int AverageRequestSize { get; set; }
        public int LastRequestSize { get; set; }
        public int AverageAllRequestsSize { get; set; }
        public int LastAllRequestsSize { get; set; }
        public int AverageAllRequestsLatency { get; set; }
        public int LastAllRequestsLatency { get; set; }   
        public int AverageRequestLatency { get; set; }
        public int LastRequestLatency { get; set; }

    }
}