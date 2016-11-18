//-----------------------------------------------------------
// <copyright file="UserEntity.cs" company="adidas AG">
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
    public class UserEntity : TableEntity
    {
        /// <summary>
        /// class which implements model for userentity.
        /// </summary>
        public UserEntity(string partitionkey, string UserID)
        {
            this.PartitionKey = partitionkey;
            this.RowKey = UserID;
        }
        public UserEntity() { }

        public string UserID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Fullname { get; set; }
        public string Email { get; set; }
        public string Domain { get; set; }
        public string DeviceName { get; set; }

    }
}