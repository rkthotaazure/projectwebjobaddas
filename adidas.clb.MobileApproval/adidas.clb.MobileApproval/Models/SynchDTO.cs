using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace adidas.clb.MobileApproval.Models
{
    public class SynchDTO
    {
        public int avgSynchFreq { get; set; }
        public int lastSynchFreq { get; set; }
        public int bestSynchFreq { get; set; }
        public int retryAfter { get; set; }
        public int totalReqCount { get; set; }
        public int urgentReqCount { get; set; }
    }
}