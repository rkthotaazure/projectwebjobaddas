//-----------------------------------------------------------
// <copyright file="DeviceEntity.cs" company="adidas AG">
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
    /// class which implements model for deviceentity.
    /// </summary>
    public class DeviceEntity: TableEntity
    {
        public DeviceEntity(string partitionkey ,string DeviceID)
        {
            this.PartitionKey = partitionkey;
            this.RowKey = DeviceID;
        }

        public DeviceEntity() { }
        public string DeviceOS { get; set; }
    }
}