//-----------------------------------------------------------
// <copyright file="UpdateTriggeringMessage.cs" company="adidas AG">
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
    /// class which implements model for updatetriggering queue message.
    /// </summary>
    [Serializable]
    public class UpdateTriggeringMessage
    {
        public string _type { get; set; }
        public UserUpdateMsg Users { get; set; }
        public IEnumerable<RequestUpdateMsg> Requests { get; set; }
        public bool VIP { get; set; }
        public bool GetPDFs { get; set; }
        public Nullable<DateTime> ChangeAfter { get; set; }
        public UpdateTriggeringMessage()
        {
            _type = "updateTriggeringMsg";
        }
    }

    [Serializable]
    public class UserUpdateMsg
    {
        public string _type { get; set; }
        public string UserID { get; set; }
        public string UserName { get; set; }
        public IEnumerable<UpdateTriggerBackend> Backends { get; set; }
        public UserUpdateMsg()
        {
            _type = "userUpdateMsg";
        }
    }

    [Serializable]
    public class RequestUpdateMsg
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
    public class UpdateTriggerBackend
    {
        public string _type { get; set; }
        public string BackendID { get; set; }
        public string BackendName { get; set; }
        public UpdateTriggerBackend()
        {
            _type = "backend";
        }
    }

    [Serializable]
    public class Request
    {
        public string _type { get; set; }
        public UpdateTriggerBackend Backend { get; set; }
        public string ID { get; set; }
        public string Title { get; set; }
        public Request()
        {
            _type = "request";
        }
    }

}