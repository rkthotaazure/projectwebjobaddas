using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace adidas.clb.MobileApproval.Controllers
{
    public class SyncAPIController : ApiController
    {
        // GET: api/SyncAPI
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/SyncAPI/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/SyncAPI
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/SyncAPI/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/SyncAPI/5
        public void Delete(int id)
        {
        }
    }
}
