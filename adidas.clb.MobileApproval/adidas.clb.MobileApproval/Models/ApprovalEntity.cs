using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace adidas.clb.MobileApproval.Models
{
    public class ApprovalEntity : TableEntity
    {
        public ApprovalEntity(string UserRowKey, string RequestRowKey)
        {
            this.PartitionKey = UserRowKey;
            this.RowKey = RequestRowKey;
        }
        public ApprovalEntity() { }
        public string ApprovalRowKey { get; set; }
        public DateTime ApprovalTimestamp { get; set; }
        public bool BackendConfirmed { get; set; }
        public int MissingConfirmations { get; set; }
        public bool BackendOverwritten { get; set; }



    }
}