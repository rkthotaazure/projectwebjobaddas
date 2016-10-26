using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
namespace adidas.clb.MobileApproval.Models
{
    public class BackendEntity : TableEntity
    {
        public BackendEntity(string strPartitionkey, string strRowkey)
        {
            this.PartitionKey = strPartitionkey;
            this.RowKey = strRowkey;
        }
        public BackendEntity() { }

        public string Title { get; set; }
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