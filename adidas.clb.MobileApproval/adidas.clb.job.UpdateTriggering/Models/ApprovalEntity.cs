//-----------------------------------------------------------
// <copyright file="ApprovalEntity.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace adidas.clb.job.UpdateTriggering.Models
{
    /// <summary>
    /// class which implements model for Approval entity.
    /// </summary>
    public class ApprovalEntity : TableEntity
    {
        public ApprovalEntity(string type, string RequestApprovalId)
        {
            this.PartitionKey = type;
            this.RowKey = RequestApprovalId;
        }
        public ApprovalEntity() { }
        public string RequestId { get; set; }
        public string BackendID { get; set; }
        public string Status { get; set; }
        private DateTime? dueDate = null;
        public DateTime? DueDate
        {
            get
            {
                return this.dueDate.HasValue ? this.dueDate.Value== default(DateTime) ? (DateTime?)null : this.dueDate.Value : (DateTime?)null;
            }

            set { this.dueDate = value; }
        }
        private DateTime? decisionDate = null;
        public DateTime? DecisionDate
        {
            get
            {
                return this.decisionDate.HasValue ? this.decisionDate.Value == default(DateTime) ? (DateTime?)null : this.decisionDate.Value : (DateTime?)null;
            }

            set { this.decisionDate = value; }
        }
        public Boolean BackendConfirmed { get; set; }
        public int Missingconfirmations { get; set; }
        public string ServiceLayerTaskID { get; set; }
        public string TaskTitle { get; set; }
        public Boolean Backendoverwritten { get; set; }
    }
}
