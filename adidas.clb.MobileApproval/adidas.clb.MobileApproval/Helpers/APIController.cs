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
            try
            {
                //get API endpoint and format it in agents/{backendID}/requestApproval/{reqID}
                string backendApiEndpoint = UrlSettings.GetBackendAgentRequestApprovalAPI(backendID, apprReqID);
                string approvalquery = JsonConvert.SerializeObject(apprReqDetails);
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
                                var request = new HttpRequestMessage(HttpMethod.Post, backendApiEndpoint);
                                request.Content = new StringContent(approvalquery, Encoding.UTF8, "application/json");
                                var resultset = client.SendAsync(request).Result;
                                //if response message returns success code then return the successcode message
                                if (resultset.IsSuccessStatusCode)
                                {
                                    string response = resultset.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                                    if (!string.IsNullOrEmpty(response))
                                    {
                                        //request update acknowledgement
                                        RequestsUpdateAck Objacknowledgement = JsonConvert.DeserializeObject<RequestsUpdateAck>(response);
                                        //if request update acknowledgement error object is null means backend api successfully called
                                        if (Objacknowledgement.Error == null)
                                        {
                                            result = "OK";
                                        }
                                    }
                                }
                                else
                                {
                                    result = "Error occurred while saving data";
                                }
                                IsSuccessful = true;
                            }
                            catch (Exception serviceException)
                            {
                                //Increasing RetryAttemptCount variable
                                RetryAttemptCount = RetryAttemptCount + 1;
                                //Checking retry call count is eual to max retry count or not
                                if (RetryAttemptCount == maxRetryCount)
                                {
                                    LoggerHelper.WriteToLog(serviceException + " - exception while Call baceknd agent request approval API in  ApprovalAPIController::Retry attempt count: [" + RetryAttemptCount + "]" + serviceException.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                                    throw new ServiceLayerException(serviceException.Message, serviceException.InnerException);
                                }
                                else
                                {
                                    LoggerHelper.WriteToLog(serviceException + " - exception while Call baceknd agent request approval API in  ApprovalAPIController::Retry attempt count: [" + RetryAttemptCount + "]" + serviceException.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
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
                    LoggerHelper.WriteToLog("baceknd agent request approval API is null", CoreConstants.Priority.High, CoreConstants.Category.Error);
                    return null;
                }
            }
            catch (Exception exception)
            {
                // logging an error if in case some exception occurs
                LoggerHelper.WriteToLog(exception + " - exception while Call baceknd agent request approval API in  ApprovalAPIController: Post Method()"
                     + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new ServiceLayerException(exception.Message, exception.InnerException);
            }

        }
    }
}