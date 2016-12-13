//-----------------------------------------------------------
// <copyright file="GetCarPDFDetailsDAL.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using adidas.clb.job.UpdateTriggering.Models;
using adidas.clb.job.UpdateTriggering.Exceptions;
using adidas.clb.job.UpdateTriggering.Utility;
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

namespace adidas.clb.job.UpdateTriggering.App_Data.DAL
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
        public CarApprovalModel GetPDFDetailsFromCAR(string requestID)
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
                    //getting CARApprovalBasicInfo details from dataset table 1
                    if (dsPdfDetails.Tables[0] != null && dsPdfDetails.Tables[0].Rows.Count > 0)
                    {
                        DataRow row = dsPdfDetails.Tables[0].Rows[0];
                        objCarbasicInfo = new CarSummary()
                        {
                            Name = (!row.IsNull("Name")) ? Convert.ToString(row["Name"]) : string.Empty,
                            Description = (!row.IsNull("Description")) ? Convert.ToString(row["Description"]) : string.Empty,
                            Controller = (!row.IsNull("Controller") && !string.IsNullOrEmpty(Convert.ToString(row["Controller"]))) ? Convert.ToInt32(row["Controller"]) : 0,
                            Requestor = (!row.IsNull("Requestor") && !string.IsNullOrEmpty(Convert.ToString(row["Requestor"]))) ? Convert.ToInt32(row["Requestor"]) : 0,
                            DateofRequest= (!row.IsNull("DateofRequest") && !string.IsNullOrEmpty(Convert.ToString(row["DateofRequest"]))) ? Convert.ToDateTime(row["DateofRequest"]) : (DateTime?)null,
                            BrandDescription = (!row.IsNull("BrandDescription")) ? Convert.ToString(row["BrandDescription"]) : string.Empty,
                            CountryDescription = (!row.IsNull("CountryDescription")) ? Convert.ToString(row["CountryDescription"]) : string.Empty,
                            MarketDescription = (!row.IsNull("MarketDescription")) ? Convert.ToString(row["MarketDescription"]) : string.Empty,
                            InvestmentTypeDescription = (!row.IsNull("InvestmentTypeDescription")) ? Convert.ToString(row["InvestmentTypeDescription"]) : string.Empty,
                            EstimatedStartDate = (!row.IsNull("EstimatedStartDate") && !string.IsNullOrEmpty(Convert.ToString(row["EstimatedStartDate"]))) ? Convert.ToDateTime(row["EstimatedStartDate"]) : (DateTime?)null,
                            EstimatedCompletionDate = (!row.IsNull("EstimatedCompletionDate") && !string.IsNullOrEmpty(Convert.ToString(row["EstimatedCompletionDate"]))) ? Convert.ToDateTime(row["EstimatedCompletionDate"]) : (DateTime?)null,
                            Budgeted= (!row.IsNull("Budgeted") && !string.IsNullOrEmpty(Convert.ToString(row["Budgeted"]))) ? Convert.ToBoolean(Convert.ToInt32(row["Budgeted"])) : false,
                            Capex = (!row.IsNull("Capex") && !string.IsNullOrEmpty(Convert.ToString(row["Capex"]))) ? Convert.ToDecimal(row["Capex"]) : 0,
                            CapexLocal = (!row.IsNull("CapexLocal") && !string.IsNullOrEmpty(Convert.ToString(row["CapexLocal"]))) ? Convert.ToDecimal(row["CapexLocal"]) : 0,
                            SpenttodateEUR = (!row.IsNull("SpenttodateEUR") && !string.IsNullOrEmpty(Convert.ToString(row["SpenttodateEUR"]))) ? Convert.ToDecimal(row["SpenttodateEUR"]) : 0,
                            CAPEXThisRequest = (!row.IsNull("CAPEXthisrequest") && !string.IsNullOrEmpty(Convert.ToString(row["CAPEXthisrequest"]))) ? Convert.ToDecimal(row["CAPEXthisrequest"]) : 0,
                            LocalCurency = (!row.IsNull("LocalCurency") && !string.IsNullOrEmpty(Convert.ToString(row["LocalCurency"]))) ? Convert.ToInt32(row["LocalCurency"]) : 0,
                            AssetNo = (!row.IsNull("AssetNo")) ? Convert.ToString(row["AssetNo"]) : string.Empty,
                            CostCenterInternalOrder = (!row.IsNull("CostCenterInternalOrder")) ? Convert.ToString(row["CostCenterInternalOrder"]) : string.Empty,
                            IMSNumber = (!row.IsNull("IMSNumber") && !string.IsNullOrEmpty(Convert.ToString(row["IMSNumber"]))) ? Convert.ToInt32(row["IMSNumber"]) : 0,
                            CAPEXCodeGrape = (!row.IsNull("CAPEXCodeGrape")) ? Convert.ToString(row["CAPEXCodeGrape"]) : string.Empty,
                            FinanceLease = (!row.IsNull("FinanceLease") && !string.IsNullOrEmpty(Convert.ToString(row["FinanceLease"]))) ? Convert.ToBoolean(Convert.ToInt32(row["FinanceLease"])) : false,
                            ContractualObligation = (!row.IsNull("ContractualObligation") && !string.IsNullOrEmpty(Convert.ToString(row["ContractualObligation"]))) ? Convert.ToBoolean(Convert.ToInt32(row["ContractualObligation"])) : false,
                            PurchaseOption = (!row.IsNull("PurchaseOption") && !string.IsNullOrEmpty(Convert.ToString(row["PurchaseOption"]))) ? Convert.ToBoolean(Convert.ToInt32(row["PurchaseOption"])) : false,
                            GlobalRealEstate = (!row.IsNull("Budgeted") && !string.IsNullOrEmpty(Convert.ToString(row["GlobalRealEstate"]))) ? Convert.ToBoolean(Convert.ToInt32(row["GlobalRealEstate"])) : false,
                            NoOfYears = (!row.IsNull("NoOfYears") && !string.IsNullOrEmpty(Convert.ToString(row["NoOfYears"]))) ? Convert.ToInt32(row["NoOfYears"]) : 0,


                        };

                    }
                    //getting CarCapexMatrix details from dataset table 2
                    //writing it into CarCapexMatrixlist
                    List<CarCapexMatrix> lstCarCapexmatrix = null;
                    if (dsPdfDetails.Tables[1] != null && dsPdfDetails.Tables[1].Rows.Count > 0)
                    {
                        lstCarCapexmatrix = dsPdfDetails.Tables[1].AsEnumerable().Select(row =>
                                                            new CarCapexMatrix
                                                            {
                                                                CapexMatricDescription = row.Field<string>("Description"),                                                               
                                                                Y1 = row.Field<decimal>("Y1"),                                                               
                                                                Y2 = row.Field<decimal>("Y2"),                                                               
                                                                Y3 = row.Field<decimal>("Y3"),
                                                                TotalSum = row.Field<decimal>("TotalSum")                                                             
                                                            }).ToList();
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
