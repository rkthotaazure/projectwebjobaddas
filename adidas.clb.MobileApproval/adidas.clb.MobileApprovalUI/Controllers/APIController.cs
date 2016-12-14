//-----------------------------------------------------------
// <copyright file="APIController.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using adidas.clb.MobileApprovalUI.Utility;
using adidas.clb.MobileApprovalUI.Exceptions;
using adidas.clb.MobileApprovalUI.Models;

namespace adidas.clb.MobileApprovalUI.Controllers
{

    /// <summary>
    /// The controller that will handle requests to the SharePoint API.
    /// </summary>
    //[Authorize]

    /// <summary>
    /// The controller that will handle requests to the SharePoint API.
    /// </summary>
    public class APIController : Controller
    {
        /// <summary>
        /// Gets all the backend applications in services object
        /// </summary>
        /// <returns>returns the response recived from endpoint uri</returns>
        /// <exception cref="DataAccessException">throws exception in case not able to reach API</exception>
        /// 
        string WebApiRootURL = SettingsHelper.WebApiUrl;


        public async Task<string> Getbackendapplications()
        {
            string allBackendApps = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(WebApiRootURL))
                {
                    //Creates the enpoint uri to be called
                    StringBuilder EndPointUri = new StringBuilder(WebApiRootURL);
                    Uri APIEndPointUri =
                         new Uri(EndPointUri.Append(string.Format(SettingsHelper.PersonalizationAPIBackend)).ToString());
                    Helper JsonHelperObj = new Helper();
                    //Gets the response returned by the Personalization API
                    allBackendApps = await JsonHelperObj.APIGetCall(string.Format(APIEndPointUri.ToString()));
                }
                else
                {
                    //Write the trace in db that no url exists
                    LoggerHelper.WriteToLog("WebApiRootURL URL is null", CoreConstants.Priority.High, CoreConstants.Category.Error);
                    return null;
                }
            }
            catch (Exception exception)
            {

                LoggerHelper.WriteToLog(exception + " - Error while creating client context : "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);

            }
            return allBackendApps;
        }
        public async Task<string> Getuserinfo(string userid)
        {
            string userinfo = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(WebApiRootURL))
                {
                    //Creates the enpoint uri to be called
                    StringBuilder EndPointUri = new StringBuilder(WebApiRootURL);
                    string api = string.Format(SettingsHelper.PersonalizationAPIUser, userid);
                    Uri APIEndPointUri =
                        new Uri(EndPointUri.Append(string.Format(api)).ToString());
                    Helper JsonHelperObj = new Helper();
                    //Gets the response returned by the Personalization API
                    userinfo = await JsonHelperObj.APIGetCall(string.Format(APIEndPointUri.ToString()));
                    return userinfo;
                }
                else
                {
                    //Write the trace in db that no url exists
                    LoggerHelper.WriteToLog("WebApiRootURL URL is null", CoreConstants.Priority.High, CoreConstants.Category.Error);
                    return null;
                }
            }
            catch (Exception exception)
            {
                // logging an error if in case some exception occurs
                LoggerHelper.WriteToLog(exception, "Error while fetching the most popular videos" + exception.ToString());
                throw new DataAccessException("Data Access Exception:-Error while fetching Getuserinfo ");
            }
        }
        public async Task<string> Getuserbackendinfo(string userid)
        {
            string userbackendinfo = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(WebApiRootURL))
                {
                    //Creates the enpoint uri to be called
                    StringBuilder EndPointUri = new StringBuilder(WebApiRootURL);
                    Uri APIEndPointUri =
                        new Uri(EndPointUri.Append(string.Format(SettingsHelper.PersonalizationAPIUserBackend, userid)).ToString());
                    Helper JsonHelperObj = new Helper();
                    //Gets the response returned by the Personalization API
                    userbackendinfo = await JsonHelperObj.APIGetCall(string.Format(APIEndPointUri.ToString()));
                }
                else
                {
                    //Write the trace in db that no url exists
                    LoggerHelper.WriteToLog("WebApiRootURL URL is null", CoreConstants.Priority.High, CoreConstants.Category.Error);
                    return null;
                }
            }
            catch (Exception exception)
            {

                // logging an error if in case some exception occurs
                LoggerHelper.WriteToLog(exception, "Error while fetching the most popular videos" + exception.ToString());
                throw new DataAccessException("Data Access Exception:-Error while fetching Getuserbackendinfo ");

            }
            return userbackendinfo;
        }
        public async Task<string> Getselectuserbackendinfo(string userid, string selectedbackend)
        {
            string slecteduserbackendinfo = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(WebApiRootURL))
                {
                    //Creates the enpoint uri to be called
                    StringBuilder EndPointUri = new StringBuilder(WebApiRootURL);
                    Uri APIEndPointUri =
                        new Uri(EndPointUri.Append(string.Format(SettingsHelper.PersonalizationAPIBackendId, userid)).ToString());
                    Helper JsonHelperObj = new Helper();
                    //Gets the response returned by the Personalization API
                    slecteduserbackendinfo = await JsonHelperObj.APIGetCall(string.Format(APIEndPointUri.ToString()));
                }
                else
                {
                    //Write the trace in db that no url exists
                    LoggerHelper.WriteToLog("WebApiRootURL URL is null", CoreConstants.Priority.High, CoreConstants.Category.Error);
                    return null;
                }
            }
            catch (Exception exception)
            {

                // logging an error if in case some exception occurs
                LoggerHelper.WriteToLog(exception, "Error while fetching the most popular videos" + exception.ToString());
                throw new DataAccessException("Data Access Exception:-Error while fetching Getselectuserbackendinfo");

            }
            return slecteduserbackendinfo;
        }
        public async Task<string> Getuserdevicesinfo(string userid)
        {
            string userdeviceinfo = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(WebApiRootURL))
                {
                    //Creates the enpoint uri to be called
                    StringBuilder EndPointUri = new StringBuilder(WebApiRootURL);
                    Uri APIEndPointUri =
                        new Uri(EndPointUri.Append(string.Format(SettingsHelper.PersonalizationAPIUserDevice, userid)).ToString());
                    Helper JsonHelperObj = new Helper();
                    //Gets the response returned by the Personalization API
                    userdeviceinfo = await JsonHelperObj.APIGetCall(string.Format(APIEndPointUri.ToString()));
                }
                else
                {
                    //Write the trace in db that no url exists
                    LoggerHelper.WriteToLog("WebApiRootURL URL is null", CoreConstants.Priority.High, CoreConstants.Category.Error);
                    return null;
                }
            }
            catch (Exception exception)
            {

                // logging an error if in case some exception occurs
                LoggerHelper.WriteToLog(exception, "Error while fetching the most popular videos" + exception.ToString());
                throw new DataAccessException("Data Access Exception:-Error while fetching userdeviceinfo");

            }
            return userdeviceinfo;
        }
        public async Task<string> Getselectuserdeviceinfo(string userid, int deviceid)
        {
            string selectuserdeviceinfo = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(WebApiRootURL))
                {
                    //Creates the enpoint uri to be called
                    StringBuilder EndPointUri = new StringBuilder(WebApiRootURL);
                    Uri APIEndPointUri =
                        new Uri(EndPointUri.Append(string.Format(SettingsHelper.PersonalizationAPIUserDeviceID, userid)).ToString());
                    Helper JsonHelperObj = new Helper();
                    //Gets the response returned by the Personalization API
                    selectuserdeviceinfo = await JsonHelperObj.APIGetCall(string.Format(APIEndPointUri.ToString()));
                }
                else
                {
                    //Write the trace in db that no url exists
                    LoggerHelper.WriteToLog("WebApiRootURL URL is null", CoreConstants.Priority.High, CoreConstants.Category.Error);
                    return null;
                }
            }
            catch (Exception exception)
            {

                // logging an error if in case some exception occurs
                LoggerHelper.WriteToLog(exception, "Error while fetching the most popular videos" + exception.ToString());
                throw new DataAccessException("Data Access Exception:-Error while fetching selectuserdeviceinfo");


            }
            return selectuserdeviceinfo;
        }
        public async Task<string> SaveUserinfo(PersonalizationRequsetDTO userdata, string userid)
        {
            string rspSaveinfo = string.Empty;
            try
            {
                string WebApiRootURL = SettingsHelper.WebApiUrl;
                if (!string.IsNullOrEmpty(WebApiRootURL))
                {
                    //Creates the enpoint uri to be called
                    StringBuilder EndPointUri = new StringBuilder(WebApiRootURL);
                    string api = string.Format(SettingsHelper.PersonalizationAPIUser, userid);
                    Uri spotlightEndPointUri =
                        new Uri(EndPointUri.Append(string.Format(api)).ToString());
                    Helper JsonHelperObj = new Helper();
                    //Gets the response returned by the Personalization API
                    rspSaveinfo = await JsonHelperObj.APIPostCall(string.Format(spotlightEndPointUri.ToString()), userdata);
                }
                else
                {
                    //Write the trace in db that no url exists
                    LoggerHelper.WriteToLog("WebApiRootURL URL is null", CoreConstants.Priority.High, CoreConstants.Category.Error);
                    return null;
                }
            }
            catch (Exception exception)
            {
                // logging an error if in case some exception occurs
                LoggerHelper.WriteToLog(exception, "Error while fetching the most popular videos" + exception.ToString());
                throw new DataAccessException("Data Access Exception:-Error while saving user info");
            }
            return rspSaveinfo;
        }
        public async Task<string> GetApprovalrequestcount(SynchRequestDTO syncData, string userid)
        {
            string rspRequestcountinfo = string.Empty;
            try
            {
                string WebApiRootURL = SettingsHelper.WebApiUrl;
                if (!string.IsNullOrEmpty(WebApiRootURL))
                {
                    //Creates the enpoint uri to be called
                    StringBuilder EndPointUri = new StringBuilder(WebApiRootURL);
                    string api = string.Format(SettingsHelper.SyncAPIUserBackend, userid);
                    //SettingsHelper.PersonalizationAPIUser + userid + "/";
                    Uri spotlightEndPointUri =
                        new Uri(EndPointUri.Append(string.Format(api)).ToString());
                    Helper JsonHelperObj = new Helper();
                    //Gets the response returned by the Sync API
                    rspRequestcountinfo = await JsonHelperObj.SyncAPIPostCall(string.Format(spotlightEndPointUri.ToString()), syncData);
                }
                else
                {
                    //Write the trace in db that no url exists
                    LoggerHelper.WriteToLog("WebApiRootURL URL is null", CoreConstants.Priority.High, CoreConstants.Category.Error);
                    return null;
                }
            }
            catch (Exception exception)
            {
                // logging an error if in case some exception occurs
                LoggerHelper.WriteToLog(exception, "Error while fetching the most popular videos" + exception.ToString());
                throw new DataAccessException("Data Access Exception:-Error while saving user info");
            }

            return rspRequestcountinfo;
        }
        public async Task<string> GetUserBackendTasks(SynchRequestDTO syncData, string userid, string backendId)
        {
            string rspRequesttasks = string.Empty;
            try
            {
                string WebApiRootURL = SettingsHelper.WebApiUrl;
                if (!string.IsNullOrEmpty(WebApiRootURL))
                {
                    //Creates the enpoint uri to be called
                    StringBuilder EndPointUri = new StringBuilder(WebApiRootURL);
                    string api = string.Format(SettingsHelper.SyncAPIUserBackendReq, userid, backendId);
                    //SettingsHelper.PersonalizationAPIUser + userid + "/";
                    Uri spotlightEndPointUri =
                        new Uri(EndPointUri.Append(string.Format(api)).ToString());
                    Helper JsonHelperObj = new Helper();
                    //Gets the response returned by the Sync API
                    rspRequesttasks = await JsonHelperObj.SyncAPIPostCall(string.Format(spotlightEndPointUri.ToString()), syncData);
                }
                else
                {
                    //Write the trace in db that no url exists
                    LoggerHelper.WriteToLog("WebApiRootURL URL is null", CoreConstants.Priority.High, CoreConstants.Category.Error);
                    return null;
                }
            }
            catch (Exception exception)
            {
                // logging an error if in case some exception occurs
                LoggerHelper.WriteToLog(exception, "Error while fetching the most popular videos" + exception.ToString());
                throw new DataAccessException("Data Access Exception:-Error while saving user info");
            }

            return rspRequesttasks;
        }
        public async Task<string> GetRequestInfo(SynchRequestDTO syncData,string requestID)
        {
            string rspRequesttasks = string.Empty;
            try
            {
                string WebApiRootURL = SettingsHelper.WebApiUrl;
                if (!string.IsNullOrEmpty(WebApiRootURL))
                {
                    //Creates the enpoint uri to be called
                    StringBuilder EndPointUri = new StringBuilder(WebApiRootURL);
                    string api = string.Format(SettingsHelper.SyncAPIUserBackendReqID, requestID);
                    //SettingsHelper.PersonalizationAPIUser + userid + "/";
                    Uri spotlightEndPointUri =
                        new Uri(EndPointUri.Append(string.Format(api)).ToString());
                    Helper JsonHelperObj = new Helper();
                    //Gets the response returned by the Sync API
                    rspRequesttasks = await JsonHelperObj.SyncAPIPostCall(string.Format(spotlightEndPointUri.ToString()), syncData);
                }
                else
                {
                    //Write the trace in db that no url exists
                    LoggerHelper.WriteToLog("WebApiRootURL URL is null", CoreConstants.Priority.High, CoreConstants.Category.Error);
                    return null;
                }
            }
            catch (Exception exception)
            {
                // logging an error if in case some exception occurs
                LoggerHelper.WriteToLog(exception, "Error while fetching the most popular videos" + exception.ToString());
                throw new DataAccessException("Data Access Exception:-Error while saving user info");
            }

            return rspRequesttasks;
        }
        public async Task<string> GetApprovers(SynchRequestDTO syncData, string requestID)
        {
            string rspApprovers = string.Empty;
            try
            {
                string WebApiRootURL = SettingsHelper.WebApiUrl;
                if (!string.IsNullOrEmpty(WebApiRootURL))
                {
                    //Creates the enpoint uri to be called
                    StringBuilder EndPointUri = new StringBuilder(WebApiRootURL);
                    string api = string.Format(SettingsHelper.SyncAPIUApprovers, requestID);
                    //SettingsHelper.PersonalizationAPIUser + userid + "/";
                    Uri spotlightEndPointUri =
                        new Uri(EndPointUri.Append(string.Format(api)).ToString());
                    Helper JsonHelperObj = new Helper();
                    //Gets the response returned by the Sync API
                    rspApprovers = await JsonHelperObj.SyncAPIPostCall(string.Format(spotlightEndPointUri.ToString()), syncData);
                }
                else
                {
                    //Write the trace in db that no url exists
                    LoggerHelper.WriteToLog("WebApiRootURL URL is null", CoreConstants.Priority.High, CoreConstants.Category.Error);
                    return null;
                }
            }
            catch (Exception exception)
            {
                // logging an error if in case some exception occurs
                LoggerHelper.WriteToLog(exception, "Error while fetching the most popular videos" + exception.ToString());
                throw new DataAccessException("Data Access Exception:-Error while saving user info");
            }

            return rspApprovers;
        }
        public async Task<string> SendApprovalInfo(ApprovalQuery ObjApprovalQuer, string requestID)
        {
            string rspApprovaltasks = string.Empty;
            try
            {
                string WebApiRootURL = SettingsHelper.WebApiUrl;
                if (!string.IsNullOrEmpty(WebApiRootURL))
                {
                    //Creates the enpoint uri to be called
                    StringBuilder EndPointUri = new StringBuilder(WebApiRootURL);
                    string api = string.Format(SettingsHelper.ApprovalAPIReqID, requestID);
                    //SettingsHelper.PersonalizationAPIUser + userid + "/";
                    Uri spotlightEndPointUri =
                        new Uri(EndPointUri.Append(string.Format(api)).ToString());
                    Helper JsonHelperObj = new Helper();
                    //Gets the response returned by the Sync API
                    rspApprovaltasks = await JsonHelperObj.ApprovalAPIPostCall(string.Format(spotlightEndPointUri.ToString()), ObjApprovalQuer);
                }
                else
                {
                    //Write the trace in db that no url exists
                    LoggerHelper.WriteToLog("WebApiRootURL URL is null", CoreConstants.Priority.High, CoreConstants.Category.Error);
                    return null;
                }
            }
            catch (Exception exception)
            {
                // logging an error if in case some exception occurs
                LoggerHelper.WriteToLog(exception, "Error while fetching the most popular videos" + exception.ToString());
                throw new DataAccessException("Data Access Exception:-Error while saving user info");
            }

            return rspApprovaltasks;
        }
    }
}