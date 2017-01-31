//-----------------------------------------------------------
// <copyright file="Helper.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using adidas.clb.MobileApprovalUI.Controllers;
using adidas.clb.MobileApprovalUI.Exceptions;
using adidas.clb.MobileApprovalUI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft;
using Newtonsoft.Json;
using Microsoft.Azure.ActiveDirectory.GraphClient;

namespace adidas.clb.MobileApprovalUI.Utility
{
    /// <summary>
    /// The Helper class which contains all common the functions which is used across the application.
    /// </summary
    public class Helper
    {
        /// <summary>
        /// This method is exceuted to call the API endpoint URI
        /// </summary>
        /// <param name="endpointUri">endpoint URI to be called</param>
        /// <param name="credentials">credential needed to connect</param>
        /// <returns>result received from the API</returns>
        /// <exception cref="Exception">Error while connecting to API</exception>
        // Get API call for user details
        public async Task<string> APIGetCall(string endpointUri)
        {
            string result = string.Empty;
            try
            {
                UserProfileController userProfileObj = new UserProfileController();                
                //create object of client request
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Accept", "application/json;odata=verbose");
                    client.DefaultRequestHeaders.Add("ContentType", "application/json;odata=verbose");
                    //client.DefaultRequestHeaders.Add("Authorization", "Bearer " + await userProfileObj.GetTokenForApplication());
                    result = await client.GetStringAsync(endpointUri);
                    //if(responseMessage.StatusCode.Equals(HttpStatusCode.NotFound))
                    //{
                    //    return responseMessage;
                    //}
                    return result;
                }
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception, "Error While Connecting to an API." + exception.ToString());
                throw new DataAccessException("Error While Connecting to an API");
            }
        }
        //Post call for userinformation save
        public async Task<string> APIPostCall(string endpointUri, PersonalizationRequsetDTO data)
        {
            UserProfileController userProfileObj = new UserProfileController();
            var result = string.Empty;
            try
            {
                //create object of client request
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Accept", "application/json;odata=verbose");
                    client.DefaultRequestHeaders.Add("ContentType", "application/json;odata=verbose");
                   //client.DefaultRequestHeaders.Add("Authorization", "Bearer " + await userProfileObj.GetTokenForApplication());
                    HttpResponseMessage responseMessage = await client.PutAsJsonAsync(endpointUri, data);
                    if ((HttpStatusCode.OK).ToString().Equals(responseMessage.ReasonPhrase))
                    {
                        result = responseMessage.ReasonPhrase.ToString(); ;//"Data has been successfully saved";
                    }
                    else
                    {
                        result = SettingsHelper.ErrorSavedataMsg; //"Error occurred while saving data";
                    }
                }
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception, "Error While Connecting to an API." + exception.ToString());
                throw new DataAccessException("Error While Connecting to an API");
            }
            return result;
        }
        //Post call for sync api
        public async Task<string> SyncAPIPostCall(string endpointUri, SynchRequestDTO request)
        {
            UserProfileController userProfileObj = new UserProfileController();
            var result = string.Empty;
            try
            {

                //create object of client request
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Accept", "application/json;odata=verbose");
                    client.DefaultRequestHeaders.Add("ContentType", "application/json;odata=verbose");
                    //client.DefaultRequestHeaders.Add("Authorization", "Bearer " + await userProfileObj.GetTokenForApplication());
                    //get API endpoint and format

                    var request1 = new HttpRequestMessage(HttpMethod.Post, endpointUri);
                    request1.Content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                    var result1 = client.SendAsync(request1).Result;
                    //if the api call returns successcode then return the result into string
                    if (result1.IsSuccessStatusCode)
                    {
                        result = result1.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    }
                }
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception, "Error While Connecting to an API." + exception.ToString());
                throw new DataAccessException("Error While Connecting to an API");
            }
            return result;
        }
        //Post call for approval api
        public async Task<string> ApprovalAPIPostCall(string endpointUri, ApprovalQuery ObjApprovalQuery)
        {
            UserProfileController userProfileObj = new UserProfileController();
            var result = string.Empty;
            try
            {

                //create object of client request
                using (HttpClient client = new HttpClient())
                {

                    client.DefaultRequestHeaders.Add("Accept", "application/json;odata=verbose");
                    client.DefaultRequestHeaders.Add("ContentType", "application/json;odata=verbose");
                    //client.DefaultRequestHeaders.Add("Authorization", "Bearer " + await userProfileObj.GetTokenForApplication());
                   
                    //get API endpoint and format

                    var request1 = new HttpRequestMessage(HttpMethod.Post, endpointUri);
                    request1.Content = new StringContent(JsonConvert.SerializeObject(ObjApprovalQuery), Encoding.UTF8, "application/json");
                    var result1 = client.SendAsync(request1).Result;
                    //if the api call returns successcode then return the result into string
                    if (result1.IsSuccessStatusCode)
                    {
                        result = result1.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    }
                }
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception, "Error While Connecting to an API." + exception.ToString());
                throw new DataAccessException("Error While Connecting to an API");
            }
            return result;
        }
    }
        
}
