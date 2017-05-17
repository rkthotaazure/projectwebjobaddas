using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace adidas.clb.MobileApproval.Models
{
    public class ApprovalCountDTO
    {
        public string BackendID { get; set; }
        public string BackendName { get; set; }
        public int WaitingCount { get; set; }
        public int UrgentPendingCount { get; set; }
        public int ApprovedCount { get; set; }
        public int RejectedCount { get; set; }
    }
    public class SyncResponseResultDTO<T>
    {
        public string _type { get; set; }
        public SynchRequestDTO query { get; set; }
        public List<T> result { get; set; }
        public ErrorDTO error { get; set; }
        public int SyncTime { get; set; }
        public SyncResponseResultDTO()
        {
            _type = "syncResponseResult";
        }
    }
}