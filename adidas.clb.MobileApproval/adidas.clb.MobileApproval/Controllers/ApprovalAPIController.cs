//-----------------------------------------------------------
// <copyright file="ApprovalAPIController.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using adidas.clb.MobileApproval.Utility;
using log4net;

namespace adidas.clb.MobileApproval.Controllers
{
    [Authorize]
    public class ApprovalAPIController : ApiController
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        // GET: api/ApprovalAPI
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/ApprovalAPI/5
        public string Get(int id)
        {
            try
            {
                throw new FileNotFoundException();
            }
            catch(Exception exception) {

                LoggerHelper.WriteToLog(exception + " - Error while creating client context : "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
            }
            return "value";
        }

        // POST: api/ApprovalAPI
        public void Post([FromBody]string value)
        {

        }

        // PUT: api/ApprovalAPI/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/ApprovalAPI/5
        public void Delete(int id)
        {
        }
    }
}
