//-----------------------------------------------------------
// <copyright file="Rules.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using adidas.clb.MobileApproval.Models;

namespace adidas.clb.MobileApproval.Utility
{
    public static class Rules
    {
        /// <summary>
        /// The class which implements methods for Rules.
        /// </summary>
        public static int Rule1(IEnumerable<UserBackendEntity> userbackends)
        {
            int MaxLatency=userbackends.Max(m => Math.Max(m.AverageAllRequestsLatency, m.LastAllRequestsLatency));            
            return Convert.ToInt32(MaxLatency*1.2);
        }
    }
}