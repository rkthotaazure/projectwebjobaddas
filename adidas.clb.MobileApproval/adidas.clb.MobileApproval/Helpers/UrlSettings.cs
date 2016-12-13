using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using adidas.clb.MobileApproval.Exceptions;
using adidas.clb.MobileApproval.Utility;
namespace adidas.clb.MobileApproval.Helpers
{
    public class UrlSettings
    {
        public static string GetBackendAgentRequestApprovalAPI(string bacekndID,string apprRequestID)        
        {
            try
            {
                string apiuri = string.Format(ConfigurationManager.AppSettings["BackendAPIURL"].ToString() + string.Format(ConfigurationManager.AppSettings["BackendAgentRequestApprovalAPIRouteMethod"].ToString(), bacekndID, apprRequestID));
                return apiuri;
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error while formatting the backend agent request approval api url in  : UrlSettings::GetBackendAgentRequestApprovalAPI() "
                     + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new ServiceLayerException(exception.Message, exception.InnerException);
            }

        }
    }
}