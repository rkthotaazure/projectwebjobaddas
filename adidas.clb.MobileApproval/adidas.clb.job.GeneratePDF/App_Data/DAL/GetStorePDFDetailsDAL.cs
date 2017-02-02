//-----------------------------------------------------------
// <copyright file="GetStorePDFDetailsDAL.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using adidas.clb.job.GeneratePDF.Models;
using adidas.clb.job.GeneratePDF.Exceptions;
using adidas.clb.job.GeneratePDF.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace adidas.clb.job.GeneratePDF.App_Data.DAL
{
    class GetStorePDFDetailsDAL
    {
        //Application insights interface reference for logging the error details into Application Insight azure service.
        static IAppInsight InsightLogger { get { return AppInsightLogger.Instance; } }
        private SQLDataProvider sqldataProvider;
        public GetStorePDFDetailsDAL()
        {
            sqldataProvider = new SQLDataProvider();
        }
        /// <summary>
        /// This method returns the pdf details of store from stored procedure based on requestID
        /// </summary>
        /// <param name="requestID"></param>
        /// <returns></returns>
        public StoreApprovalModel GetPDFDetailsFromStore(string requestID,string backendID)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                StoreApprovalModel ObjSA = null;               
                // Create an array list for storing parameters
                ArrayList parameterArray = new ArrayList();
                // Initialize a new sql parameter list
                List<SqlParameter> sqlParameters = new List<SqlParameter>();
                // Initialize a new sql parameters
                SqlParameter pRequestID = new SqlParameter("@BPMID", requestID);
                pRequestID.Direction = ParameterDirection.Input;
                // Adding all sql parameters to sql parameter array
                sqlParameters.Add(pRequestID);
                // Open a new SQL Connection              
                sqldataProvider.OpenConnection(ConfigurationManager.ConnectionStrings["BakendAgentStoreConnectionString"].ConnectionString);
                // Execute stored procedure for fetching PDf details from STORE Backend.
                DataSet dsPdfDetails = (DataSet)sqldataProvider.ExecuteDataSet(CommandType.StoredProcedure, ConfigurationManager.AppSettings["GetStorePDFDetailsSPName"], sqlParameters.ToArray());
                //StoreApprovalBasicInfo objSAbasicInfo = null;
                Dictionary<string, object> objReqDetails = null;
                //stored procedure returns two result sets
                if (dsPdfDetails != null && dsPdfDetails.Tables.Count > 0)
                {
                    FieldsMapping objFieldsMap = new FieldsMapping();
                    //call FieldsMapping.GetFiledsMappingJsonString() method which will return  RequestFieldsMappingJson,ApproverFieldsMappingJson 
                    MappingTypes ObjMaps = objFieldsMap.GetFiledsMappingJsonString(backendID);
                    //getting StoreApprovalBasicInfo details from dataset table 1
                    if (dsPdfDetails.Tables[0] != null && dsPdfDetails.Tables[0].Rows.Count > 0)
                    {
                        DataRow row = dsPdfDetails.Tables[0].Rows[0];

                        //Call FieldsMapping.MapDtFieldstoBackendRequest() method, which returns Mapping request details to RequestDetailsMapping class object
                        //StoreApprovalBasicInfo objReqDetails = objFieldsMap.MapDtFieldstoBackendRequest<StoreApprovalBasicInfo>(row, ObjMaps.RequestFieldsMappingJson);
                        //objSAbasicInfo = new StoreApprovalBasicInfo()
                        //{
                        //    DisplayBPMID = (!string.IsNullOrEmpty(objReqDetails.DisplayBPMID)) ? objReqDetails.DisplayBPMID : string.Empty,
                        //    RequestName = (!string.IsNullOrEmpty(objReqDetails.RequestName)) ? objReqDetails.RequestName : string.Empty,
                        //    MarketName = (!string.IsNullOrEmpty(objReqDetails.MarketName)) ? objReqDetails.MarketName : string.Empty,
                        //    ProjectName = (!string.IsNullOrEmpty(objReqDetails.ProjectName)) ? objReqDetails.ProjectName : string.Empty,
                        //    BrandName = (!string.IsNullOrEmpty(objReqDetails.BrandName)) ? objReqDetails.BrandName : string.Empty,
                        //    SecurityDeposit = (!string.IsNullOrEmpty(Convert.ToString(objReqDetails.SecurityDeposit))) ? objReqDetails.SecurityDeposit : 0,
                        //    TotalInvestment = (!string.IsNullOrEmpty(Convert.ToString(objReqDetails.TotalInvestment))) ? objReqDetails.TotalInvestment : 0,
                        //    KeyMoney = (!string.IsNullOrEmpty(Convert.ToString(objReqDetails.KeyMoney))) ? objReqDetails.KeyMoney : 0,
                        //    Brand = (!string.IsNullOrEmpty(Convert.ToString(objReqDetails.Brand))) ? Convert.ToDecimal(objReqDetails.Brand) : 0,
                        //    CaseID = (!string.IsNullOrEmpty(Convert.ToString(objReqDetails.CaseID))) ? Convert.ToInt32(objReqDetails.CaseID) : 0,
                        //    StoreTypeName = (!string.IsNullOrEmpty(objReqDetails.StoreTypeName)) ? objReqDetails.StoreTypeName : string.Empty,
                        //    NetSellingSpace = (!string.IsNullOrEmpty(Convert.ToString(objReqDetails.NetSellingSpace))) ? Convert.ToInt32(objReqDetails.NetSellingSpace) : 0,
                        //    OpeningDate = (!string.IsNullOrEmpty(Convert.ToString(objReqDetails.OpeningDate))) ? Convert.ToDateTime(objReqDetails.OpeningDate) : (DateTime?)null,
                        //    LeaseEndDate = (!string.IsNullOrEmpty(Convert.ToString(objReqDetails.LeaseEndDate))) ? Convert.ToDateTime(objReqDetails.LeaseEndDate) : (DateTime?)null,
                        //    LeasingPeriodDec = (!string.IsNullOrEmpty(Convert.ToString(objReqDetails.LeasingPeriodDec))) ? Convert.ToDecimal(objReqDetails.LeasingPeriodDec) : 0,
                        //    CancelPeriod = (!string.IsNullOrEmpty(Convert.ToString(objReqDetails.CancelPeriod))) ? Convert.ToInt32(objReqDetails.CancelPeriod) : 0,
                        //    LeaseBreakOption = (!string.IsNullOrEmpty(Convert.ToString(objReqDetails.LeaseBreakOption))) ? Convert.ToInt32(objReqDetails.LeaseBreakOption) : 0,
                        //    CapexSpendYear = (!string.IsNullOrEmpty(Convert.ToString(objReqDetails.CapexSpendYear))) ? Convert.ToInt32(objReqDetails.CapexSpendYear) : 0,
                        //    GrossLeasedArea = (!string.IsNullOrEmpty(Convert.ToString(objReqDetails.GrossLeasedArea))) ? Convert.ToInt32(objReqDetails.GrossLeasedArea) : 0
                        //};
                         objReqDetails = objFieldsMap.MapDBFieldstoBackendRequest(row, ObjMaps.RequestFieldsMappingJson);
                    }
                   //getting StoreExecutiveSummary details from dataset table 2

                    List<StoreExecutiveSummary> lstSASummary = null;
                    if (dsPdfDetails.Tables[1] != null && dsPdfDetails.Tables[1].Rows.Count > 0)
                    {
                        //call FieldsMapping.MapApproverFieldstoBackendRequest() method, which returns list of approver details after Mapping into ApproverFeildsMapping class object
                        lstSASummary = objFieldsMap.MapApproverFieldstoBackendRequest<StoreExecutiveSummary>(dsPdfDetails.Tables[1], ObjMaps.MatrixFieldsMappingJson);


                        //lstSASummary = dsPdfDetails.Tables[1].AsEnumerable().Select(row =>
                        //                                    new StoreExecutiveSummary
                        //                                    {
                        //                                        LineID = row.Field<string>("LineID"),
                        //                                        Description = row.Field<string>("Description"),
                        //                                        Y0 = row.Field<string>("Y0"),
                        //                                        Y0Val = row.Field<string>("Y0Val"),
                        //                                        Y1 = row.Field<string>("Y1"),
                        //                                        Y1Val = row.Field<string>("Y1Val"),
                        //                                        Y2 = row.Field<string>("Y2"),
                        //                                        Y2Val = row.Field<string>("Y2Val"),
                        //                                        Y3 = row.Field<string>("Y3"),
                        //                                        Y3Val = row.Field<string>("Y3Val"),
                        //                                        Y4 = row.Field<string>("Y4"),
                        //                                        Y4Val = row.Field<string>("Y4Val"),
                        //                                        Y5 = row.Field<string>("Y5"),
                        //                                        Y5Val = row.Field<string>("Y5Val"),
                        //                                        GRSVal = row.Field<string>("GRSVal"),
                        //                                    }).ToList();
                    }
                    //getting store pdf details into StoreApprovalModel class
                    ObjSA = new StoreApprovalModel()
                    {
                        StoreBasicInformation = objReqDetails,
                        StoreSummaryDetails = lstSASummary
                    };
                }
                return ObjSA;

            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                throw new DataAccessException(exception.Message, exception.InnerException);
            }

        }
    }
}
