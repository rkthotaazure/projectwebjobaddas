using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace adidas.clb.MobileApproval.Models
{
    public class SyncNewRequestsCountDTO
    {

        public string _type { get; set; }
        public SynchRequestDTO query { get; set; }        
        public ErrorDTO error { get; set; }
        public int UnReadRequestsCount { get; set; }
        public SyncNewRequestsCountDTO()
        {
            _type = "syncnewrequestscount";
        }
    }
}