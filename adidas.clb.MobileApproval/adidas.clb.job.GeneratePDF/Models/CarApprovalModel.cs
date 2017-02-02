//-----------------------------------------------------------
// <copyright file="CarApprovalModel.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adidas.clb.job.GeneratePDF.Models
{
    /// <summary>
    /// class which implements model for CAR pdf details obj.
    /// </summary>
    class CarApprovalModel
    {
        public Dictionary<string, object> CarBasicInformation { get; set; }
        public IEnumerable<CarCapexMatrix> CarCapexMatrixDetails { get; set; }
    }
    class CarSummary
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Nullable<int> Requestor { get; set; }
        public Nullable<int> Controller { get; set; }
        public Nullable<DateTime> DateofRequest { get; set; }
        public string BrandDescription { get; set; }
        public string CountryDescription { get; set; }
        public string MarketDescription { get; set; }
        public string InvestmentTypeDescription { get; set; }
        public Nullable<DateTime> EstimatedStartDate { get; set; }
        public Nullable<DateTime> EstimatedCompletionDate { get; set; }
        public Nullable<bool> Budgeted { get; set; }
        public Nullable<decimal> Capex { get; set; }
        public Nullable<decimal> CapexLocal { get; set; }
        public Nullable<int> LocalCurency { get; set; }
        public Nullable<decimal> SpenttodateEUR { get; set; }
        public Nullable<decimal> CAPEXThisRequest { get; set; }
        public string AssetNo { get; set; }
        public string CostCenterInternalOrder { get; set; }
        public Nullable<int> IMSNumber { get; set; }
        public string CAPEXCodeGrape { get; set; }
        public Nullable<bool> FinanceLease { get; set; }
        public Nullable<bool> ContractualObligation { get; set; }
        public Nullable<bool> PurchaseOption { get; set; }
        public Nullable<bool> GlobalRealEstate { get; set; }
        public Nullable<int> NoOfYears { get; set; }

    }
    class CarCapexMatrix
    {
        public string CapexMatricDescription { get; set; }
        public Nullable<decimal> Y1 { get; set; }
        public Nullable<decimal> Y2 { get; set; }
        public Nullable<decimal> Y3 { get; set; }
        public Nullable<decimal> TotalSum { get; set; }
    }
}
