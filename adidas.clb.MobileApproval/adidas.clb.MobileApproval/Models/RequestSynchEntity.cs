using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
namespace adidas.clb.MobileApproval.Models
{
    public class RequestSynchEntity : TableEntity
    {
        public RequestSynchEntity(string UserDeviceRowKey,string RequestRowKey)
        {
            this.PartitionKey = UserDeviceRowKey;
            this.RowKey = RequestRowKey;
        }
        public RequestSynchEntity() { }
        public DateTime LastSync { get; set; }

         

    }
}