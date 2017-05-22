using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
namespace adidas.clb.MobileApproval.Models
{
    public class ApprovalEntity : TableEntity
    {
        public ApprovalEntity(string UserRowKey, string RequestRowKey)
        {
            this.PartitionKey = UserRowKey;
            this.RowKey = RequestRowKey;
        }
        public ApprovalEntity() { }
        public string RequestId { get; set; }
        public string TaskTitle { get; set; }
        public string BackendID { get; set; }
        public string Domain { get; set; }
        public string Status { get; set; }
        private DateTime? created;
        public DateTime? Created
        {
            get
            {
                return this.created.HasValue ? this.created.Value == default(DateTime) ? (DateTime?)null : this.created.Value : (DateTime?)null;
            }

            set { this.created = value; }
        }
        private DateTime? dueDate = null;
        public DateTime? DueDate
        {
            get
            {
                return this.dueDate.HasValue ? this.dueDate.Value : (DateTime?)null;
            }

            set { this.dueDate = value; }
        }
        private DateTime? decisionDate = null;
        public DateTime? DecisionDate
        {
            get
            {
                return this.decisionDate.HasValue ? this.decisionDate.Value : (DateTime?)null;
            }

            set { this.decisionDate = value; }
        }
        public Boolean BackendConfirmed { get; set; }
        public int Missingconfirmations { get; set; }
        public Boolean Backendoverwritten { get; set; }
        public string ServiceLayerTaskID { get; set; }
        public string Comment { get; set; }
        public string TaskStatus { get; set; }
        public string TaskViewStatus { get; set; }
    }

    public class ApprovalDTO
    {
        public string RequestId { get; set; }
        public string TaskTitle { get; set; }
        public string BackendID { get; set; }
        public string Domain { get; set; }
        public string Status { get; set; }
        private DateTime? created;
        public DateTime? Created
        {
            get
            {
                return this.created.HasValue ? this.created.Value == default(DateTime) ? (DateTime?)null : this.created.Value : (DateTime?)null;
            }

            set { this.created = value; }
        }
        private DateTime? dueDate = null;
        public DateTime? DueDate
        {
            get
            {
                return this.dueDate.HasValue ? this.dueDate.Value : (DateTime?)null;
            }

            set { this.dueDate = value; }
        }
        private DateTime? decisionDate = null;
        public DateTime? DecisionDate
        {
            get
            {
                return this.decisionDate.HasValue ? this.decisionDate.Value : (DateTime?)null;
            }

            set { this.decisionDate = value; }
        }
        public Boolean BackendConfirmed { get; set; }
        public int Missingconfirmations { get; set; }
        public Boolean Backendoverwritten { get; set; }
        public string ServiceLayerTaskID { get; set; }
        public string Comment { get; set; }
        public string TaskStatus { get; set; }
        public string TaskViewStatus { get; set; }
    }

    public class ApprovalsCountDTO
    {
        public string BackendID { get; set; }
        public string Status { get; set; }
        public int Count { get; set; }
    }
}