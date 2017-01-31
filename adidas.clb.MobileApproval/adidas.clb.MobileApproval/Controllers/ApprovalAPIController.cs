//-----------------------------------------------------------
// <copyright file="ApprovalAPIController.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using adidas.clb.MobileApproval.Exceptions;
using adidas.clb.MobileApproval.Models;
using adidas.clb.MobileApproval.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using log4net;
using adidas.clb.MobileApproval.App_Code.BL.Approval;
using Newtonsoft.Json;
namespace adidas.clb.MobileApproval.Controllers
{
    //[Authorize]

    public class ApprovalAPIController : ApiController
    {
        //Application insights interface reference for logging the error details into Application Insight azure service.
        static IAppInsight InsightLogger { get { return AppInsightLogger.Instance; } }
        /// <summary>
        /// This method updates the approval object in service layer and invoke the backend agent
        /// </summary>
        /// <param name="ObjApprovalQuery"></param>
        /// <returns></returns>
        // POST: api/ApprovalAPI
        [Route("api/approval/requests/{apprReqID}")]
        public HttpResponseMessage PostApproval(ApprovalQuery ObjApprovalQuery)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                InsightLogger.TrackEvent("adidas.clb.MobileApproval:: ApprovalAPIController :: Endpoint : api/approval/requests/{apprReqID} , Action :: POST: Updates the status of a user approval request method execution has been started, TimeStamp :: " + DateTime.UtcNow);
                //Checking ApprovalQuery object is valid or not(User and decision status are the mandatory input data required)
                if (!string.IsNullOrEmpty(ObjApprovalQuery.UserID) && !string.IsNullOrEmpty(ObjApprovalQuery.ApprovalDecision.Status))
                {
                    //Asynchronously Updates the status of the approval object , set the backend confirmed flag to false and invoke the backend request approval api
                    //Fire And Forget Method implementaion
                    InsightLogger.TrackEvent("adidas.clb.MobileApproval:: ApprovalAPIController :: Endpoint : api/approval/requests/{apprReqID} , Action :: Get Message has recevied, Response :: Approval Query Message ::" + JsonConvert.SerializeObject(ObjApprovalQuery));
                    Task.Factory.StartNew(() =>
                    {
                        ApprovalBL objappr = new ApprovalBL();
                        objappr.UpdateApprovalObject(ObjApprovalQuery);

                    });
                    InsightLogger.TrackEvent("adidas.clb.MobileApproval:: ApprovalAPIController :: Endpoint : api/approval/requests/{apprReqID} , Action :: return Acknowledgement message, Response :: true ,TimeStamp :: " + DateTime.UtcNow);
                    //return Response message success status code 
                    return Request.CreateResponse(HttpStatusCode.OK);

                }
                else
                {
                    InsightLogger.TrackEvent("adidas.clb.MobileApproval:: ApprovalAPIController :: Endpoint : api/approval/requests/{apprReqID} , Action :: Get Message has recevied, Response :: Approval Query is Invalid, Message ::" + JsonConvert.SerializeObject(ObjApprovalQuery));
                    //if model is not valid which means it doesn't contains mandatory fields return error message
                    return Request.CreateResponse(HttpStatusCode.OK, DataProvider.ApprovalResponseError<ApprovalResponse>("400", "Approval Query is Invalid", ""));
                }

            }
            catch (DataAccessException dalexception)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.ApprovalResponseError<ApprovalResponse>("400", dalexception.Message, dalexception.Message));
            }
            catch (BusinessLogicException blexception)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.ApprovalResponseError<ApprovalResponse>("400", blexception.Message, blexception.Message));
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserBackendDTO>("400", exception.Message, exception.StackTrace));
            }
        }
    }
}
