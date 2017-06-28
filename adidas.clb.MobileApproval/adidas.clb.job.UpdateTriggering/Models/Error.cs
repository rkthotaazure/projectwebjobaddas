﻿//-----------------------------------------------------------
// <copyright file="CarApprovalModel.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adidas.clb.job.UpdateTriggering.Models
{
    /// <summary>
    /// This class defines  model for Error details obj.
    /// </summary>
    public class Error
    {
        public string _type { get; set; }
        public string code { get; set; }
        public string shorttext { get; set; }
        public string longtext { get; set; }
        public Error() { _type = "error"; }
        public Error(string code, string shorttext, string longtext)
        {
            this.code = code;
            this.shorttext = shorttext;
            this.longtext = longtext;
            this._type = "error";
        }
    }
}
