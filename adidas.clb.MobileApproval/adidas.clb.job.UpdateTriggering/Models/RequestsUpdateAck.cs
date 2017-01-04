using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adidas.clb.job.UpdateTriggering.Models
{
    public class RequestsUpdateAck
    {
        public string _type { get; set; }
        public RequestsUpdateQuery Query { set; get; }
        public Error Error { get; set; }
        public RequestsUpdateAck()
        {
            _type = "requestsUpdateAck";
        }
    }
}
