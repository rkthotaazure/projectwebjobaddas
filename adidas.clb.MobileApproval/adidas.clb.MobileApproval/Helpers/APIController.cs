using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
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
                //checking backend endpoint null or empty
                if (!string.IsNullOrEmpty(backendApiEndpoint))
                {
                    using (HttpClient client = new HttpClient())
                    {
                        //POST: Submits the approval or rejection for one specific request

                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        HttpResponseMessage responseMessage = await client.PutAsJsonAsync(backendApiEndpoint, apprReqDetails);
                        //if response message returns success code then return the successcode message
                        if (responseMessage.IsSuccessStatusCode)
                        {
                            result = responseMessage.ReasonPhrase.ToString(); 
                        }
                        else
                        {
                            result = "Error occurred while saving data";
                        }
                    }
                    return result="OK";
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