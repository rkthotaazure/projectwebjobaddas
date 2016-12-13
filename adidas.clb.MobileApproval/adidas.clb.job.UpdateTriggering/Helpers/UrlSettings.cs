using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using adidas.clb.job.UpdateTriggering.Exceptions;
using adidas.clb.job.UpdateTriggering.Utility;
namespace adidas.clb.job.UpdateTriggering.Helpers
{
    public class UrlSettings
    {
        //Application insights interface reference for logging the error details into Application Insight azure service.
        static IAppInsight InsightLogger { get { return AppInsightLogger.Instance; } }
        /// <summary>
        /// This method format backend agent api requestsUpdateRetrieval (example :: http://backendagent.net/api/agents/{backendID}/requestsUpdateRetrieval)
        /// </summary>
        /// <param name="bacekndID"></param>
        /// <returns></returns>
        public static string GetBackendAgentRequestApprovalAPI(string bacekndID)        
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                //concat backend agent main url + api method
                string apiuri = string.Format(ConfigurationManager.AppSettings["BackendAgentURL"].ToString() + string.Format(ConfigurationManager.AppSettings["BackendAgentRequestUpdateAPI"].ToString(), bacekndID));
                return apiuri;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                throw new ServiceLayerException(exception.Message, exception.InnerException);
            }

        }
    }
}