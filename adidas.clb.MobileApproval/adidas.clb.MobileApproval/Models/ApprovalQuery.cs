using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
namespace adidas.clb.MobileApproval.Models
{    
    public class ApprovalQuery
    {
        public string _type { get; set; }
        //Mandatory Field
        [Required]
        public string ApprovalRequestID { get; set; }
        //Mandatory Field
        [Required]
        public string UserID { get; set; }
        public string BackendID { get; set; }
        public string Domain { get; set; }
        //Mandatory Field
        [Required]
        public Decision ApprovalDecision { get; set; }
        public string DeviceID { get; set; }
        public ApprovalQuery()
        {
            _type = "approvalQuery";
        }

    }    
    public class Decision
    {
        public DateTime DecisionDate { get; set; }
        public string Comment { get; set; }
        public string Status { get; set; }
    }
    
    public class ApprovalResponse
    {
        public string _type { get; set; }
        public ApprovalQuery Query { get; set; }
        public ErrorDTO Error { get; set; }
        public ApprovalEntity Result { get; set; }
        public ApprovalResponse()
        {
            _type = "approvalResponse";
        }
    }
    public class ApprovalResponseDTO<T>
    {
        public string _type { get; set; }
        public ApprovalQuery query { get; set; }
        public T result { get; set; }
        public ErrorDTO error { get; set; }
        public ApprovalResponseDTO()
        {
            _type = "approvalResponse";
        }
    }
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

    public class RequestsUpdateQuery
    {
        public string _type { get; set; }
        public BackendUser User { get; set; }
        public IEnumerable<RequestUpdateMsg> Requests { get; set; }
        public bool VIP { get; set; }
        public bool GetPDFs { get; set; }
        public Nullable<DateTime> ChangeAfter { get; set; }
        public string BackendID { get; set; }
        public RequestsUpdateQuery()
        {
            _type = "requestsUpdateQuery";
        }
    }
    public class Error
    {
        public string _type { get; set; }
        public string code { get; set; }
        public string shorttext { get; set; }
        public string longtext { get; set; }
        public Error() { _type = "error"; }
        public Error(string code, string shorttext, string longtext)
        {
            this.code = code;
            this.shorttext = shorttext;
            this.longtext = longtext;
            this._type = "error";
        }
    }
}