//-----------------------------------------------------------
// <copyright file="RequestPDF.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adidas.clb.job.RequestsUpdate.Models
{
    /// <summary>
    /// class which implements model for Request pdf obj.
    /// </summary>
    public class RequestPDF
    {
        public string _type { get; set; }
        public string PDFUri { get; set; }
        public string UserId { get; set; }
        public string RequestID { get; set; }
        public RequestPDF() { _type = "requestPDFAddress"; }
    }
}
