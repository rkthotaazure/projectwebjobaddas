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
        private ApprovalDAL objApprovalDAL;
        public ApprovalBL()
        {
            objApprovalDAL = new ApprovalDAL();
        }
        public string UpdateApprovalObject(ApprovalQuery objApprQry)
        {
            try
            {
                string objApprRes = null;
                //calling data access layer method  for updating the approval status details              
                objApprRes = objApprovalDAL.UpdateApprovalObjectStatus(objApprQry);
                return objApprRes;
            }
            catch (DataAccessException DALexception)
            {
                throw DALexception;
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error in BL while updating ApprovalEntity details in BL.Approval::UpdateApprovalObject() "
                       + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new BusinessLogicException("Error in BL while updating ApprovalEntity details in BL.Approval::UpdateApprovalObject()" + exception.Message, exception.InnerException);
            }
        }
    }
}