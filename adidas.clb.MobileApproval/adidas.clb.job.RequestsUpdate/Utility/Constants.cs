//-----------------------------------------------------------
// <copyright file="Constants.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using System.Configuration;

namespace adidas.clb.job.RequestsUpdate.Utility
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
        // Azure queue Constants
        public struct AzureQueues
        {
            public const string RequsetUpdateQueue = "requestupdateinputqueue";
            public const string RequsetPDFQueue = "requestpdf";
            public const string Trace = "Trace";
            public const string Error = "Error";
        }
        //Azure Table constants
        public struct AzureTables
        {
            public const string RequestsPK = "Requests_";
            public const string ApprovalPK = "Approval_";
            public const string ApproverPK = "Approver_";
            public const string FieldPK = "Field_";
            public const string RequestTransactions = "RequestTransactions";
            public const string AzureStorageConnectionString = "AzureStorageConnectionString";
            public const string UnderScore = "_";
            public const string UserBackendPK = "UB_";
            public const string BackendPK = "Backend";
            public const string ReferenceData = "ReferenceData";
            public const string UserDeviceConfiguration = "UserDeviceConfiguration";
            public const string PartitionKey = "PartitionKey";
            public const string BackendId = "BackendID";
            public const string Status = "status";
            public const string DueDate = "dueDate";            
            public const string InProgress = "In-Progress";
            public const string Waiting = "Waiting";
        }

        // Azure blob Constants
        public struct AzureBlob
        {
            public const string BlobRequsetPDF = "requestpdf";
            public const string PDFContentType = "application/pdf";
            public const string AzureTempBlobStorageAccountName = "AzureTempBlobStorageAccountName";
            public const string AzureTempBlobStorageAccountKey = "AzureTempBlobStorageAccountKey";
            public const string RequestPDF = "RequestPDF";
            public const string RequestPDFExtension = ".pdf";
        }
    }
}
