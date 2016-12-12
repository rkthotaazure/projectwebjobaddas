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
            try
            {
                //deserialize Queue message to Requests 
                RequestsUpdateData requestsdata = JsonConvert.DeserializeObject<RequestsUpdateData>(message);
                List<BackendRequest> backendrequestslist = requestsdata.requests;
                RequestUpdateBL requsetupdatebl = new RequestUpdateBL();
                int TotalRequestsize = 0;
                int TotalRequestlatency = 0;
                int requestcount = backendrequestslist.Count;
                //check if requests were available
                if (backendrequestslist != null)
                {                
                    //looping through each request to add requests , approvers and fields for each requet
                    foreach (BackendRequest backendrequest in backendrequestslist)
                    {
                        //split main backendrequest object into individual entities
                        List<Approvers> approvers = backendrequest.requset.approvers;
                        List<Field> genericInfoFields = backendrequest.requset.fields.genericInfo;
                        List<Field> overviewFields = backendrequest.requset.fields.overview;
                        Request request = backendrequest.requset;                        
                        //calling BL methods to add request , approval, approvers and fields                        
                        requsetupdatebl.AddUpdateRequest(backendrequest, requestsdata.UserId, requestsdata.BackendID);                        
                        requsetupdatebl.AddUpdateApproval(request, requestsdata.UserId, requestsdata.BackendID);                        
                        requsetupdatebl.AddUpdateApprovers(approvers, request.id);                        
                        requsetupdatebl.AddUpdateFields(genericInfoFields, overviewFields, request.id);                        
                        //caliculating request size                        
                        int requestsize=requsetupdatebl.CalculateRequestSize(backendrequest);                       
                        //caliculating total of size for all requests
                        TotalRequestsize = TotalRequestsize+requestsize;
                        //caliculating total of latency for all requests
                        TotalRequestlatency = TotalRequestlatency + request.Latency;                   
                      }                    
                }                
                //calling BL methods to update average sizes and latencies for userbackend and backend     
                requsetupdatebl.UpdateUserBackend(requestsdata.UserId,requestsdata.BackendID,TotalRequestsize, TotalRequestlatency,requestcount);
                requsetupdatebl.UpdateBackend(requestsdata.BackendID, TotalRequestsize, TotalRequestlatency, requestcount);

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
                LoggerHelper.WriteToLog(exception + " - exception while processing queue message into entities : "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                log.WriteLine(exception.Message);
            }
        }

        // This function will get triggered/executed when a new message is written 
        // on an Azure Queue called requestpdf.
        public static void ProcessRequsetPDFQueueMessage([QueueTrigger(CoreConstants.AzureQueues.RequsetPDFQueue)] string message, TextWriter log)
        {
            try
            {
                //deserialize Queue message to get PDF uri 
                RequestPDF requestPDFdata = JsonConvert.DeserializeObject<RequestPDF>(message);
                RequestUpdateBL requestupdatebl = new RequestUpdateBL();
                //calling BL method to add Request PDF to blob
                //Uri PDFuri = requestupdatebl.AddRequestPDFToBlob(new Uri(requestPDFdata.PDFUri),requestPDFdata.RequestID);
                Uri PDFuri = new Uri(requestPDFdata.PDFUri);
                //updating request entity with pdf uri
                requestupdatebl.AddPDFUriToRequest(PDFuri, requestPDFdata.UserId,requestPDFdata.RequestID);
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
                LoggerHelper.WriteToLog(exception + " - exception while processing queue message into uri to pick pdf from temp blob and upload in requetupdate blob : "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                log.WriteLine(exception.Message);
            }
        }
    }
}
