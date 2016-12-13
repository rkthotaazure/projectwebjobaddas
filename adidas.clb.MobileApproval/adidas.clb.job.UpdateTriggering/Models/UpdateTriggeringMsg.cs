//-----------------------------------------------------------
// <copyright file="UpdateTriggeringMsg.cs" company="adidas AG">
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
    /// class which implements model for UpdateTriggeringMsg  obj.
    /// </summary>
    class UpdateTriggeringMsg
    {
        public string _type { get; set; }
        public IEnumerable<UserUpdateMsg> Users { get; set; }
        public IEnumerable<RequestUpdateMsg> Requests { get; set; }
        public bool VIP { get; set; }
        public bool GetPDFs { get; set; }       
        public Nullable<DateTime> ChangeAfter { get; set; }
        public UpdateTriggeringMsg()
        {
            _type = "updateTriggeringMsg";
        }

    }
    [Serializable]
    class UserUpdateMsg
    {
        public string _type { get; set; }
        public string UserID { get; set; }
        public string UserName { get; set; }
        public IEnumerable<Backend> Backends{get;set;}
        public UserUpdateMsg()
        {
            _type = "userUpdateMsg";
        }
    }
    [Serializable]
    class RequestUpdateMsg
    {
        public string _type { get; set; }
        public string ServiceLayerReqID { get; set; }
        public Request request { get; set; }
        public RequestUpdateMsg()
        {
            _type = "requestUpdateMsg";
        }
    }
    [Serializable]
    class Backend
    {
        public string _type { get; set; }
        public string ID { get; set; }
        public string Name { get; set; }
        public Backend()
        {
            _type = "backend";
        }
    }
    [Serializable]
    class Request
    {
        public string _type { get; set; }
        public Backend Backend { get; set; }
        public string ID { get; set; }
        public string Title { get; set; }
        public Request()
        {
            _type = "request";
        }
    }
}
