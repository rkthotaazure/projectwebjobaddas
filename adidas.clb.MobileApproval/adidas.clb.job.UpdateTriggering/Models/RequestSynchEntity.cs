using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
namespace adidas.clb.job.UpdateTriggering.Models
{
    /// <summary>
    /// This class defines Request Synch Entity Class
    /// </summary>
    public class RequestSynchEntity : TableEntity
    {
        public RequestSynchEntity(string BackendRowKey, string RequestID)
        {
            this.PartitionKey = BackendRowKey;
            this.RowKey = RequestID;
        }
        public RequestSynchEntity() { }

        public string RequestID { get; set; }
        public string BackendID { get; set; }
        public string UserID { get; set; }
        public bool UpdateTriggered { get; set; }
        public int ExpectedLatency { get; set; }
        public DateTime LastUpdate { get; set; }
        public DateTime LastChange { get; set; }
        public DateTime ExpectedUpdate { get; set; }
    }
}
