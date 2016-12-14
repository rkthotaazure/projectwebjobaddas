using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace adidas.clb.MobileApprovalUI.Models
{
    public class Device
    {
        public string id { get; set; }
        public string name { get; set; }
        public string brand { get; set; }
        public string model { get; set; }
        public int maxSynchReplySize { get; set; }
        
    }
}