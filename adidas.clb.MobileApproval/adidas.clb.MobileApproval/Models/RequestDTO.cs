//-----------------------------------------------------------
// <copyright file="RequestDTO.cs" company="adidas AG">
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
    /// class which implements model for Request data transfer object.
    /// </summary>    
    public class RequestDTO
    {
        public string id { get; set; }
        public string title { get; set; }
        public DateTime created { get; set; }
        public string status { get; set; }
        public int Latency { get; set; }
        public string PDFUri { get; set; }
        public Requester requeter { get; set; }
        public Fields fields { get; set; }
        public List<Approvers> approvers { get; set; }
    }
    
    public class Requester
    {
        public string userID { get; set; }
        public string name { get; set; }
    }

    public class Fields
    {
        public List<FieldDTO> overview { get; set; }
        public List<FieldDTO> genericInfo { get; set; }
    }
    
    public class Approvers
    {
        public int order { get; set; }
        public BackendUser user { get; set; }
        public DateTime created { get; set; }
        public string status { get; set; }
        public DateTime dueDate { get; set; }
        public DateTime creatdecisionDate { get; set; }
    }

    public class BackendUser
    {
        public string userID { get; set; }
        public string userName { get; set; }
    }
}
