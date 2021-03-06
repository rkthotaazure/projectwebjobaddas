﻿//-----------------------------------------------------------
// <copyright file="Functions.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;
using adidas.clb.job.RequestsUpdate.APP_Code.BL;
using adidas.clb.job.RequestsUpdate.APP_Code.DAL;
using adidas.clb.job.RequestsUpdate.Exceptions;
using adidas.clb.job.RequestsUpdate.Models;
using adidas.clb.job.RequestsUpdate.Utility;

namespace adidas.clb.job.RequestsUpdate
{
    public class Functions
    {
        static IAppInsight InsightLogger { get { return AppInsightLogger.Instance; } }
        //read userbackend entity azure table reference from config
        public static string azureTableUserDeviceConfiguration = ConfigurationManager.AppSettings["AzureTables.UserDeviceConfiguration"];
        // This function will get triggered/executed when a new message is written 
        // on an Azure Queue called RequsetUpdate.
        public static void ProcessRequsetQueueMessage([QueueTrigger(CoreConstants.AzureQueues.RequsetUpdateQueue)] string message, TextWriter log)
        {
            //Get Caller Method name
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                InsightLogger.TrackEvent("RequestUpdateWebJob :: method : requestupdate queue trigger, action:Reading queue message, response:succuess, message: " + message);
                //note down Request update queue message trigger timestamp
                DateTime requestUpdateMsgTriggerTimestamp = DateTime.Now;
                //deserialize Queue message to Requests 
                RequestsUpdateData requestsdata = JsonConvert.DeserializeObject<RequestsUpdateData>(message);

                List<BackendRequest> backendrequestslist = requestsdata.Requests;
                RequestUpdateBL requsetupdatebl = new RequestUpdateBL();
                //get backend to get missingconfirmationlimit for backend and also to update flags
                BackendEntity backend = requsetupdatebl.Getbackend(requestsdata.BackendID);
                //declare int variable for get total request size from backend response
                int TotalRequestsize = 0;
                //declare int variable for get total request latency from backend response
                int TotalRequestlatency = 0;
                //declare int variable for get total request count from backend response
                int requestcount = 0;
                DateTime? utQueueEntryTimestamp = null;
                //Declare UserBackendEntity object
                UserBackendEntity userbackend = null;
                //declare string varaible
                string latencyvaluesstrr = string.Empty;
                //check if requests were available
                if (backendrequestslist != null && backendrequestslist.Count > 0)
                {
                    //get request count in backend response
                    requestcount = backendrequestslist.Count;
                    if (!string.IsNullOrEmpty(requestsdata.UserId) && !string.IsNullOrEmpty(requestsdata.BackendID))
                    {
                        //calling DAL method for getting userbackend entity details
                        RequestUpdateDAL requestupdatedal = new RequestUpdateDAL();
                        userbackend = requestupdatedal.GetUserBackend(requestsdata.UserId, requestsdata.BackendID);
                        if (userbackend != null)
                        {
                            //if userbackend is not null then get userbackend UT queue message entry timestamp 
                            utQueueEntryTimestamp = userbackend.QueueMsgEntryTimestamp;
                        }
                    }
                    //looping through each backendrequest to add requests , approvers and fields for each requet
                    InsightLogger.TrackEvent("RequestUpdateWebJob :: method : requestupdate queue trigger, action:looping all requests");
                    foreach (BackendRequest backendrequest in backendrequestslist)
                    {
                        //split main backendrequest object into individual entities
                        List<Approvers> approvers = backendrequest.RequestsList.Approvers;
                        List<Field> genericInfoFields = backendrequest.RequestsList.Fields.GenericInfo;
                        List<Field> overviewFields = backendrequest.RequestsList.Fields.Overview;
                        Request request = backendrequest.RequestsList;
                        int reqLatency = 0;                       
                        //calling BL methods to add request , approval, approvers and fields
                        if (!string.IsNullOrEmpty(request.UserID))
                        {
                            reqLatency = 0;
                            InsightLogger.TrackEvent("RequestUpdateWebJob :: method : requestupdate queue trigger, action:clearing request waiting flag, response:success");
                            InsightLogger.TrackEvent("RequestUpdateWebJob :: method : requestupdate queue trigger, action:update request object, response:success, requestID:" + request.ID);
                            //This method insert/update request approval entities details into Request Transaction table
                            requsetupdatebl.AddUpdateApproval(approvers, request.ID, backendrequest.RequestsList.UserID, requestsdata.BackendID, backend.MissingConfirmationsLimit, request.Title);
                            InsightLogger.TrackEvent("RequestUpdateWebJob :: method : requestupdate queue trigger, action:update/remove/create approval object, response:success");
                            //This method insert/update request approvers details into Request Transaction table
                            requsetupdatebl.AddUpdateApprovers(approvers, request.ID);
                            //This method insert/update request fileds(Overview & Genric information) details into Request Transaction table
                            requsetupdatebl.AddUpdateFields(genericInfoFields, overviewFields, request.ID);
                            //This method insert/update request entity details into Request Transaction table
                            reqLatency = requsetupdatebl.AddUpdateRequest(backendrequest, request.UserID, requestsdata.BackendID, requestUpdateMsgTriggerTimestamp, utQueueEntryTimestamp);
                            //caliculating request size                        
                            int requestsize = requsetupdatebl.CalculateRequestSize(backendrequest);
                            //caliculating total size for all requests
                            TotalRequestsize = TotalRequestsize + requestsize;
                            //caliculating total latency of all request                          
                            TotalRequestlatency = TotalRequestlatency + reqLatency;
                            latencyvaluesstrr = latencyvaluesstrr + "," + " RequestID :" + request.ID + " Latency in milliseconds :" + reqLatency;
                        }
                    }
                    if (!string.IsNullOrEmpty(latencyvaluesstrr))
                    {
                        //log each request latency 
                        InsightLogger.TrackSpecificEvent("RequestUpdateWebJob :: " + latencyvaluesstrr);
                    }

                }
                //note down timestamp value once response sit into service layer
                DateTime responseInsertIntostorageTimestamp = DateTime.Now;
                InsightLogger.TrackEvent("RequestUpdateWebJob :: method : requestupdate queue trigger, action:looping all requests end");
                //calling BL methods to update average sizes and latencies for userbackend and backend
                if (!string.IsNullOrEmpty(requestsdata.UserId))
                {
                    InsightLogger.TrackEvent("RequestUpdateWebJob :: method : requestupdate queue trigger, action:check user or request, response:user");
                    requsetupdatebl.UpdateUserBackend(requestsdata.UserId, requestsdata.BackendID, TotalRequestsize, TotalRequestlatency, requestcount, requestUpdateMsgTriggerTimestamp, responseInsertIntostorageTimestamp);
                    InsightLogger.TrackEvent("RequestUpdateWebJob :: method : requestupdate queue trigger, action:update userbackend tracking variables, response:success");
                }
                InsightLogger.TrackEvent("RequestUpdateWebJob :: method : requestupdate queue trigger, action:update backend tracking variables, response:success, backendID:" + requestsdata.BackendID);
                requsetupdatebl.UpdateBackend(backend, TotalRequestsize, TotalRequestlatency, requestcount);
                InsightLogger.TrackEvent("RequestUpdateWebJob :: method : requestupdate queue trigger, action:end of the method");
            }
            catch (DataAccessException dalexception)
            {
                //write message to web job dashboard logs
                log.WriteLine(dalexception.Message);
            }
            catch (BusinessLogicException blexception)
            {
                //write message to web job dashboard logs
                log.WriteLine(blexception.Message);
            }
            catch (Exception exception)
            {
                //write exception into application insights
                InsightLogger.Exception(exception.Message + " - Error in Queue trigger while processing request", exception, callerMethodName);
                log.WriteLine(exception.Message);
            }
        }

