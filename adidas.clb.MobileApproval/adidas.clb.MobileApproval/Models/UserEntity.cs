using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
namespace adidas.clb.MobileApproval.Models
{
    public class UserEntity : TableEntity
    {
        public UserEntity(string partitionkey, string UserName)
        {
            this.PartitionKey = partitionkey;
            this.RowKey = UserName;
        }
        public UserEntity() { }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Fullname { get; set; }
        public string Email { get; set; }
        public string Domain { get; set; }

    }
}