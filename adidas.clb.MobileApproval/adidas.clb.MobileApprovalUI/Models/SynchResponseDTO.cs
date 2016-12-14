//-----------------------------------------------------------
// <copyright file="SynchResponseDTO.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace adidas.clb.MobileApprovalUI.Models
{
    /// <summary>
    /// class which implements model for synchapi responce with array of result items.
    /// </summary>
    public class SynchResponseListDTO<T>
    {
        public string _type { get; set; }
        public SynchRequestDTO query { get; set; }
        public IEnumerable<T> result { get; set; }
        public ErrorDTO error { get; set; }
        public SynchResponseListDTO()
        {
            _type = "synchResponse";
        }
    }

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
    public class ErrorDTO
    {
        public string _type { get; set; }
        public string code { get; set; }
        public string shorttext { get; set; }
        public string longtext { get; set; }
        public ErrorDTO() { _type = "error"; }
        public ErrorDTO(string code, string shorttext, string longtext)
        {
            this.code = code;
            this.shorttext = shorttext;
            this.longtext = longtext;
            this._type = "error";
        }
    }
}