//-----------------------------------------------------------
// <copyright file="BackendRequest.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace adidas.clb.job.RequestsUpdate.Models
{
    /// <summary>
    /// class which implements model for Request update data object.
    /// </summary>
    public class RequestsUpdateData
    {
        public string _type { get; set; }
        public string UserId { get; set; }
        public string BackendID { get; set; }
        public List<BackendRequest> requests { get; set; }
        public RequestsUpdateData() { _type = "requestsUpdateData"; }
    }
    /// <summary>
    /// class which implements model for personalizationapi requset.
    /// </summary>
    [Serializable]
    public class BackendRequest
    {
        public string _type { get; set; }
        public string serviceLayerReqID { get; set; }
        public Request requset { get; set; }        
        public BackendRequest() { _type = "backendRequest"; }
    }
}