//-----------------------------------------------------------
// <copyright file="UserDTO.cs" company="adidas AG">
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
    /// class which implements model for user data transfer object.
    /// </summary>
    public class UserDTO
    {
        public string UserID { get; set; }
        public string _type { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Fullname { get; set; }
        public string Email { get; set; }
        public string Domain { get; set; }
        public string DeviceName { get; set; }
        public UserDTO() { _type = "user"; }
        public IEnumerable<UserBackendDTO>  userbackends { get; set; }
        public IEnumerable<UserDeviceDTO> userdevices { get; set; }
        public string DeviceOS { get; set; }
    }
}