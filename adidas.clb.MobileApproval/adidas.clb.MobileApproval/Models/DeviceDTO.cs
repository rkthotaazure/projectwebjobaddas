//-----------------------------------------------------------
// <copyright file="DeviceDTO.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace adidas.clb.MobileApproval.Models
{
    /// <summary>
    /// class which implements model for device data transfer object.
    /// </summary>
    public class DeviceDTO
    {
        public string DeviceID { get; set; }
        public string DeviceName { get; set; }
        public string DeviceBrand { get; set; }
        public string DeviceModel { get; set; }
        public int maxSynchReplySize { get; set; }
    }
}