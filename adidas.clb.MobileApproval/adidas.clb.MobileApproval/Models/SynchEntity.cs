//-----------------------------------------------------------
// <copyright file="SynchEntity.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.Storage.Table;

namespace adidas.clb.MobileApproval.Models
{
    /// <summary>
    /// class which implements model for synch entity.
    /// </summary>
    public class SynchEntity:TableEntity
    {
        public SynchEntity(string partitionkey, string rowkey)
        {
            this.PartitionKey = partitionkey;
            this.RowKey = rowkey;
        }
        public int avgSynchFreq { get; set; }
        public int lastSynchFreq { get; set; }
        public int bestSynchFreq { get; set; }
        public int retryAfter { get; set; }
        public int totalReqCount { get; set; }
        public int urgentReqCount { get; set; }
        public SynchEntity() { }
    }
}