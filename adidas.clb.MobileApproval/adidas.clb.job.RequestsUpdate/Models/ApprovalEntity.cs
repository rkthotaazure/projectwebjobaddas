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

namespace adidas.clb.job.RequestsUpdate.Models
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
        public Boolean BackendConfirmed { get; set; }
        public int Missingconfirmations { get; set; }
        public Boolean Backendoverwritten { get; set; }
    }   
}
