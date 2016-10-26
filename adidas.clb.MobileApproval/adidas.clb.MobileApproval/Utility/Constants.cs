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
        //Azure Table Names
        public struct AzureTables
        {
            public const string ReferenceData = "ReferenceData";
            public const string PartitionKey = "PartitionKey";
            public const string Backend = "Backend";
            public const string User = "User";
            public const string UserDeviceConfiguration = "UserDeviceConfiguration";
        }
        // Get the root video portal url
        public static string RootSiteUrl
        {
            get { return ConfigurationManager.AppSettings["RootSiteUrl"]; }
        }
    }
}
