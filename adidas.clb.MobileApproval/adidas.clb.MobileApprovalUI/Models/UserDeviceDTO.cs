//-----------------------------------------------------------
// <copyright file="UserDeviceDTO.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace adidas.clb.MobileApprovalUI.Models
{
    /// <summary>
    /// class which implements model for userdevice data transfer object.
    /// </summary>
    public class UserDeviceDTO
    {
        public string _type { get; set; }
        public string UserID { get; set; }
        public DeviceDTO device { get; set; }
        public UserDeviceDTO() { _type = "userDevice"; }
    }
    public class DeviceDTO
    {
        public string DeviceID { get; set; }
        public string DeviceName { get; set; }
        public string DeviceBrand { get; set; }
        public string DeviceModel { get; set; }
        public int maxSynchReplySize { get; set; }
    }
}