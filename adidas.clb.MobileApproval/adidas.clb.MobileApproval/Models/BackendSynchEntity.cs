using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
namespace adidas.clb.MobileApproval.Models
{
    public class BackendSynchEntity : TableEntity
    {
        public BackendSynchEntity(string UserDeviceRowKey, string BackendRowKey)
        {
            this.PartitionKey = UserDeviceRowKey;
            this.RowKey = BackendRowKey;
        }


        public BackendSynchEntity() { }

        public DateTime LastSynch { get; set; }
        public int AverageSynchFrequency { get; set; }
        public int LastSynchFrequency { get; set; }
        public int BestSynchFrequency { get; set; }


    }
}