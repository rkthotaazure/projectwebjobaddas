//-----------------------------------------------------------
// <copyright file="RequestSynchEntity.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.Storage.Table;

namespace adidas.clb.MobileApproval.Models
{
    /// <summary>
    /// class which implements model for request synch entity.
    /// </summary>
    public class RequestSynchEntity : TableEntity
    {
        public RequestSynchEntity(string partitionkey, string rowkey)
        {
            this.PartitionKey = partitionkey;
            this.RowKey = rowkey;
        }
        public string BackendID { get; set; }
        public string UserID { get; set; }
        public Boolean Updatetriggered { get; set; }
        public int Expectedlatency { get; set; }
        public Boolean Lastupdate { get; set; }
        private DateTime? lastchange = null;

        public DateTime? LastChange
        {
            get
            {
                return this.lastchange.HasValue ? this.lastchange.Value : (DateTime?)null;
            }

            set { this.lastchange = value; }
        }

        private DateTime? expectedupdate = null;

        public DateTime? ExpectedUpdate
        {
            get
            {
                return this.expectedupdate.HasValue ? this.expectedupdate.Value : (DateTime?)null;
            }

            set { this.lastchange = value; }
        }        
        public RequestSynchEntity() { }
    }
}