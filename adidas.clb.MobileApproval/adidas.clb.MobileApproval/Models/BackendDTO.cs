//-----------------------------------------------------------
// <copyright file="BackendDTO.cs" company="adidas AG">
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
    /// class which implements model for backend data transfer object.
    /// </summary>
    public class BackendDTO
    {        
        public string _type { get; set; }
        public string BackendID { get; set; }
        public int AgentPullingFrequency { get; set; }
        public int DefaultUpdateFrequency { get; set; }
        public int AverageRequestSize { get; set; }
        public int LastRequestSize { get; set; }
        public int AverageAllRequestsLatency { get; set; }
        public int LastAllRequestsLatency { get; set; }
        public int AverageRequestLatency { get; set; }
        public int LastRequestLatency { get; set; }
        public int MissingConfirmationsLimit { get; set; }
        public BackendDTO() { _type = "backend"; }
    }
}