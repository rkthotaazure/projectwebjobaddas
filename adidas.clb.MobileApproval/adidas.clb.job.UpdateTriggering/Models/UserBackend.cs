//-----------------------------------------------------------
// <copyright file="UserBackend.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
namespace adidas.clb.job.UpdateTriggering.Models
{
    public class UserBackend : TableEntity
    {
        /// <summary>
        /// class which implements model for userbackendentity.
        /// </summary>
        public UserBackend(string UserRowKey, string BackendRowKey)
        {
            this.PartitionKey = UserRowKey;
            this.RowKey = BackendRowKey;
        }

        public UserBackend() { }

        public string UserID { get; set; }
        public string BackendName { get; set; }
        public string BackendID { get; set; }
        public int OpenApprovals { get; set; }
        public int DefaultUpdateFrequency { get; set; }
        public bool UpdateTriggered { get; set; }
        public int ExpectedLatency { get; set; }
        public DateTime LastUpdate { get; set; }
        public int OpenRequests { get; set; }
        public int UrgentApprovals { get; set; }
        public int AverageRequestSize { get; set; }
        public int LastRequestSize { get; set; }
        public int AverageAllRequestsSize { get; set; }
        public int LastAllRequestsSize { get; set; }
        public int AverageAllRequestsLatency { get; set; }
        public int LastAllRequestsLatency { get; set; }
        public int AverageRequestLatency { get; set; }
        public int LastRequestLatency { get; set; }
        public DateTime ExpectedUpdate { get; set; }
        public Int32 TotalRequestsCount { get; set; }
        public Int32 TotalBatchRequestsCount { get; set; }
        private DateTime? _queueMsgEntryTimestamp = null;
        //This Property repersents  timestamp when message insert into update triggering queue
        public DateTime? QueueMsgEntryTimestamp
        {
            get { return this._queueMsgEntryTimestamp.HasValue ? this._queueMsgEntryTimestamp : (DateTime?)null; }
            set { this._queueMsgEntryTimestamp = value; }
        }
        private DateTime? _queueTriggerTimestamp = null;
        //This Property repersents  timestamp when message trigger by update triggering queue
        public DateTime? QueueTriggerTimestamp
        {
            get { return this._queueTriggerTimestamp.HasValue ? this._queueTriggerTimestamp : (DateTime?)null; }
            set { this._queueTriggerTimestamp = value; }
        }
        private DateTime? _backendInvokeTimestamp = null;
        //This Property repersents  timestamp when backend invoked by update triggering queue with message
        public DateTime? BackendInvokeTimestamp
        {
            get { return this._backendInvokeTimestamp.HasValue ? this._backendInvokeTimestamp : (DateTime?)null; }
            set { this._backendInvokeTimestamp = value; }
        }
        private DateTime? _requestUpdateMsgEntryTimestamp = null;
        //This Property repersents  timestamp when backend response inserte into request update queue
        public DateTime? RequestUpdateMsgEntryTimestamp
        {
            get { return this._requestUpdateMsgEntryTimestamp.HasValue ? this._requestUpdateMsgEntryTimestamp : (DateTime?)null; }
            set { this._requestUpdateMsgEntryTimestamp = value; }
        }
        private DateTime? _requestUpdateMsgTriggerTimestamp = null;
        //This Property repersents  timestamp when request update message triggered by request update module
        public DateTime? RequestUpdateMsgTriggerTimestamp
        {
            get { return this._requestUpdateMsgTriggerTimestamp.HasValue ? this._requestUpdateMsgTriggerTimestamp : (DateTime?)null; }
            set { this._requestUpdateMsgTriggerTimestamp = value; }
        }
        private DateTime? _responseInsertIntostorageTimestamp = null;
        //This Property repersents  timestamp when response inser into table storage
        public DateTime? ResponseInsertIntostorageTimestamp
        {
            get { return this._responseInsertIntostorageTimestamp.HasValue ? this._responseInsertIntostorageTimestamp : (DateTime?)null; }
            set { this._responseInsertIntostorageTimestamp = value; }
        }
    }
}
