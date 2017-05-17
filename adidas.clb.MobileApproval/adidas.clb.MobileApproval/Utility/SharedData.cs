using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using adidas.clb.MobileApproval.Exceptions;
using adidas.clb.MobileApproval.Models;
using adidas.clb.MobileApproval.Utility;
using adidas.clb.MobileApproval.App_Code.BL.Synch;

namespace adidas.clb.MobileApproval.Utility
{
    public static class SharedData
    {
     
     
        static IAppInsight InsightLogger { get { return AppInsightLogger.Instance; } }
        public static char taskStatusSplitCharacter = Convert.ToChar(ConfigurationManager.AppSettings["TaskStatusSplitCharacter"]);
        public static List<string> ReturnTaskStatus(string strArr)
        {
            string callerMethodName = string.Empty;
            try
            {
                List<string> lstTaskStatus = null;
                if (!string.IsNullOrEmpty(strArr))
                {
                    lstTaskStatus = new List<string>();
                    lstTaskStatus = strArr.Split(taskStatusSplitCharacter).ToList();
                }
                return lstTaskStatus;
            }
            catch (DataAccessException dalexception)
            {
                InsightLogger.Exception(dalexception.Message, dalexception, callerMethodName);
                throw new DataAccessException(dalexception.Message, dalexception.InnerException);
            }
        }
        public static List<string> ReturnRequestStatus(string strArr)
        {
            string callerMethodName = string.Empty;
            try
            {
                List<string> lstTaskStatus = null;
                if (!string.IsNullOrEmpty(strArr))
                {
                    lstTaskStatus = new List<string>();
                    lstTaskStatus = strArr.Split(taskStatusSplitCharacter).ToList();
                }
                return lstTaskStatus;
            }
            catch (DataAccessException dalexception)
            {
                InsightLogger.Exception(dalexception.Message, dalexception, callerMethodName);
                throw new DataAccessException(dalexception.Message, dalexception.InnerException);
            }
        }
    }
}