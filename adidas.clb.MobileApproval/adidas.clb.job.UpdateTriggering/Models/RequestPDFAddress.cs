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

namespace adidas.clb.job.UpdateTriggering.Models
{
    /// <summary>
    ///Thic  class defines model for Request pdf obj.
    /// </summary>
    public class RequestPDFAddress
    {
        public string _type { get; set; }
        public string PDF_URL { get; set; }
        public string RequestID { get; set; }
        public RequestPDFAddress() { _type = "requestPDFAddress"; }
    }
}
