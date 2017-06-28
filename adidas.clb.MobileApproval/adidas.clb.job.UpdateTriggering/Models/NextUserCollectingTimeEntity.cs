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
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
namespace adidas.clb.job.UpdateTriggering.Models
{
    /// <summary>
    /// This class defines model for Next Collecting Time of the user entity
    /// </summary>
    public class NextUserCollectingTimeEntity : TableEntity
    {
        public NextUserCollectingTimeEntity(string partitionkey, string rowkey)
        {
            this.PartitionKey = partitionkey;
            this.RowKey = rowkey;
        }
        public NextUserCollectingTimeEntity() { }
        public string BackendID { get; set; }
        public int MinimumUpdateFrequency { get; set; }
        public DateTime RegularUpdateLastCollectingTime { get; set; }
        public DateTime RegularUpdateNextCollectingTime { get; set; }
        public DateTime MissingUpdateLastCollectingTime { get; set; }
        public DateTime MissingUpdateNextCollectingTime { get; set; }
        public bool MissingUpdateTrigger { get; set; }

    }
}
