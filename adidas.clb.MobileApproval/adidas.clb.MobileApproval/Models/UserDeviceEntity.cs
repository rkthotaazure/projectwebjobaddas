using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
namespace adidas.clb.MobileApproval.Models
{
    public class UserDeviceEntity: TableEntity
    {
        public UserDeviceEntity(string UserRowKey,string DeviceRowKey)
        {
            this.PartitionKey = UserRowKey;
            this.RowKey = DeviceRowKey;
        }
        public UserDeviceEntity() { }
    }
}