using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace adidas.clb.MobileApprovalUI.Models
{
    public class ApprovalCountDTO
    {
            public string BackendID { get; set; }
            public string BackendName { get; set; }
            public int WaitingCount { get; set; }
            public int ApprovedCount { get; set; }
            public int RejectedCount { get; set; }
    }
}