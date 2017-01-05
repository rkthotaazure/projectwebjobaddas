//-----------------------------------------------------------
// <copyright file="FieldEntity.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace adidas.clb.job.RequestsUpdate.Models
{
    /// <summary>
    /// class which implements model for Field object.
    /// </summary>
    public class FieldEntity : TableEntity
    {
        public FieldEntity(string type, string requsetID)
        {
            this.PartitionKey = type;
            this.RowKey = requsetID;
        }
        public FieldEntity() { }
        public string Name { get; set; }
        public string Value { get; set; }
        public string Group { get; set; }

    }
}
