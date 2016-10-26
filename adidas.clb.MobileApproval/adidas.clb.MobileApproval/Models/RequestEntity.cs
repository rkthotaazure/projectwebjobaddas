using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
namespace adidas.clb.MobileApproval.Models
{
    public class RequestEntity : TableEntity
    {

        public RequestEntity(String BackendRowKey, string guid)
        {
            this.PartitionKey = BackendRowKey;
            this.RowKey = guid;
        }
        public RequestEntity() { }
        public int AgentPullingFrequency { get; set; }

        public int DefaultUpdateFrequency { get; set; }
        public int AverageRequestSize { get; set; }
        public int LastRequestSize { get; set; }
        public int AverageAllRequestsLatency { get; set; }
        public int LastAllRequestsLatency { get; set; }
        public int AverageRequestLatency { get; set; }
        public int LastRequestLatency { get; set; }
        public int MissingConfirmationsLimit { get; set; }

    }
}