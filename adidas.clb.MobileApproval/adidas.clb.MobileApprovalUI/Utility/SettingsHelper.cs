//-----------------------------------------------------------
// <copyright file="SettingsHelper.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------

using System;
using System.Configuration;

using SPO = Microsoft.SharePoint.Client;

namespace adidas.clb.MobileApprovalUI.Utility
{
    /// <summary>
    /// The class which contain references to all the constants used across the application.
    /// </summary>
    public class SettingsHelper
    {
      
        //O365 user id for authentication
        public static string userID
        {
            get { return ConfigurationManager.AppSettings["Mobileapprovalaccount"]; }
        }
        //O365 password for authentication
        public static string password
        {
            get { return ConfigurationManager.AppSettings["Mobileapprovalpassword"]; }
        }
        //O365 tenant url
        public static string TenantUrl
        {
            get { return ConfigurationManager.AppSettings["TenantURL"]; }
        }
        //SP Online credentials for authentication
        private static SPO.SharePointOnlineCredentials _spoCredentials;
        public static SPO.SharePointOnlineCredentials SpoCredentials
        {
            get
            {
                if (_spoCredentials != null)
                {
                    return _spoCredentials;
                }
                var securePassword = new System.Security.SecureString();
                foreach (var c in password)
                {
                    securePassword.AppendChar(c);
                }
                _spoCredentials = new SPO.SharePointOnlineCredentials(userID, securePassword);
                return _spoCredentials;
            }
        }
      
        //Graph resource id for authentication
        public static string ResourceID
        {
            get { return ConfigurationManager.AppSettings["ResourceID"]; }
        }
        //Tenant id url for authentication
        public static string TenantIdUrl
        {
            get { return ConfigurationManager.AppSettings["TenantIdUrl"]; }
        }
        //User object id url for authentication
        public static string UserObjectIdUrl
        {
            get { return ConfigurationManager.AppSettings["UserObjectIdUrl"]; }
        }
        //IDA client id for authentication
        public static string IDAClientId
        {
            get { return ConfigurationManager.AppSettings["ida:ClientId"]; }
        }
        //IDA client secret for authentication
        public static string IDAClientSecret
        {
            get { return ConfigurationManager.AppSettings["ida:ClientSecret"]; }
        }
        //AAD instance for authentication
        public static string AADInstance
        {
            get { return ConfigurationManager.AppSettings["ida:AADInstance"]; }
        }
        //WebAPI url for all API's
        public static string WebApiUrl
        {
            get { return ConfigurationManager.AppSettings["APIUrl"]; }
        }

        //PersonalizationAPI for all Backends Constant
        public static string PersonalizationAPIBackend
        {
            get { return ConfigurationManager.AppSettings["PersonalizationAPIBackend"]; }
        }

        //PersonalizationAPI for get userinfo Constant
        public static string PersonalizationAPIUser
        {
            get { return ConfigurationManager.AppSettings["PersonalizationAPIUser"]; }
        }

        //PersonalizationAPI for single user devices constant
        public static string PersonalizationAPIUserDevice
        {
            get { return ConfigurationManager.AppSettings["PersonalizationAPIUserDevice"]; }
        }

        //PersonalizationAPI for user deviced id Constant
        public static string PersonalizationAPIUserDeviceID
        {
            get { return ConfigurationManager.AppSettings["PersonalizationAPIUserDeviceID"]; }
        }

        //PersonalizationAPI for user specific Backends Constant
        public static string PersonalizationAPIUserBackend
        {
            get { return ConfigurationManager.AppSettings["PersonalizationAPIUserBackend"]; }
        }

        //PersonalizationAPI for user specific Backends id Constant
        public static string PersonalizationAPIBackendId
        {
            get { return ConfigurationManager.AppSettings["PersonalizationAPIBackendId"]; }
        }
        public static string UserId
        {
            get { return ConfigurationManager.AppSettings["UserId"]; }
        }
        public static string SavedataMsg
        {
            get { return ConfigurationManager.AppSettings["SavedataMsg"]; }
        }
        public static string ErrorSavedataMsg
        {
            get { return ConfigurationManager.AppSettings["ErrorSavedataMsg"]; }
        }
        // SyncAPI to get backends associated to user, each one indicating the count of current open requests
        public static string SyncAPIUserBackend
        {
            get { return ConfigurationManager.AppSettings["SyncAPIUserBackend"]; }
        }
        public static string SyncAPIUserBackendReq
        {
            get { return ConfigurationManager.AppSettings["SyncAPIUserBackendReq"]; }
        }
        public static string SyncAPIUserBackendReqID
        {
            get { return ConfigurationManager.AppSettings["SyncAPIUserBackendReqID"]; }
        }
        public static string SyncAPIUApprovers
        {
            get { return ConfigurationManager.AppSettings["SyncAPIUApprovers"]; }
        }
        public static string SyncAPIPDF
        {
            get { return ConfigurationManager.AppSettings["SyncAPIPDF"]; }
        }        
        public static string ApprovalAPIReqID
        {
            get { return ConfigurationManager.AppSettings["ApprovalAPIReqID"]; }
        }
        public static string SyncAPIUserBackendReqCompleted
        {
            get { return ConfigurationManager.AppSettings["SyncAPIUserBackendReqCompleted"]; }
        }
        public static string SyncAPIUserBackendTasksCount
        {
            get { return ConfigurationManager.AppSettings["SyncAPIUserBackendTasksCount"]; }
        }
        public static string SyncAPIUserNewRequestsCount
        {
            get { return ConfigurationManager.AppSettings["SyncAPIUserNewTasksCount"]; }
        }

    }
}