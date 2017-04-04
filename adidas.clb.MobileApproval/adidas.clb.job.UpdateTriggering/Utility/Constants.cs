//-----------------------------------------------------------
// <copyright file="Constants.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using System;
using System.Configuration;

namespace adidas.clb.job.UpdateTriggering.Utility
{
    /// The class that contains the constants used in the application.
    public class CoreConstants
    {
        // Logging Priority Constants

        public struct Priority
        {
            public const string Lowest = "Lowest";
            public const string Low = "Low";
            public const string Normal = "Normal";
            public const string High = "High";
            public const string Highest = "Highest";
        }
        // Logging Category Constants
        public struct Category
        {
            public const string General = "General";
            public const string Trace = "Trace";
            public const string Error = "Error";
            public const string User = "User";
            public const string Request = "Request";
        }

        #region "Exception and Error Constants"
        public const string INVALIDPARAMETER_MSG = "Parameters are invalid for this operation Parameters:{0}";
        //Exception message when the usr control fails to load.
        public const string USERCONTROL_MSG = "Unable to Load Control";
        public const string WEBPART_MSG = "Error in Web part";
        public const string NULLOBJECT_MSG = "Object is null Object Name:{0} Value:{1}";
        public const string WEBSERVICEERROR_MSG = "Error in Web Service";
        public const string APPLICATIONLOGIC_MSG = "Error in Application Logic";
        public const string APPLICATIONDATA_MSG = "Error in Application Data";
        public const string UTILITIES_MSG = "Error in Utilities";

        public const string EXCEPTION_POLICY_BUSINESSLOGIC = "BusinessLogicPolicy";
        public const string EXCEPTION_POLICY_DATAACCESS = "DataAccessPolicy";
        public const string EXCEPTION_POLICY_UTILITY = "UtilityPolicy";
        public const string EXCEPTION_POLICY_USERINTERFACE = "UserInterfacePolicy";
        public const string EXCEPTION_POLICY_PASSTHROUGH = "PassThroughPolicy";
        #endregion

        #region
        //Backends
        public struct Backends
        {
            public const string CAR = "car";
            public const string BPMOnline = "bpmonline";
        }
        #endregion
        //Azure Table constants
        public struct AzureTables
        {
            public const string ReferenceData = "ReferenceData";
            public const string UserDeviceConfiguration = "UserDeviceConfiguration";
            public const string AzureStorageConnectionString = "GenericMobileStorageConnectionString";
            public const string PartitionKey = "PartitionKey";
            public const string Timestamp = "Timestamp";
            public const string RowKey = "RowKey";
            public const string Backend = "Backend";
            public const string User = "User";
            public const string UserDevicePK = "UD_";
            public const string UserBackendPK = "UB_";
            public const string BackendSynchPK = "BS_";
            public const string RequestSynchPK = "RequestSynch";
            public const string RequestPK = "Requests_";
            public const string ApprovalPK = "Approval_";
            public const string FieldPK = "Field_";
            public const string ApproverPK = "Approver_";            
            public const string UpdateTriggerNextCollectingTime = "B_NextCollectingTime";
            public const string BackendID = "BackendID";
            public const string Status = "Status";
            public const string RequestID = "RequestId";
        }
        public struct AzureQueues
        {
            public const string UpdateTriggerInputQueue = "updatetriggerinputqueue";
            public const string GeneratePdfQueue = "generatepdfqueue";
            public const string UTQueue = "utQueue";
            public const string UTMissedUpdatesQueue = "utMissingUpdatesQueue";
            public const string VIPQueue = "vipQueue";
            public const string PDFQueue = "pdfQueue";
        }
        public struct AzureWebJobTimers
        {
            public const string RegularUpdateTimer = "00:01:00";
            public const string MissedUpdateTimer = "00:01:00";
            public const string UpdateNextCollectingTimer = "01:00:00";

        }
        public struct TimeIntervals
        {
            public const int TotalHours = 23;
            public const int TotalMinutes = 59;
            public const int singledigit = 9;
            public const string TotalSeconds = "00";
        }


        //// Get the root video portal url
        //public static string RootSiteUrl
        //{
        //    get { return ConfigurationManager.AppSettings["RootSiteUrl"]; }
        //}
    }
}
