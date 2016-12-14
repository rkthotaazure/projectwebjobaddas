using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace adidas.clb.MobileApprovalUI.Models
{
    public class Backend
    {
        public string BackendID { get; set; }
        public string DefaultUpdateFrequency { get; set; }
        public int MissingConfirmationsLimit { get; set; }
        

    }
}