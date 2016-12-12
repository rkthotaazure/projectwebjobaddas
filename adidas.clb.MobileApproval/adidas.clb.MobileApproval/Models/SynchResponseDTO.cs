//-----------------------------------------------------------
// <copyright file="SynchResponseDTO.cs" company="adidas AG">
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
    /// class which implements model for synchapi responce with single result item.
    /// </summary>
    public class SynchResponseDTO<T>
    {
        public string _type { get; set; }
        public SynchRequestDTO query { get; set; }
        public List<T> result { get; set; }
        public ErrorDTO error { get; set; }
        public SynchResponseDTO()
        {
            _type = "synchResponse";
        }
    }

    /// <summary>
    /// class which implements model for synchapi responce with single result item.
    /// </summary>
    public class SynchResponseItemDTO<T>
    {
        public string _type { get; set; }
        public SynchRequestDTO query { get; set; }
        public T result { get; set; }
        public ErrorDTO error { get; set; }
        public SynchResponseItemDTO()
        {
            _type = "synchResponse";
        }
    }
}