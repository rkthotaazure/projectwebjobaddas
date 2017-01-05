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
        public string ID { get; set; }
        public string Title { get; set; }
        public string UserID { get; set; }
        public DateTime Created { get; set; }
        public string Status { get; set; }
        public int Latency { get; set; }
        public Requester Requester { get; set; }
        public Fields Fields { get; set; }
        public List<Approvers> Approvers { get; set; }

    }

    [Serializable]
    public class Requester
    {
        public string UserID { get; set; }
        public string Name { get; set; }

    }

    [Serializable]
    public class Fields
    {
        public List<Field> Overview { get; set; }
        public List<Field> GenericInfo { get; set; }

    }

    [Serializable]
    public class Field
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Group { get; set; }

    }

    [Serializable]
    public class Approvers
    {
        public int Order { get; set; }
        public BackendUser User { get; set; }
        public Nullable<DateTime> Created { get; set; }
        public string Status { get; set; }
        public Nullable<DateTime> DueDate { get; set; }
        public Nullable<DateTime> DecisionDate { get; set; }

    }

    [Serializable]
    public class BackendUser
    {
        public string UserID { get; set; }
        public string UserName { get; set; }

    }
}