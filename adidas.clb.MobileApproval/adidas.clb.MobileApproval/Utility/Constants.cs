//-----------------------------------------------------------
// <copyright file="Constants.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using System.Configuration;

namespace adidas.clb.MobileApproval.Utility
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
        }
        // web.config keys
        public struct Config
        {
            public const string UpdateFrequency = "UpdateFrequency";
        }
        //Azure Table constants
        public struct AzureTables
        {
            public const string ReferenceData = "ReferenceData";
            public const string UserDeviceConfiguration = "UserDeviceConfiguration";
            public const string RequestTransactions = "RequestTransactions";
            public const string AzureStorageConnectionString = "GenericMobileStorageConnectionString";
            public const string PartitionKey = "PartitionKey";
            public const string RowKey = "RowKey";
            public const string Backend = "Backend";
            public const string User = "User";
            public const string UserDevicePK = "UD_";
            public const string UserBackendPK = "UB_";
            public const string BackendSynchPK = "BS_";
            public const string RequestSynchPK = "RequestSynch";
            public const string RequestsPK = "Requests_";
            public const string ApproverPK = "Approver_";
            public const string ApprovalPK = "Approval_";
            public const string FieldPK = "Field_";
            public const string BackendId = "BackendID";
            public const string UnderScore = "_";
            public const string Status = "Status";
            public const string InProgress = "In-Progress";
            public const string Waiting = "Waiting";

        }
        //Azure Queue constants
        public struct AzureQueues
        {
            public const string UpdateTriggerQueueName = "updatetriggerinputqueue";           

        }
        // Get the root video portal url
        public static string RootSiteUrl
        {
            get { return ConfigurationManager.AppSettings["RootSiteUrl"]; }
        }
    }
}
