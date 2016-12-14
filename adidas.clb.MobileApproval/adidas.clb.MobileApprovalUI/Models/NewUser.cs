//-----------------------------------------------------------
// <copyright file="NewUser.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using adidas.clb.MobileApprovalUI.Models.JSONHelper;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace adidas.clb.MobileApprovalUI.Models
{
    public class NewUser
    {
        public string UserID { get; set; }
        public string _type { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Fullname { get; set; }
        public string Email { get; set; }
        public string Domain { get; set; }
        public NewUser() { _type = "user"; }
        public IEnumerable<Backend> userbackends { get; set; }
        public IEnumerable<Device> userdevices { get; set; }

    }
}