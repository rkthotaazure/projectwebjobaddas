//-----------------------------------------------------------
// <copyright file="RequestDTO.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace adidas.clb.MobileApprovalUI.Models
{
    /// <summary>
    /// class which implements model for Request data transfer object.
    /// </summary>    
    public class RequestDTO
    {
        public string ID { get; set; }
        public string Title { get; set; }
        public DateTime Created { get; set; }
        public string Status { get; set; }
        public int Latency { get; set; }
        public RequesterDTO Requester { get; set; }
        public List<FieldDTO> Fields { get; set; }
        public List<Approvers> Approvers { get; set; }
    }
    
    public class RequesterDTO
    {
        public string UserID { get; set; }
        public string Name { get; set; }
    }

    public class Fields
    {
        public List<FieldDTO> Overview { get; set; }
        public List<FieldDTO> GenericInfo { get; set; }
    }
    
    public class Approvers
    {
        public int Order { get; set; }
        public string UserID { get; set; }
        public string UserName { get; set; }
        public DateTime Created { get; set; }
        public string Status { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime DecisionDate { get; set; }
    }
   
    public class FieldDTO
    {
        public FieldDTO() { }
        public string Name { get; set; }
        public string Value { get; set; }
        public string Group { get; set; }
    }
}
