using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace adidas.clb.MobileApprovalUI.Models
{
    public class RequestDetails
    {
        public SynchRequestDTO syncRequest { get; set; }
        public string requestID { get; set; }
        public string taskID { get; set; }
        public string taskViewStatus { get; set; }

    }
}