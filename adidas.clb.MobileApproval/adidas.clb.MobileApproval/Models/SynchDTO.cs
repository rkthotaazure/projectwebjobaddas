//-----------------------------------------------------------
// <copyright file="SynchDTO.cs" company="adidas AG">
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
    /// class which implements model for synch data transfer object.
    /// </summary>
    public class SynchDTO
    {
        public int avgSynchFreq { get; set; }
        public int lastSynchFreq { get; set; }
        public int bestSynchFreq { get; set; }
        public int retryAfter { get; set; }
        public int totalReqCount { get; set; }
        public int urgentReqCount { get; set; }
    }
}