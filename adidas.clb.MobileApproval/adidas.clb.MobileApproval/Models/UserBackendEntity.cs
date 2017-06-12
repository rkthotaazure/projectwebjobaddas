//-----------------------------------------------------------
// <copyright file="UserBackendEntity.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace adidas.clb.MobileApproval.Models
{
    public class UserBackendEntity : TableEntity
    {
        /// <summary>
        /// class which implements model for userbackendentity.
        /// </summary>
        public UserBackendEntity(string UserRowKey, string BackendRowKey)
        {
            this.PartitionKey = UserRowKey;
            this.RowKey = BackendRowKey;
        }

        public UserBackendEntity() { }

        public string UserID { get; set; }
        public string BackendID { get; set; }
        public string BackendName { get; set; }
        public Int32 OpenApprovals { get; set; }
        public Int32 DefaultUpdateFrequency { get; set; }
        public bool UpdateTriggered { get; set; }
        public Int32 ExpectedLatency { get; set; }
        private DateTime? lastUpdate = null;
        public DateTime? LastUpdate
        {
            get
            {
                return this.lastUpdate.HasValue ? this.lastUpdate.Value : (DateTime?)null;
            }

            set { this.lastUpdate = value; }
        }    
        public Int32 OpenRequests { get; set; }        
        public Int32 UrgentApprovals { get; set; }
        public Int32 AverageRequestSize { get; set; }
        public Int32 LastRequestSize { get; set; }
        public Int32 AverageAllRequestsSize { get; set; }
        public Int32 LastAllRequestsSize { get; set; }
        public Int32 AverageAllRequestsLatency { get; set; }
        public Int32 LastAllRequestsLatency { get; set; }
        public Int32 AverageRequestLatency { get; set; }
        public Int32 LastRequestLatency { get; set; }
        private DateTime? expectedUpdate = null;
        public DateTime? ExpectedUpdate
        {
            get
            {
                return this.expectedUpdate.HasValue ? this.expectedUpdate.Value : (DateTime?)null;
            }

            set { this.expectedUpdate = value; }
        }
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