//-----------------------------------------------------------
// <copyright file="ApproverEntity.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace adidas.clb.MobileApproval.Models
{
    /// <summary>
    /// class which implements model for Approver entity.
    /// </summary>
    public class ApproverEntity : TableEntity
    {
        public ApproverEntity(string type, string RequestApprovalId)
        {
            this.PartitionKey = type;
            this.RowKey = RequestApprovalId;
        }
        public ApproverEntity() { }
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
    }
}