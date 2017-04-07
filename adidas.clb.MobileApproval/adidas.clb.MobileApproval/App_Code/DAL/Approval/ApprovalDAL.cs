using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using adidas.clb.MobileApproval.Exceptions;
using adidas.clb.MobileApproval.Models;
using adidas.clb.MobileApproval.Utility;
using adidas.clb.MobileApproval.Helpers;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System.Threading.Tasks;
namespace adidas.clb.MobileApproval.App_Code.DAL.Approval
{
    /// <summary>
    /// Implements ApprovalDAL class
    /// </summary>
    public class ApprovalDAL
    {
        //Application insights interface reference for logging the error details into Application Insight azure service.
        static IAppInsight InsightLogger { get { return AppInsightLogger.Instance; } }
        private static string taskApprovedComment = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["TaskApprovedComment"]);
        //create APIController varaible
        private APIController apiController;
        public ApprovalDAL()
        {
            //crete new object for APIController class
            apiController = new APIController();
        }
        /// <summary>
        /// This method updates approval object status
        /// </summary>
        /// <param name="objApprQry"></param>
        /// <returns></returns>
        public void UpdateApprovalObjectStatus(ApprovalQuery objApprQry)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                //ApprovalResponse objApprovalResponse = null;  
                string acknowledgement=string.Empty;
                string backendID = string.Empty;
                string domain = string.Empty;
                //reading ApprovalQuery request object properties and assign the values to below variables.
                string userID = objApprQry.UserID;
                string taskID = objApprQry.ApprovalRequestID;
                string status = objApprQry.ApprovalDecision.Status;
                //string comment = objApprQry.ApprovalDecision.Comment;
                string comment = string.Format(taskApprovedComment, objApprQry.DeviceID);
                DateTime decisionDate = objApprQry.ApprovalDecision.DecisionDate;
                string requestID = string.Empty;
                string approverOrder = string.Empty;
                string[] taskIDArr = taskID.Split('_');
                if (taskIDArr != null && (taskIDArr.Length == 2))
                {
                    approverOrder = Convert.ToString(taskIDArr[1]);
                }
                //get approvalrequest object from RequestTransactions azure table based on partitionkey and rowkey(requestID)
                ApprovalEntity apprReqEntity = DataProvider.Retrieveentity<ApprovalEntity>(CoreConstants.AzureTables.RequestTransactions, string.Concat(CoreConstants.AzureTables.ApprovalPK, userID), taskID);
                if (apprReqEntity != null)
                {
                    backendID = apprReqEntity.BackendID;
                    domain = apprReqEntity.Domain;
                    requestID = apprReqEntity.RequestId;
                    //update status,decisiondate,comment column values in approvalrequest object
                    apprReqEntity.Status = status;
                    apprReqEntity.DecisionDate = decisionDate;
                    apprReqEntity.Comment = comment;
                    //updating the backend confirmed flag set to false
                    apprReqEntity.BackendConfirmed = false;
                    //call dataprovider method to update entity to azure table
                    DataProvider.UpdateEntity<ApprovalEntity>(CoreConstants.AzureTables.RequestTransactions, apprReqEntity);
                    InsightLogger.TrackEvent("ApprovalAPIController :: Endpoint : api/approval/requests/{ " + requestID + "} , Action :: Update Approval object data in Azure table, Response :: Success ,Details are ApprovalRequestID=" + requestID + " Approver:" + userID + " status:" + status);
                    InsightLogger.TrackEvent("ApprovalAPIController :: Endpoint : api/approval/requests/{ " + requestID + "} , Action :: Cleared Backend-Confirmed flag(set Backend-Confirmed value to false), Response :: Success ");
                    //get domain,backendid details from service layer
                    objApprQry.Domain = domain;
                    objApprQry.BackendID = backendID;
                    objApprQry.ApprovalRequestID = requestID;
                    objApprQry.ApproverOrder = approverOrder;
                    //call backend requestApproval/ api
                    var result = apiController.UpdateApprovalRequest(objApprQry, backendID, requestID);
                    //if (string.IsNullOrEmpty(result.ToString()))
                    //{                    
                    //    acknowledgement = result.ToString();
                    //}
                }
                else
                {
                    InsightLogger.TrackEvent("ApprovalAPIController :: Endpoint : api/approval/requests/{ requestID } , Action :: Update Approval object data in Azure table, Response :: Error ,Details are ApprovalRequestID=" + requestID + " Approver:" + userID + " status:" + status);

                }
                //return acknowledgement;
            }
            //catch (ServiceLayerException serException)
            //{
            //    throw serException;
            //}
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                //throw new DataAccessException("Error while updating the approval object staus in ApprovalDAL :: UpdateApprovalObjectStatus() method,Error Message : " + exception.Message, exception.InnerException);
            }
        }
       
    }
}