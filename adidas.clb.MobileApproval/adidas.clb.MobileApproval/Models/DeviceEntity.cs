using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
namespace adidas.clb.MobileApproval.Models
{
    public class DeviceEntity: TableEntity
    {
        public DeviceEntity(string partitionkey ,string DeviceID)
        {
            this.PartitionKey = partitionkey;
            this.RowKey = DeviceID;
        }

        public DeviceEntity() { }
        public string DeviceOS { get; set; }
    }
}