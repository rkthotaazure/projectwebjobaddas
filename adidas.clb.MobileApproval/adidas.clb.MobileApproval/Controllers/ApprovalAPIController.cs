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
using System.Web.Http;
using log4net;
using adidas.clb.MobileApproval.App_Code.BL.Approval;

namespace adidas.clb.MobileApproval.Controllers
{
    //[Authorize]
    public class ApprovalAPIController : ApiController
    {
        // POST: api/ApprovalAPI
        [Route("api/approval/requests/{apprReqID}")]
        public HttpResponseMessage Post(ApprovalQuery ObjApprovalQuery)
        {
            try
            {
                string response = null;               
                //Checking ApprovalQuery object is valid or not
                if (ModelState.IsValid)
                {
                    ApprovalBL objappr = new ApprovalBL();
                    //updating the service layer approval object
                    response = objappr.UpdateApprovalObject(ObjApprovalQuery);
                    if (!string.IsNullOrEmpty(response))
                    {
                        //return Response message success status code 
                        return Request.CreateResponse(HttpStatusCode.OK);
                    }
                    else
                    {
                        //return error message
                        return Request.CreateResponse(HttpStatusCode.OK, DataProvider.ApprovalResponseError<ApprovalResponse>("400", "Response return error", ""));
                    }
                }
                else
                {
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
                LoggerHelper.WriteToLog(exception + " - exception while updating the service layer approval object in ApprovalAPIController: Post Method()"
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserBackendDTO>("400", exception.Message, exception.StackTrace));
            }
        }
    }
}
