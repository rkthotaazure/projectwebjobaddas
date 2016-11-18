//-----------------------------------------------------------
// <copyright file="Request.cs" company="adidas AG">
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
    /// class which implements model for Request data transfer object.
    /// </summary>
    [Serializable]
    public class Request
    {
        public string id { get; set; }
        public string title { get; set; }
        public DateTime created { get; set; }
        public string status { get; set; }
        public int Latency { get; set; }
        public Requester  requeter { get; set; }
        public Fields fields { get; set; }
        public List<Approvers> approvers { get; set; }
    }

    [Serializable]
    public class Requester
    {
        public string userID { get; set; }
        public string name { get; set; }       
    }

    [Serializable]
    public class Fields
    {
        public List<Field> overview { get; set; }
        public List<Field> genericInfo { get; set; }
    }

    [Serializable]
    public class Field
    {
        public string name { get; set; }
        public string value { get; set; }
        public string group { get; set; }
    }

    [Serializable]
    public class Approvers
    {
        public int order { get; set; }
        public BackendUser user { get; set; }
        public DateTime created { get; set; }
        public string status { get; set; }
        public DateTime dueDate { get; set; }
        public DateTime creatdecisionDate { get; set; }
    }

    [Serializable]
    public class BackendUser
    {
        public string userID { get; set; }
        public string userName { get; set; }        
    }
}