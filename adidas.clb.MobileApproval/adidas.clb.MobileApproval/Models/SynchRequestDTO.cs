//-----------------------------------------------------------
// <copyright file="SynchRequestDTO.cs" company="adidas AG">
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
    /// class which implements model for synchapi requset.
    /// </summary>
    public class SynchRequestDTO
    {
        public string _type { get; set; }
        public string userId { get; set; }
        public UserDeviceDTO client { get; set; }
        public Params parameters { get; set; }
        public SynchRequestDTO() { _type = "synchQuery"; }
    }
    /// <summary>
    /// class which implements model for synchapi requset.
    /// </summary>
    public class Params
    {
        public Boolean forceUpdate { get; set; }
        public Filters filters { get; set; }
        public Depth depth { get; set; }
    }
    /// <summary>
    /// class which implements model for synchapi requset.
    /// </summary>
    public class Filters
    {
        public List<string> backends { get; set; }
        public string reqStatus { get; set; }
        public string apprStatus { get; set; }
        public Boolean isChanged { get; set; }
        public Boolean onlyChangedReq{ get; set; }
    }
    /// <summary>
    /// class which implements model for synchapi requset.
    /// </summary>
    public class Depth
    {
        public Boolean overview { get; set; }
        public Boolean genericInfo { get; set; }
        public Boolean approvers { get; set; }
    }
}