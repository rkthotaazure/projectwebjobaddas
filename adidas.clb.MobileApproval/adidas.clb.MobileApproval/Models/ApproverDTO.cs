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
        public string userID { get; set; }
        public string userName { get; set; }
        public DateTime created { get; set; }
        public string status { get; set; }
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