        // This function will get triggered/executed when a new message is written 
        // on an Azure Queue called requestpdf.
        public static void ProcessRequsetPDFQueueMessage([QueueTrigger(CoreConstants.AzureQueues.RequsetPDFQueue)] string message, TextWriter log)
        {
            //Get Caller Method name
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                System.Threading.Thread.Sleep(5000);
                InsightLogger.TrackEvent("RequestUpdateWebJob :: method : requestpdfuri queue trigger, action:reading queue message, response:success, message:" + message);
                //deserialize Queue message to get PDF uri 
                RequestPDF requestPDFdata = JsonConvert.DeserializeObject<RequestPDF>(message);
                RequestUpdateBL requestupdatebl = new RequestUpdateBL();
                Uri PDFuri = new Uri(requestPDFdata.PDFUri);
                InsightLogger.TrackEvent("RequestUpdateWebJob :: method : requestpdfuri queue trigger, action:getting pdfuri from message, response:success");
                //updating request entity with pdf uri
                requestupdatebl.AddPDFUriToRequest(PDFuri, requestPDFdata.UserId, requestPDFdata.RequestID);
                InsightLogger.TrackEvent("RequestUpdateWebJob :: method : requestpdfuri queue trigger, action:updating request object with pdf uri, response:success, requsetID:" + requestPDFdata.RequestID);
            }
            catch (DataAccessException dalexception)
            {
                //write message to web job dashboard logs
                log.WriteLine(dalexception.Message);
            }
            catch (BusinessLogicException blexception)
            {
                //write message to web job dashboard logs
                log.WriteLine(blexception.Message);
            }
            catch (Exception exception)
            {
                //write exception into application insights
                InsightLogger.Exception(exception.Message + " - Error in Queue trigger while processing pdf uri", exception, callerMethodName);
                log.WriteLine(exception.Message);
            }
        }
    }
}
