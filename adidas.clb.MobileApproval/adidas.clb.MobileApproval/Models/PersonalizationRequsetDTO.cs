//-----------------------------------------------------------
// <copyright file="PersonalizationRequsetDTO.cs" company="adidas AG">
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
    /// class which implements model for personalizationapi requset.
    /// </summary>
    public class PersonalizationRequsetDTO
    {
        public string _type { get; set; }
        public UserDTO user { get; set; }
        public IEnumerable<UserDeviceDTO> userdevices { get; set; }
        public IEnumerable<UserBackendDTO> userbackends { get; set; }
        public PersonalizationRequsetDTO() { _type = "personalizationQuery"; }
    }
}