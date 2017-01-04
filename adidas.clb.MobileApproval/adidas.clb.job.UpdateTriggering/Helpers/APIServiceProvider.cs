using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
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
        public string CallBackendAgent(string backendID, string UpdateTriggeringMessage)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                string acknowledgement = string.Empty;
                RequestsUpdateAck Objacknowledgement = null;
                using (HttpClient client = new HttpClient())
                {
                   
                    //get API endpoint and format
                    string backendApiEndpoint = UrlSettings.GetBackendAgentRequestApprovalAPI(backendID);
                    InsightLogger.TrackEvent("Invoking backend agent API through UpdateTriggering module :: Start ::  \n API Endpont : " + backendApiEndpoint + " \n UpdateTriggeringMessage: " + UpdateTriggeringMessage);
                    //Post Triggers the pulling of updated requests data from a the given backend / given requests
                    var request = new HttpRequestMessage(HttpMethod.Post, backendApiEndpoint);
                    request.Content = new StringContent(UpdateTriggeringMessage, Encoding.UTF8, "application/json");
                    var result = client.SendAsync(request).Result;
                    //if the api call returns successcode then return the result into string
                    if (result.IsSuccessStatusCode)
                    {
                        string response = result.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                        if (!string.IsNullOrEmpty(response))
                        {
                            //request update acknowledgement
                            Objacknowledgement = JsonConvert.DeserializeObject<RequestsUpdateAck>(response);
                            //if request update acknowledgement error object is null means backend api successfully called
                            if (Objacknowledgement.Error == null)
                            {
                                acknowledgement = "Backend API has been invoked successfully. ";
                            }
                            else
                            {
                                InsightLogger.TrackEvent("Backend API has thrown an error :: ErrorMessage=" + Objacknowledgement.Error.longtext);
                            }
                        }
                                               
                    }
                    InsightLogger.TrackEvent("Invoking backend agent API through UpdateTriggering module :: End");
                }
                return acknowledgement;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                throw new ServiceLayerException(exception.Message,exception.InnerException);
            }
        }
    }
}
