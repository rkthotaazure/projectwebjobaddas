using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
namespace adidas.clb.MobileApprovalUI.Models
{
    public class ApprovalQuery
    {
        public string _type { get; set; }
        //Mandatory Field
        [Required]
        public string ApprovalRequesID { get; set; }
        //Mandatory Field
        [Required]
        public string UserID { get; set; }
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

}