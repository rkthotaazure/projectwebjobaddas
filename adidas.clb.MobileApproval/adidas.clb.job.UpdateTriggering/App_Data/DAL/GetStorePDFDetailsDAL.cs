//-----------------------------------------------------------
// <copyright file="GetStorePDFDetailsDAL.cs" company="adidas AG">
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
        public StoreApprovalModel GetPDFDetailsFromStore(string requestID)
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
                StoreApprovalBasicInfo objSAbasicInfo = null;
                //stored procedure returns two result sets
                if (dsPdfDetails != null && dsPdfDetails.Tables.Count > 0)
                {
                    //getting StoreApprovalBasicInfo details from dataset table 1
                    if (dsPdfDetails.Tables[0] != null && dsPdfDetails.Tables[0].Rows.Count > 0)
                    {
                        DataRow row = dsPdfDetails.Tables[0].Rows[0];
                        objSAbasicInfo = new StoreApprovalBasicInfo()
                        {
                            DisplayBPMID = (!row.IsNull("DisplayBPMID")) ? Convert.ToString(row["DisplayBPMID"]) : string.Empty,
                            RequestName = (!row.IsNull("RequestName")) ? Convert.ToString(row["RequestName"]) : string.Empty,
                            MarketName = (!row.IsNull("MarketName")) ? Convert.ToString(row["MarketName"]) : string.Empty,
                            ProjectName = (!row.IsNull("ProjectName")) ? Convert.ToString(row["ProjectName"]) : string.Empty,
                            BrandName = (!row.IsNull("BrandName")) ? Convert.ToString(row["BrandName"]) : string.Empty,
                            SecurityDeposit = (!row.IsNull("SecurityDeposit") && !string.IsNullOrEmpty(Convert.ToString(row["SecurityDeposit"]))) ? Convert.ToDecimal(row["SecurityDeposit"]) : 0,
                            TotalInvestment = (!row.IsNull("TotalInvestment") && !string.IsNullOrEmpty(Convert.ToString(row["TotalInvestment"]))) ? Convert.ToDecimal(row["TotalInvestment"]) : 0,
                            KeyMoney = (!row.IsNull("KeyMoney") && !string.IsNullOrEmpty(Convert.ToString(row["KeyMoney"]))) ? Convert.ToDecimal(row["KeyMoney"]) : 0,
                            Brand = (!row.IsNull("Brand") && !string.IsNullOrEmpty(Convert.ToString(row["Brand"]))) ? Convert.ToDecimal(row["Brand"]) : 0,
                            CaseID = (!row.IsNull("CaseID") && !string.IsNullOrEmpty(Convert.ToString(row["CaseID"]))) ? Convert.ToInt32(row["CaseID"]) : 0,
                            StoreTypeName = (!row.IsNull("StoreTypeName")) ? Convert.ToString(row["StoreTypeName"]) : string.Empty,
                            NetSellingSpace = (!row.IsNull("NetSellingSpace") && !string.IsNullOrEmpty(Convert.ToString(row["NetSellingSpace"]))) ? Convert.ToInt32(row["NetSellingSpace"]) : 0,
                            OpeningDate = (!row.IsNull("OpeningDate") && !string.IsNullOrEmpty(Convert.ToString(row["OpeningDate"]))) ? Convert.ToDateTime(row["OpeningDate"]) : (DateTime?)null,
                            LeaseEndDate = (!row.IsNull("LeaseEndDate") && !string.IsNullOrEmpty(Convert.ToString(row["LeaseEndDate"]))) ? Convert.ToDateTime(row["LeaseEndDate"]) : (DateTime?)null,
                            LeasingPeriodDec = (!row.IsNull("LeasingPeriodDec") && !string.IsNullOrEmpty(Convert.ToString(row["LeasingPeriodDec"]))) ? Convert.ToDecimal(row["LeasingPeriodDec"]) : 0,
                            CancelPeriod = (!row.IsNull("CancelPeriod") && !string.IsNullOrEmpty(Convert.ToString(row["CancelPeriod"]))) ? Convert.ToInt32(row["CancelPeriod"]) : 0,
                            LeaseBreakOption = (!row.IsNull("LeaseBreakOption") && !string.IsNullOrEmpty(Convert.ToString(row["LeaseBreakOption"]))) ? Convert.ToInt32(row["LeaseBreakOption"]) : 0,
                            CapexSpendYear = (!row.IsNull("CapexSpendYear") && !string.IsNullOrEmpty(Convert.ToString(row["CapexSpendYear"]))) ? Convert.ToInt32(row["CapexSpendYear"]) : 0,
                            GrossLeasedArea = (!row.IsNull("GrossLeasedArea") && !string.IsNullOrEmpty(Convert.ToString(row["GrossLeasedArea"]))) ? Convert.ToInt32(row["GrossLeasedArea"]) : 0
                        };

                    }
                    //getting StoreExecutiveSummary details from dataset table 2
                    List<StoreExecutiveSummary> lstSASummary = null;
                    if (dsPdfDetails.Tables[1] != null && dsPdfDetails.Tables[1].Rows.Count > 0)
                    {
                        lstSASummary = dsPdfDetails.Tables[1].AsEnumerable().Select(row =>
                                                            new StoreExecutiveSummary
                                                            {
                                                                LineID = row.Field<string>("LineID"),
                                                                Description = row.Field<string>("Description"),
                                                                Y0 = row.Field<string>("Y0"),
                                                                Y0Val = row.Field<string>("Y0Val"),
                                                                Y1 = row.Field<string>("Y1"),
                                                                Y1Val = row.Field<string>("Y1Val"),
                                                                Y2 = row.Field<string>("Y2"),
                                                                Y2Val = row.Field<string>("Y2Val"),
                                                                Y3 = row.Field<string>("Y3"),
                                                                Y3Val = row.Field<string>("Y3Val"),
                                                                Y4 = row.Field<string>("Y4"),
                                                                Y4Val = row.Field<string>("Y4Val"),
                                                                Y5 = row.Field<string>("Y5"),
                                                                Y5Val = row.Field<string>("Y5Val"),
                                                                GRSVal = row.Field<string>("GRSVal"),
                                                            }).ToList();
                    }
                    //getting store pdf details into StoreApprovalModel class
                    ObjSA = new StoreApprovalModel()
                    {
                        StoreBasicInformation = objSAbasicInfo,
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
