//-----------------------------------------------------------
// <copyright file="BackendEntity.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace adidas.clb.job.UpdateTriggering.Models
{
    /// <summary>
    /// class which implements model for backendentity.
    /// </summary>
    public class BackendEntity : TableEntity
    {
        public BackendEntity(string strPartitionkey, string strRowkey)
        {
            this.PartitionKey = strPartitionkey;
            this.RowKey = strRowkey;
        }
        public BackendEntity() { }

        public string BackendID { get; set; }
        public string BackendName { get; set; }
        public int AgentPullingFrequency { get; set; }
        public int DefaultUpdateFrequency { get; set; }
        public int AverageRequestSize { get; set; }
        public int LastRequestSize { get; set; }
        public int AverageAllRequestsLatency { get; set; }
        public int LastAllRequestsLatency { get; set; }
        public int AverageRequestLatency { get; set; }
        public int LastRequestLatency { get; set; }
        public int MissingConfirmationsLimit { get; set; }
        public Int32 TotalRequestsCount { get; set; }
        public Int32 TotalBatchRequestsCount { get; set; }

    }
}