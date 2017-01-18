using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using adidas.clb.MobileApproval.Utility;
using adidas.clb.MobileApproval.Exceptions;
using adidas.clb.MobileApproval.Models;
using Newtonsoft.Json;
namespace adidas.clb.MobileApproval.Helpers
{
    /// <summary>
    /// implements APIController contoller class for calling the backend agent  request update api's
    /// </summary>
    public class APIController
    {
        //Application insights interface reference for logging the error details into Application Insight azure service.
        static IAppInsight InsightLogger { get { return AppInsightLogger.Instance; } }

        /// <summary>
        /// This method will update the status of backend request with the help of backend agent requestApproval API
        /// </summary>
        /// <param name="apprReqDetails"></param>
        /// <param name="backendID"></param>
        /// <param name="apprReqID"></param>
        /// <returns></returns>
        public async Task<string> UpdateApprovalRequest(ApprovalQuery apprReqDetails, string backendID, string apprReqID)
        {
            string result = string.Empty;
            string callerMethodName = string.Empty;
            try
            {
                
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                //get API endpoint and format it in agents/{backendID}/requestApproval/{reqID}
                string backendApiEndpoint = UrlSettings.GetBackendAgentRequestApprovalAPI(backendID, apprReqID);
                string apiURL = string.Format(ConfigurationManager.AppSettings["BackendAgentRequestApprovalAPIRouteMethod"].ToString(), backendID, apprReqID);
                string approvalquery = JsonConvert.SerializeObject(apprReqDetails);
                InsightLogger.TrackEvent("adidas.clb.MobileApproval:: Approval API :: invoke the backend API for submit the approval or rejection for given request :: Baceknd API:" + backendApiEndpoint + " Approval Request Message:" + approvalquery);
                //Max Retry call from web.config
                int maxRetryCount = Convert.ToInt32(ConfigurationManager.AppSettings["MaxRetryCount"]);
                int maxThreadSleepInMilliSeconds = Convert.ToInt32(ConfigurationManager.AppSettings["MaxThreadSleepInMilliSeconds"]);
                int RetryAttemptCount = 0;
                bool IsSuccessful = false;
                //checking backend endpoint null or empty
                if (!string.IsNullOrEmpty(backendApiEndpoint))
                {
                    using (HttpClient client = new HttpClient())
                    {
                        //Implemented Use of retry / back-off logic
                        do
                        {
                            try
                            {
                                //POST: Submits the approval or rejection for one specific request
                                client.BaseAddress = new Uri(ConfigurationManager.AppSettings["BackendAPIURL"]);
                                client.DefaultRequestHeaders.Accept.Clear();
                                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                                StringContent content = new StringContent(approvalquery, Encoding.UTF8, "application/json");                               
                                HttpResponseMessage response = await client.PostAsync(apiURL, content);

                                //var request = new HttpRequestMessage(HttpMethod.Post, backendApiEndpoint);
                                //StringContent content = new StringContent(approvalquery, Encoding.UTF8, "application/json");
                                //request.Content = new StringContent(approvalquery, Encoding.UTF8, "application/json");
                                //var resultset = client.PostAsync(request).Result;
                                //if response message returns success code then return the successcode message
                                //if (resultset.IsSuccessStatusCode)
                                //{
                                //    string resMessage = resultset.ReasonPhrase;
                                //    string response = resultset.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                                //    if (!string.IsNullOrEmpty(response))
                                //    {
                                //        //request update acknowledgement
                                //        RequestsUpdateAck Objacknowledgement = JsonConvert.DeserializeObject<RequestsUpdateAck>(response);
                                //        //if request update acknowledgement error object is null means backend api successfully called
                                //        if (Objacknowledgement.Error == null)
                                //        {
                                //            result = resMessage;
                                //        }
                                //    }
                                //}
                                //else
                                //{
                                //    result = "Error occurred while invoking the backend Agent Request approval API.";
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
                                    InsightLogger.TrackEvent(callerMethodName + " - exception while Call baceknd agent request approval API in  ApprovalAPIController::Retry attempt count: [" + RetryAttemptCount + "]");
                                    throw new ServiceLayerException(serviceException.Message, serviceException.InnerException);
                                }
                                else
                                {
                                    InsightLogger.Exception(serviceException.Message, serviceException, callerMethodName);
                                    InsightLogger.TrackEvent(callerMethodName + " - exception while Call baceknd agent request approval API in  ApprovalAPIController::Retry attempt count: [" + RetryAttemptCount + "]");
                                    //Putting the thread into some milliseconds sleep  and again call the same method call.
                                    Thread.Sleep(maxThreadSleepInMilliSeconds);
                                }
                            }
                        } while (!IsSuccessful);
                       
                    }
                    return result;
                }
                else
                {
                    //Write the trace in db that no url exists
                    InsightLogger.TrackEvent(callerMethodName + " Error Details : baceknd agent request approval API is null");                    
                    return null;
                }
            }
            catch (Exception exception)
            {
                // logging an error if in case some exception occurs
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                throw new ServiceLayerException(exception.Message, exception.InnerException);
            }

        }
    }
}