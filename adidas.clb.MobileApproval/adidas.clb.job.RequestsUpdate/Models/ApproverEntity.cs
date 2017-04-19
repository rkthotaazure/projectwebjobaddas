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

namespace adidas.clb.job.RequestsUpdate.Models
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
        public int Order { get; set; }
        public string UserID { get; set; }
        public string UserName { get; set; }
        private DateTime? created = null;

        public DateTime? Created {
            get
            {
                return this.created.HasValue ? this.created.Value == default(DateTime) ? (DateTime?)null : this.created.Value : (DateTime?)null;
            }

            set { this.created = value; }
        }
        public string Status { get; set; }
        private DateTime? dueDate = null;
        public DateTime? DueDate
        {
            get
            {
                return this.dueDate.HasValue ? this.dueDate.Value == default(DateTime) ? (DateTime?)null : this.dueDate.Value : (DateTime?)null;
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
        public string Comment { get; set; }
    }
}
