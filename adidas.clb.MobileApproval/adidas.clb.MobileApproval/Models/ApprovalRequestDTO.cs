//-----------------------------------------------------------
// <copyright file="ApprovalRequestDTO.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace adidas.clb.MobileApproval.Models
{
    /// <summary>
    /// class which implements model for approvalrequest data transfer object.
    /// </summary>
    public class ApprovalRequestDTO
    {
        public string _type { get; set; }
        public string ServiceLayerReqID { get; set; }
        public string BackendID { get; set; }
        public ApprovalDTO approval { get; set; }
        public RequestDTO request { get; set; }
        public int retryAfter { get; set; }
        public ApprovalRequestDTO() { _type = "approvalRequest"; }
    }
}