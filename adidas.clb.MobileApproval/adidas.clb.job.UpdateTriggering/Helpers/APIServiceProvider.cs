using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Net.Http;
using adidas.clb.job.UpdateTriggering.Exceptions;
using adidas.clb.job.UpdateTriggering.Utility;
using adidas.clb.job.UpdateTriggering.Models;
using Newtonsoft.Json;

namespace adidas.clb.job.UpdateTriggering.Helpers
{
    class APIServiceProvider
    {
        //Application insights interface reference for logging the error details into Application Insight azure service.
        static IAppInsight InsightLogger { get { return AppInsightLogger.Instance; } }

        /// <summary>
        /// This method call backend agent api requestsUpdateRetrieval (agents/{backendID}/requestsUpdateRetrieval)
        /// </summary>
        /// <param name="backendID"></param>
        /// <param name="UpdateTriggeringMessage"></param>
        /// <returns></returns>
        public void CallBackendAgent(string backendID, string UpdateTriggeringMessage,string messagecategory, string queueName)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                string acknowledgement = string.Empty;
               // RequestsUpdateAck Objacknowledgement = null;
                using (HttpClient client = new HttpClient())
                {
                   
                    //get API endpoint and format
                    string backendApiEndpoint = UrlSettings.GetBackendAgentRequestApprovalAPI(backendID);
                    //Post Triggers the pulling of updated requests data from a the given backend / given requests
                    //Max Retry call from web.config
                    int maxRetryCount = Convert.ToInt32(ConfigurationManager.AppSettings["MaxRetryCount"]);
                    int maxThreadSleepInMilliSeconds = Convert.ToInt32(ConfigurationManager.AppSettings["MaxThreadSleepInMilliSeconds"]);
                    int RetryAttemptCount = 0;
                    bool IsSuccessful = false;
                    InsightLogger.TrackEvent(queueName + " , Action :: Pass " + messagecategory + " message to agent (Invoking backend agent API) :: \n Response :: Success \n API Endpont : " + backendApiEndpoint + " \n RequestUpdateMessage: " + UpdateTriggeringMessage);
                    //Implemented Use of retry / back-off logic
                    do
                    {
                        try
                        {
                            var request = new HttpRequestMessage(HttpMethod.Post, backendApiEndpoint);
                            request.Content = new StringContent(UpdateTriggeringMessage, Encoding.UTF8, "application/json");
                           // InsightLogger.TrackEvent("updatetriggerinputqueue, Action :: Pass user message to agent (Invoking backend agent API) , Response :: Success");
                            var result = client.SendAsync(request).Result;
                            //if the api call returns successcode then return the result into string

                            //if (result.IsSuccessStatusCode)
                            //{
                            //    string response = result.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                            //    if (!string.IsNullOrEmpty(response))
                            //    {
                            //        //request update acknowledgement
                            //        Objacknowledgement = JsonConvert.DeserializeObject<RequestsUpdateAck>(response);
                            //        //if request update acknowledgement error object is null means backend api successfully called
                            //        if (Objacknowledgement.Error == null)
                            //        {
                            //            acknowledgement = "Backend API has been invoked successfully. ";
                            //            InsightLogger.TrackEvent("adidas.clb.job.UpdateTriggering :: updatetriggerinputqueue, Action :: Pass user message to agent (Invoking backend agent API) , Response :: Success");

                            //        }
                            //        else
                            //        {
                            //            InsightLogger.TrackEvent("adidas.clb.job.UpdateTriggering :: updatetriggerinputqueue, Action :: Pass user message to agent (Invoking backend agent API) , Response :: Backend API has thrown an error :: ErrorMessage=" + Objacknowledgement.Error.longtext);
                            //        }
                            //    }

                            //}
                            IsSuccessful = true;
                        }
                        catch (Exception serviceException)
                        {
                            //Increasing RetryAttemptCount variable
                            RetryAttemptCount = RetryAttemptCount + 1;
                            //Checking retry call count is eual to max retry count or not
                            if (RetryAttemptCount == maxRetryCount)
                            {
                                InsightLogger.Exception("Error in " + queueName +  ":: CallBackendAgent() method :: Retry attempt count: [ " + RetryAttemptCount + " ]", serviceException, callerMethodName);
                                throw new ServiceLayerException(serviceException.Message, serviceException.InnerException);
                            }
                            else
                            {
                                InsightLogger.Exception("Error in " + queueName + ":: CallBackendAgent() method :: Retry attempt count: [ " + RetryAttemptCount + " ]", serviceException, callerMethodName);
                                //Putting the thread into some milliseconds sleep  and again call the same method call.
                                Thread.Sleep(maxThreadSleepInMilliSeconds);
                            }
                        }

                    } while (!IsSuccessful);                   
                   
                }
                //return acknowledgement;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                throw new ServiceLayerException(exception.Message,exception.InnerException);
            }
        }
    }
}
