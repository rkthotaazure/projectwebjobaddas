using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
namespace adidas.clb.MobileApproval.Models
{
    public class FieldEntity : TableEntity
    {
        public FieldEntity(String RequestRowKey, string guid)
        {
            this.PartitionKey = RequestRowKey;
            this.RowKey = guid;
        }
        public FieldEntity() { }

        public string Key { get; set; }
        public string Value { get; set; }



    }
}