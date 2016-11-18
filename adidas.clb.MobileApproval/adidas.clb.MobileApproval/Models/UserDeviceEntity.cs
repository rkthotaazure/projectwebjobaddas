//-----------------------------------------------------------
// <copyright file="UserDeviceEntity.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace adidas.clb.MobileApproval.Models
{
    /// <summary>
    /// class which implements model for userdeviceentity.
    /// </summary>
    public class UserDeviceEntity: TableEntity
    {        
        public UserDeviceEntity(string UserRowKey,string DeviceRowKey)
        {
            this.PartitionKey = UserRowKey;
            this.RowKey = DeviceRowKey;
        }
        public UserDeviceEntity() { }
        public string UserID { get; set; }
        public string DeviceID { get; set; }
        public string DeviceName { get; set; }
        public string DeviceBrand { get; set; }
        public string DeviceModel { get; set; }
        public int maxSynchReplySize { get; set; }
    }
}