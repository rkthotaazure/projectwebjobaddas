//-----------------------------------------------------------
// <copyright file="RequestEntity.cs" company="adidas AG">
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
    /// class which implements model for Request entity.
    /// </summary>
    public class RequsetEntity : TableEntity
    {
        public RequsetEntity(string type, string requsetID)
        {
            this.PartitionKey = type;
            this.RowKey = requsetID;
        }
        public RequsetEntity() { }
        public string BackendID { get; set; }
        public string ServiceLayerReqID { get; set; }
        public string ID { get; set; }
        public string Title { get; set; }
        public DateTime Created { get; set; }
        public string Status { get; set; }
        public int Latency { get; set; }
        public string RequesterID { get; set; }
        public string RequesterName { get; set; }
        public int Agentpullingfrequency { get; set; }
        public int Defaultupdatefrequency { get; set; }
        public int Averagerequestsize { get; set; }
        public int Lastrequestsize { get; set; }
        public int Averageallrequestslatency { get; set; }
        public int Lastallrequestslatency { get; set; }
        public int Averagerequestlatency { get; set; }
        public int Lastrequestlatency { get; set; }
        public int Missingconfirmationslimit { get; set; }
        private DateTime? lastUpdate = null;
        public DateTime LastUpdate
        {
            get
            {
                return this.lastUpdate.HasValue ? this.lastUpdate.Value : DateTime.Now;
            }

            set { this.lastUpdate = value; }
        }
        public bool UpdateTriggered { get; set; }
        public string PDFUri { get; set; }
        private DateTime? expectedupdate = null;
        public DateTime? ExpectedUpdate
        {
            get
            {
                return this.expectedupdate.HasValue ? this.expectedupdate.Value : (DateTime?)null;
            }

            set { this.expectedupdate = value; }
        }
        private DateTime? _queueMsgEntryTimestamp = null;
        //This Property repersents  timestamp when message insert into update triggering queue
        public DateTime? Request_QueueMsgEntryTimestamp
        {
            get { return this._queueMsgEntryTimestamp.HasValue ? this._queueMsgEntryTimestamp : (DateTime?)null; }
            set { this._queueMsgEntryTimestamp = value; }
        }
        private DateTime? _queueTriggerTimestamp = null;
        //This Property repersents  timestamp when message trigger by update triggering queue
        public DateTime? Request_QueueMsgTriggerTimestamp
        {
            get { return this._queueTriggerTimestamp.HasValue ? this._queueTriggerTimestamp : (DateTime?)null; }
            set { this._queueTriggerTimestamp = value; }
        }
        private DateTime? _backendInvokeTimestamp = null;
        //This Property repersents  timestamp when backend invoked by update triggering queue with message
        public DateTime? Request_BackendInvokeTimestamp
        {
            get { return this._backendInvokeTimestamp.HasValue ? this._backendInvokeTimestamp : (DateTime?)null; }
            set { this._backendInvokeTimestamp = value; }
        }
        private DateTime? _requestUpdateMsgEntryTimestamp = null;
        //This Property repersents  timestamp when backend response inserte into request update queue
        public DateTime? Request_ReqUpdateQueueMsgEntryTimestamp
        {
            get { return this._requestUpdateMsgEntryTimestamp.HasValue ? this._requestUpdateMsgEntryTimestamp : (DateTime?)null; }
            set { this._requestUpdateMsgEntryTimestamp = value; }
        }
        private DateTime? _requestUpdateMsgTriggerTimestamp = null;
        //This Property repersents  timestamp when request update message triggered by request update module
        public DateTime? Request_ReqUpdateQueueMsgTriggerTimestamp
        {
            get { return this._requestUpdateMsgTriggerTimestamp.HasValue ? this._requestUpdateMsgTriggerTimestamp : (DateTime?)null; }
            set { this._requestUpdateMsgTriggerTimestamp = value; }
        }
        private DateTime? _responseInsertIntostorageTimestamp = null;
        //This Property repersents  timestamp when response inser into table storage
        public DateTime? Request_ResponseInsertIntostorageTimestamp
        {
            get { return this._responseInsertIntostorageTimestamp.HasValue ? this._responseInsertIntostorageTimestamp : (DateTime?)null; }
            set { this._responseInsertIntostorageTimestamp = value; }
        }
    }
}
