//-----------------------------------------------------------
// <copyright file="ApproverDTO.cs" company="adidas AG">
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
    /// class which implements model for approver data transfer object.
    /// </summary>
    public class ApproverDTO
    {
        public string _type { get; set; }
        public int order { get; set; }
        public string UserID { get; set; }
        public string UserName { get; set; }
        public DateTime Created { get; set; }
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
        public ApproverDTO() { _type = "approver"; }
    }
}