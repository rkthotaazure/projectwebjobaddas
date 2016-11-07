//-----------------------------------------------------------
// <copyright file="ErrorDTO.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace adidas.clb.MobileApproval.Models
{
    /// <summary>
    /// class which implements model for error in personalizationapi responce.
    /// </summary>
    public class ErrorDTO
    {
        public string _type { get; set; }
        public string code { get; set; }
        public string shorttext { get; set; }
        public string longtext { get; set; }
        public ErrorDTO() { _type = "error"; }
        public ErrorDTO(string code, string shorttext, string longtext) {
            this.code = code;
            this.shorttext = shorttext;
            this.longtext = longtext;
            this._type = "error";
        }
    }
}