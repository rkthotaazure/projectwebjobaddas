using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adidas.clb.job.GeneratePDF.Models
{
    public class RequestPdf
    {
        public string _type { get; set; }
        public string RequestID { get; set; }
        public string BackendID { get; set; }
        public RequestPdf()
        {
            _type = "requestPdf";
        }
    }
    public class RequestPDFAddress
    {
        public string _type { get; set; }
        public string PDF_URL { get; set; }
        public string RequestID { get; set; }
        public RequestPDFAddress() { _type = "requestPDFAddress"; }
    }
}
