using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;
using adidas.clb.job.GeneratePDF.App_Data.BAL;
using adidas.clb.job.GeneratePDF.App_Data.DAL;
using adidas.clb.job.GeneratePDF.Exceptions;
using adidas.clb.job.GeneratePDF.Helpers;
using adidas.clb.job.GeneratePDF.Models;
using adidas.clb.job.GeneratePDF.Utility;
namespace adidas.clb.job.GeneratePDF
{
    public class Functions
    {
        //Application insights interface reference for logging the error details into Application Insight azure service.
        static IAppInsight InsightLogger { get { return AppInsightLogger.Instance; } }
        // This function will get triggered/executed when a new message is written 
        // on an Azure Queue called queue.
        /// <summary>
        /// This method reads the message(RequestID,BackendID) from GeneratePdf Queue and creates pdf 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="log"></param>
        public static void ProcessGeneratePdfMessage([QueueTrigger("%pdfQueue%")] string message, TextWriter log)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
               // InsightLogger.TrackStartEvent(callerMethodName);
                if (!string.IsNullOrEmpty(message))
                {
                   // log.WriteLine("adidas.clb.job.GeneratePDF web job , Action :: Process Generate Pdf Message , Response :: message" + message);
                    InsightLogger.TrackEvent("GeneratePDF, Action :: Process Generate Pdf Message :: Start() , Response :: message" + message + "\n TimeStamp " + DateTime.UtcNow);
                    //Deserialize input queue message into RequestPdf object
                    RequestPdf objRequestPdf = JsonConvert.DeserializeObject<RequestPdf>(message);
                    if (objRequestPdf != null)
                    {
                        //get Request Pdf details from queue message
                        string backendID = objRequestPdf.BackendID;
                        string requestID = objRequestPdf.RequestID;
                        string userID = objRequestPdf.UserID;
                        //check backendID
                        switch (backendID.ToLower())
                        {
                            case CoreConstants.Backends.BPMOnline:
                                //create pdf file from store backend                                 
                                GeneratePDFFile objGenPdf = new GeneratePDFFile();
                                objGenPdf.GeneratePDFForStoreApproval(requestID, userID, backendID);
                                break;
                            case CoreConstants.Backends.CAR:
                                //create pdf file from store backend
                                GeneratePDFFile objGenPdfCAR = new GeneratePDFFile();
                                objGenPdfCAR.GeneratePDFForCARApproval(requestID, userID, backendID);
                                break;
                            default:
                                break;
                        }
                        //get RequestUpdateMsg list from UpdateTriggeringMsg

                        //log.WriteLine("adidas.clb.job.GeneratePDF :: Processing Generate Pdf message :: End()" + message);
                        InsightLogger.TrackEvent("GeneratePDF, Action :: Process Generate Pdf Message :: End() , Response :: message" + message + "\n TimeStamp " + DateTime.UtcNow);

                    }
                }
            }
            catch (DataAccessException dalexception)
            {
                //write exception message to web job dashboard logs
                log.WriteLine(dalexception.Message);
                //write  Data Access Exception into application insights
                InsightLogger.Exception(dalexception.Message, dalexception, callerMethodName);
            }
            catch (Exception exception)
            {
                //write exception message to web job dashboard logs
                log.WriteLine(exception.Message);
                //write  Exception into application insights
                InsightLogger.Exception(exception.Message, exception, callerMethodName);

            }
        }
    }
}
