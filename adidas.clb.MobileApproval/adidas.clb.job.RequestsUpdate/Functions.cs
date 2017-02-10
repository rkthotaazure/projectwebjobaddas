//-----------------------------------------------------------
// <copyright file="Functions.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;
using adidas.clb.job.RequestsUpdate.APP_Code.BL;
using adidas.clb.job.RequestsUpdate.Exceptions;
using adidas.clb.job.RequestsUpdate.Models;
using adidas.clb.job.RequestsUpdate.Utility;

namespace adidas.clb.job.RequestsUpdate
{
    public class Functions
    {
        static IAppInsight InsightLogger { get { return AppInsightLogger.Instance; } }
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
                InsightLogger.TrackEvent("RequestUpdateWebJob :: method : requestupdate queue trigger, action:Reading queue message, response:succuess, message: "+message);
                //deserialize Queue message to Requests 
                RequestsUpdateData requestsdata = JsonConvert.DeserializeObject<RequestsUpdateData>(message);
                List<BackendRequest> backendrequestslist = requestsdata.Requests;
                RequestUpdateBL requsetupdatebl = new RequestUpdateBL();
                //get backend to get missingconfirmationlimit for backend and also to update flags
                BackendEntity backend = requsetupdatebl.Getbackend(requestsdata.BackendID);
                int TotalRequestsize = 0;
                int TotalRequestlatency = 0;
                int requestcount = backendrequestslist.Count;
                //check if requests were available
                if (backendrequestslist != null && backendrequestslist.Count>0)
                {
                    //looping through each backendrequest to add requests , approvers and fields for each requet
                    InsightLogger.TrackEvent("RequestUpdateWebJob :: method : requestupdate queue trigger, action:looping all requests");
                    foreach (BackendRequest backendrequest in backendrequestslist)
                    {
                        //split main backendrequest object into individual entities
                        List<Approvers> approvers = backendrequest.RequestsList.Approvers;
                        List<Field> genericInfoFields = backendrequest.RequestsList.Fields.GenericInfo;
                        List<Field> overviewFields = backendrequest.RequestsList.Fields.Overview;
                        Request request = backendrequest.RequestsList;
                        //calling BL methods to add request , approval, approvers and fields
                        if(!string.IsNullOrEmpty(request.UserID))
                        {
                            InsightLogger.TrackEvent("RequestUpdateWebJob :: method : requestupdate queue trigger, action:clearing request waiting flag, response:success");
                            requsetupdatebl.AddUpdateRequest(backendrequest, request.UserID, requestsdata.BackendID);
                            InsightLogger.TrackEvent("RequestUpdateWebJob :: method : requestupdate queue trigger, action:update request object, response:success, requestID:"+request.ID);
                            requsetupdatebl.AddUpdateApproval(approvers, request.ID, backendrequest.RequestsList.UserID, requestsdata.BackendID,backend.MissingConfirmationsLimit);
                            InsightLogger.TrackEvent("RequestUpdateWebJob :: method : requestupdate queue trigger, action:update/remove/create approval object, response:success");
                            requsetupdatebl.AddUpdateApprovers(approvers, request.ID);                            
                            requsetupdatebl.AddUpdateFields(genericInfoFields, overviewFields, request.ID);                            
                            //caliculating request size                        
                            int requestsize = requsetupdatebl.CalculateRequestSize(backendrequest);
                            //caliculating total of size for all requests
                            TotalRequestsize = TotalRequestsize + requestsize;
                            //caliculating total of latency for all requests
                            TotalRequestlatency = TotalRequestlatency + request.Latency;
                        }            
                    }
                }
                InsightLogger.TrackEvent("RequestUpdateWebJob :: method : requestupdate queue trigger, action:looping all requests end");
                //calling BL methods to update average sizes and latencies for userbackend and backend
                if (!string.IsNullOrEmpty(requestsdata.UserId))
                {
                    InsightLogger.TrackEvent("RequestUpdateWebJob :: method : requestupdate queue trigger, action:check user or request, response:user");
                    requsetupdatebl.UpdateUserBackend(requestsdata.UserId, requestsdata.BackendID, TotalRequestsize, TotalRequestlatency, requestcount);
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
                InsightLogger.TrackEvent("RequestUpdateWebJob :: method : requestpdfuri queue trigger, action:reading queue message, response:success, message:"+message);
                //deserialize Queue message to get PDF uri 
                RequestPDF requestPDFdata = JsonConvert.DeserializeObject<RequestPDF>(message);
                RequestUpdateBL requestupdatebl = new RequestUpdateBL();
                Uri PDFuri = new Uri(requestPDFdata.PDFUri);
                InsightLogger.TrackEvent("RequestUpdateWebJob :: method : requestpdfuri queue trigger, action:getting pdfuri from message, response:success");
                //updating request entity with pdf uri
                requestupdatebl.AddPDFUriToRequest(PDFuri, requestPDFdata.UserId, requestPDFdata.RequestID);
                InsightLogger.TrackEvent("RequestUpdateWebJob :: method : requestpdfuri queue trigger, action:updating request object with pdf uri, response:success, requsetID:"+requestPDFdata.RequestID);
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
