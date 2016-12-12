//-----------------------------------------------------------
// <copyright file="IAppInsight.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adidas.clb.job.RequestsUpdate.Utility
{
    interface IAppInsight
    {
        void TrackEvent(string message);
        void TrackMetric(string message, long duration);
        void Exception(string message, Exception exception, string EventID);
        void TrackStartEvent(string methodname);
        void TrackEndEvent(string methodname);
    }

}
