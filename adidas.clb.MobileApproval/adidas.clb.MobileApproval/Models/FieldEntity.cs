//-----------------------------------------------------------
// <copyright file="FieldEntity.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
namespace adidas.clb.MobileApproval.Models
{
    /// <summary>
    /// class which implements model for field entity.
    /// </summary>
    public class FieldEntity : TableEntity
    {
        public FieldEntity(string RequestRowKey, string guid)
        {
            this.PartitionKey = RequestRowKey;
            this.RowKey = guid;
        }
        public FieldEntity() { }

        public string name { get; set; }
        public string value { get; set; }
        public string group { get; set; }
    }

    /// <summary>
    /// class which implements model for field data transfer object.
    /// </summary>
    public class FieldDTO 
    {        
        public FieldDTO() { }
        public string name { get; set; }
        public string value { get; set; }
        public string group { get; set; }
    }
}