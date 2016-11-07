//-----------------------------------------------------------
// <copyright file="PersonalizationResponseDTO.cs" company="adidas AG">
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
    /// class which implements model for personalizationapi responce with array of result items.
    /// </summary>
    public class PersonalizationResponseListDTO<T>
    {
        public string _type { get;  set; }
        public PersonalizationRequsetDTO query { get; set; }
        public IEnumerable<T> result { get; set; }
        public ErrorDTO error { get; set; }
        public PersonalizationResponseListDTO() {
            _type = "personalizationResponse";
        }
    }

    /// <summary>
    /// class which implements model for personalizationapi responce with single result item.
    /// </summary>
    public class PersonalizationResponseDTO<T>
    {
        public string _type { get; set; }
        public PersonalizationRequsetDTO query { get; set; }
        public T result { get; set; }
        public ErrorDTO error { get; set; }
        public PersonalizationResponseDTO()
        {
            _type = "personalizationResponse";
        }
    }
}