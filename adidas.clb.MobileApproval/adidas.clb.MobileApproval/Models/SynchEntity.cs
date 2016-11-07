using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.Storage.Table;

namespace adidas.clb.MobileApproval.Models
{
    public class SynchEntity:TableEntity
    {
        public SynchEntity(string partitionkey, string rowkey)
        {
            this.PartitionKey = partitionkey;
            this.RowKey = rowkey;
        }
        public int avgSynchFreq { get; set; }
        public int lastSynchFreq { get; set; }
        public int bestSynchFreq { get; set; }
        public int retryAfter { get; set; }
        public int totalReqCount { get; set; }
        public int urgentReqCount { get; set; }
        public SynchEntity() { }
    }
}