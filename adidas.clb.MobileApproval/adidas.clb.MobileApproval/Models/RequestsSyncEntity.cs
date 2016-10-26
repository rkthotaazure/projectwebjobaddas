using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
namespace adidas.clb.MobileApproval.Models
{
    public class RequestsSyncEntity : TableEntity
    {
        public RequestsSyncEntity(string UserDeviceRowKey, string RequestRowKey)
        {
            this.PartitionKey = UserDeviceRowKey;
            this.RowKey = RequestRowKey;
        }
        public RequestsSyncEntity() { }

        public bool UpdateTriggered { get; set; }
        public int ExpectedLatency { get; set; }
        public bool LastUpdate { get; set; }
        public bool LastChange { get; set; }


    }
}