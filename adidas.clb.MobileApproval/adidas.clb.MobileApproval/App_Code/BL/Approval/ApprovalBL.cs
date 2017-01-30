using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using adidas.clb.MobileApproval.App_Code.DAL.Approval;
using adidas.clb.MobileApproval.Exceptions;
using adidas.clb.MobileApproval.Models;
using adidas.clb.MobileApproval.Utility;
namespace adidas.clb.MobileApproval.App_Code.BL.Approval
{
    public class ApprovalBL
    {
        //Application insights interface reference for logging the error details into Application Insight azure service.
        static IAppInsight InsightLogger { get { return AppInsightLogger.Instance; } }
        private ApprovalDAL objApprovalDAL;
        public ApprovalBL()
        {
            objApprovalDAL = new ApprovalDAL();
        }
        /// <summary>
        /// This method updates the approval object in azure layer
        /// </summary>
        /// <param name="objApprQry"></param>
        public void UpdateApprovalObject(ApprovalQuery objApprQry)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                //calling data access layer method  for updating the approval status details              
                objApprovalDAL.UpdateApprovalObjectStatus(objApprQry);
            }
            //catch (DataAccessException DALexception)
            //{
            //    throw DALexception;
            //}
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                //throw new BusinessLogicException("Error in BL while updating ApprovalEntity details in BL.Approval::UpdateApprovalObject()" + exception.Message, exception.InnerException);
            }
        }
    }
}