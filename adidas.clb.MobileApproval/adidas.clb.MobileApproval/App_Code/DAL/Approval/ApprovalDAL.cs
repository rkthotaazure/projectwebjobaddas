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
        public string UpdateApprovalObjectStatus(ApprovalQuery objApprQry)
        {
            try
            {
                //ApprovalResponse objApprovalResponse = null;  
                string acknowledgement = "OK";
                string backendID = string.Empty;
                //reading ApprovalQuery object properties and assign the values to below variables.
                string userID = objApprQry.UserID;
                string requestID = objApprQry.ApprovalRequesID;
                string status = objApprQry.ApprovalDecision.Status;
                string comment = objApprQry.ApprovalDecision.Comment;
                DateTime decisionDate = objApprQry.ApprovalDecision.DecisionDate;
                //get approvalrequest object from RequestTransactions azure table based on partitionkey and rowkey(requestID)
                ApprovalEntity apprReqEntity = DataProvider.Retrieveentity<ApprovalEntity>(CoreConstants.AzureTables.RequestTransactions, string.Concat(CoreConstants.AzureTables.ApproverPK, userID), requestID);
                if (apprReqEntity != null)
                {
                    backendID = apprReqEntity.BackendID;
                    //update status,decisiondate,comment column values in approvalrequest object
                    apprReqEntity.status = status;
                    apprReqEntity.DecisionDate = decisionDate;
                    apprReqEntity.Comment = comment;
                    apprReqEntity.BackendConfirmed = false;
                    //call dataprovider method to update entity to azure table
                    DataProvider.UpdateEntity<ApprovalEntity>(CoreConstants.AzureTables.RequestTransactions, apprReqEntity);
                    //call backend requestApproval/ api
                    var result = apiController.UpdateApprovalRequest(objApprQry, backendID, requestID);
                    if (string.IsNullOrEmpty(result.ToString()))
                    {
                        //Insert response into requestupdate queue
                        //AddRequestUpdateDetailsToQueue(JsonConvert.SerializeObject(result));
                        acknowledgement = result.ToString();
                    }



                }
                return acknowledgement;
            }
            catch (ServiceLayerException serException)
            {
                throw serException;
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error while updating the approval object staus in RequestTransactions azure table in DAL : "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new DataAccessException("Error while updating the approval object staus in ApprovalDAL :: UpdateApprovalObjectStatus() method,Error Message : " + exception.Message, exception.InnerException);
            }
        }
        private void AddRequestUpdateDetailsToQueue(string requestUpdateDetails)
        {
            try
            {
                // Create the queue client.
                CloudQueueClient cqdocClient = AzureQueues.GetQueueClient();
                // Retrieve a reference to a queue.
                CloudQueue queuedoc = AzureQueues.GetRequestUpdateQueue(cqdocClient);
                // Async enqueue the message                           
                CloudQueueMessage message = new CloudQueueMessage(requestUpdateDetails);
                queuedoc.AddMessage(message);
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + "Error while adding pdf url details to queue in :: AddPdfUrlDetailsToQueue() "
                  + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);

                throw new BusinessLogicException(exception.Message, exception.InnerException);

            }
        }
    }
}