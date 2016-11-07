//-----------------------------------------------------------
// <copyright file="UserBackendDTO.cs" company="adidas AG">
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
    /// class which implements model for userbackend data transfer object.
    /// </summary>
    public class UserBackendDTO
    {        
        public string _type { get; set; }
        public string UserID { get; set; }
        public BackendDTO backend { get; set; }
        public SynchDTO synch { get; set; }
        public UserBackendDTO() { _type = "userBackend"; }
    }
}