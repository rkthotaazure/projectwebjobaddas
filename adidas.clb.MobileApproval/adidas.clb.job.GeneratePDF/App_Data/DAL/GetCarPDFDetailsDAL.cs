//-----------------------------------------------------------
// <copyright file="GetCarPDFDetailsDAL.cs" company="adidas AG">
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
    class GetCarPDFDetailsDAL
    {
        //Application insights interface reference for logging the error details into Application Insight azure service.
        static IAppInsight InsightLogger { get { return AppInsightLogger.Instance; } }
        private SQLDataProvider sqldataProvider;
        public GetCarPDFDetailsDAL()
        {
            sqldataProvider = new SQLDataProvider();
        }
        /// <summary>
        /// This method returns the pdf details of car from stored procedure based on requestID
        /// </summary>
        /// <param name="requestID"></param>
        /// <returns></returns>
        public CarApprovalModel GetPDFDetailsFromCAR(string requestID,string backendID)
        {

            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                CarApprovalModel ObjSA = null;
                // Create an array list for storing parameters
                ArrayList parameterArray = new ArrayList();
                // Initialize a new sql parameter list
                List<SqlParameter> sqlParameters = new List<SqlParameter>();
                // Initialize a new sql parameters
                SqlParameter pRequestID = new SqlParameter("@RequestID", requestID);
                pRequestID.Direction = ParameterDirection.Input;
                // Adding all sql parameters to sql parameter array
                sqlParameters.Add(pRequestID);
                // Open a new SQL Connection              
                sqldataProvider.OpenConnection(ConfigurationManager.ConnectionStrings["BakendAgentCarConnectionString"].ConnectionString);
                // Execute stored procedure for fetching PDf details from CAR Backend.
                DataSet dsPdfDetails = (DataSet)sqldataProvider.ExecuteDataSet(CommandType.StoredProcedure, ConfigurationManager.AppSettings["GetCarPDFDetailsSPName"], sqlParameters.ToArray());
                CarSummary objCarbasicInfo = null;
                //stored procedure returns two result sets
                if (dsPdfDetails != null && dsPdfDetails.Tables.Count > 0)
                {
                    FieldsMapping objFieldsMap = new FieldsMapping();
                    //call FieldsMapping.GetFiledsMappingJsonString() method which will return  RequestFieldsMappingJson,ApproverFieldsMappingJson 
                    MappingTypes ObjMaps = objFieldsMap.GetFiledsMappingJsonString(backendID);
                    //getting CARApprovalBasicInfo details from dataset table 1
                    if (dsPdfDetails.Tables[0] != null && dsPdfDetails.Tables[0].Rows.Count > 0)
                    {
                        DataRow row = dsPdfDetails.Tables[0].Rows[0];
                        //Call FieldsMapping.MapDtFieldstoBackendRequest() method, which returns Mapping request details to RequestDetailsMapping class object
                        CarSummary objReqDetails = objFieldsMap.MapDtFieldstoBackendRequest<CarSummary>(row, ObjMaps.RequestFieldsMappingJson);
                        objCarbasicInfo = new CarSummary()
                        {
                            Name = (!string.IsNullOrEmpty(objReqDetails.Name)) ? objReqDetails.Name : string.Empty,
                            Description = (!string.IsNullOrEmpty(objReqDetails.Description)) ? objReqDetails.Description : string.Empty,
                            Controller = (!string.IsNullOrEmpty(Convert.ToString(objReqDetails.Controller))) ? Convert.ToInt32(objReqDetails.Controller) : 0,
                            Requestor = (!string.IsNullOrEmpty(Convert.ToString(objReqDetails.Requestor))) ? Convert.ToInt32(objReqDetails.Requestor) : 0,
                            DateofRequest= (!string.IsNullOrEmpty(Convert.ToString(objReqDetails.DateofRequest))) ? Convert.ToDateTime(objReqDetails.DateofRequest) : (DateTime?)null,
                            BrandDescription = (!string.IsNullOrEmpty(objReqDetails.BrandDescription)) ? objReqDetails.BrandDescription : string.Empty,
                            CountryDescription = (!string.IsNullOrEmpty(objReqDetails.CountryDescription)) ? objReqDetails.CountryDescription : string.Empty,
                            MarketDescription = (!string.IsNullOrEmpty(objReqDetails.MarketDescription)) ? objReqDetails.MarketDescription : string.Empty,
                            InvestmentTypeDescription = (!string.IsNullOrEmpty(objReqDetails.InvestmentTypeDescription)) ? objReqDetails.InvestmentTypeDescription : string.Empty,
                            EstimatedStartDate = (!string.IsNullOrEmpty(Convert.ToString(objReqDetails.EstimatedStartDate))) ? Convert.ToDateTime(objReqDetails.EstimatedStartDate) : (DateTime?)null,
                            EstimatedCompletionDate = (!string.IsNullOrEmpty(Convert.ToString(objReqDetails.EstimatedCompletionDate))) ? Convert.ToDateTime(objReqDetails.EstimatedCompletionDate) : (DateTime?)null,
                            Budgeted= (!string.IsNullOrEmpty(Convert.ToString(objReqDetails.Budgeted))) ? Convert.ToBoolean(Convert.ToInt32(objReqDetails.Budgeted)) : false,
                            Capex = (!string.IsNullOrEmpty(Convert.ToString(objReqDetails.Capex))) ? Convert.ToDecimal(objReqDetails.Capex) : 0,
                            CapexLocal = (!string.IsNullOrEmpty(Convert.ToString(objReqDetails.CapexLocal))) ? Convert.ToDecimal(objReqDetails.CapexLocal) : 0,
                            SpenttodateEUR = (!string.IsNullOrEmpty(Convert.ToString(objReqDetails.SpenttodateEUR))) ? Convert.ToDecimal(objReqDetails.SpenttodateEUR) : 0,
                            CAPEXThisRequest = (!string.IsNullOrEmpty(Convert.ToString(objReqDetails.CAPEXThisRequest))) ? Convert.ToDecimal(objReqDetails.CAPEXThisRequest) : 0,
                            LocalCurency = (!string.IsNullOrEmpty(Convert.ToString(objReqDetails.LocalCurency))) ? Convert.ToInt32(objReqDetails.LocalCurency) : 0,
                            AssetNo = (!string.IsNullOrEmpty(objReqDetails.AssetNo)) ? Convert.ToString(objReqDetails.AssetNo) : string.Empty,
                            CostCenterInternalOrder = (!string.IsNullOrEmpty(objReqDetails.CostCenterInternalOrder)) ? Convert.ToString(objReqDetails.CostCenterInternalOrder) : string.Empty,
                            IMSNumber = (!string.IsNullOrEmpty(Convert.ToString(objReqDetails.IMSNumber))) ? Convert.ToInt32(objReqDetails.IMSNumber) : 0,
                            CAPEXCodeGrape = (!string.IsNullOrEmpty(objReqDetails.CAPEXCodeGrape))?objReqDetails.CAPEXCodeGrape : string.Empty,
                            FinanceLease = (!string.IsNullOrEmpty(Convert.ToString(objReqDetails.FinanceLease))) ? Convert.ToBoolean(Convert.ToInt32(objReqDetails.FinanceLease)) : false,
                            ContractualObligation = (!string.IsNullOrEmpty(Convert.ToString(objReqDetails.ContractualObligation))) ? Convert.ToBoolean(Convert.ToInt32(objReqDetails.ContractualObligation)) : false,
                            PurchaseOption = (!string.IsNullOrEmpty(Convert.ToString(objReqDetails.PurchaseOption))) ? Convert.ToBoolean(Convert.ToInt32(objReqDetails.PurchaseOption)) : false,
                            GlobalRealEstate = (!string.IsNullOrEmpty(Convert.ToString(objReqDetails.GlobalRealEstate))) ? Convert.ToBoolean(Convert.ToInt32(objReqDetails.GlobalRealEstate)) : false,
                            NoOfYears = (!string.IsNullOrEmpty(Convert.ToString(objReqDetails.NoOfYears))) ? Convert.ToInt32(objReqDetails.NoOfYears) : 0,


                        };

                    }
                    //getting CarCapexMatrix details from dataset table 2
                    //writing it into CarCapexMatrixlist
                    List<CarCapexMatrix> lstCarCapexmatrix = null;
                    if (dsPdfDetails.Tables[1] != null && dsPdfDetails.Tables[1].Rows.Count > 0)
                    {
                        //call FieldsMapping.MapApproverFieldstoBackendRequest() method, which returns list of approver details after Mapping into ApproverFeildsMapping class object
                        lstCarCapexmatrix = objFieldsMap.MapApproverFieldstoBackendRequest<CarCapexMatrix>(dsPdfDetails.Tables[1], ObjMaps.MatrixFieldsMappingJson);

                        //lstCarCapexmatrix = dsPdfDetails.Tables[1].AsEnumerable().Select(row =>
                        //                                    new CarCapexMatrix
                        //                                    {
                        //                                        CapexMatricDescription = row.Field<string>("Description"),                                                               
                        //                                        Y1 = row.Field<decimal>("Y1"),                                                               
                        //                                        Y2 = row.Field<decimal>("Y2"),                                                               
                        //                                        Y3 = row.Field<decimal>("Y3"),
                        //                                        TotalSum = row.Field<decimal>("TotalSum")                                                             
                        //                                    }).ToList();
                    }
                    //clone CAR pdf details into CarApprovalModel class
                    ObjSA = new CarApprovalModel()
                    {
                        CarBasicInformation = objCarbasicInfo,
                        CarCapexMatrixDetails = lstCarCapexmatrix
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
