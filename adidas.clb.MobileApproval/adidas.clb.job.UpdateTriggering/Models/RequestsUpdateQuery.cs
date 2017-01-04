//-----------------------------------------------------------
// <copyright file="RequestsUpdateQuery.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adidas.clb.job.UpdateTriggering.Models
{
    /// <summary>
    /// class which implements model for RequestsUpdateQuery  obj.
    /// </summary>
    public class RequestsUpdateQuery
    {
        public string _type { get; set; }
        public BackendUser User { get; set; }
        public IEnumerable<RequestUpdateMsg> Requests { get; set; }
        public bool VIP { get; set; }
        public bool GetPDFs { get; set; }
        public Nullable<DateTime> ChangeAfter { get; set; }
        public string BackendID { get; set; }
        public RequestsUpdateQuery()
        {
            _type = "requestsUpdateQuery";
        }
    }
    public class BackendUser
    {
        public string _type { get; set; }
        public string UserID { get; set; }
        public string UserName { get; set; }
        public BackendUser()
        {
            _type = "backendUser";
        }
    }
    public class RequestPdf
    {
        public string _type { get; set; }
        public string RequestID { get; set; }
        public string BackendID { get; set; }
        public RequestPdf()
        {
            _type = "requestPdf";
        }
    }
}
