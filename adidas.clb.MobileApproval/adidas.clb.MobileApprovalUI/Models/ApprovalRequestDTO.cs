//-----------------------------------------------------------
// <copyright file="ApprovalRequestDTO.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace adidas.clb.MobileApprovalUI.Models
{
    /// <summary>
    /// class which implements model for approvalrequest data transfer object.
    /// </summary>
    public class ApprovalRequestDTO
    {
        public string _type { get; set; }
        public string serviceLayerReqID { get; set; }
        public string BackendID { get; set; }
        public ApprovalDTO approval { get; set; }
        public RequestDTO request { get; set; }
        public int retryAfter { get; set; }
        public ApprovalRequestDTO() { _type = "approvalRequest"; }
    }
    public class ApprovalDTO
    {
        public string RequestId { get; set; }
        public string BackendID { get; set; }
        public string Status { get; set; }
        private DateTime? dueDate = null;
        public DateTime DueDate
        {
            get
            {
                return this.dueDate.HasValue ? this.dueDate.Value : DateTime.Now;
            }

            set { this.dueDate = value; }
        }
        private DateTime? decisionDate = null;
        public DateTime DecisionDate
        {
            get
            {
                return this.decisionDate.HasValue ? this.decisionDate.Value : DateTime.Now;
            }

            set { this.decisionDate = value; }
        }
        public Boolean BackendConfirmed { get; set; }
        public int Missingconfirmations { get; set; }
        public Boolean Backendoverwritten { get; set; }
    }
}