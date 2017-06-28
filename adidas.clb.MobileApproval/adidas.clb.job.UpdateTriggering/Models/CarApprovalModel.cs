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

namespace adidas.clb.job.UpdateTriggering.Models
{
    /// <summary>
    /// This class defines  model for CAR pdf details obj.
    /// </summary>
    class CarApprovalModel
    {
        public CarSummary CarBasicInformation { get; set; }
        public IEnumerable<CarCapexMatrix> CarCapexMatrixDetails { get; set; }
    }
    class CarSummary
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Requestor { get; set; }
        public int Controller { get; set; }
        public Nullable<DateTime> DateofRequest { get; set; }
        public string BrandDescription { get; set; }
        public string CountryDescription { get; set; }
        public string MarketDescription { get; set; }
        public string InvestmentTypeDescription { get; set; }
        public Nullable<DateTime> EstimatedStartDate { get; set; }
        public Nullable<DateTime> EstimatedCompletionDate { get; set; }
        public bool Budgeted { get; set; }
        public decimal Capex { get; set; }
        public decimal CapexLocal { get; set; }
        public int LocalCurency { get; set; }
        public decimal SpenttodateEUR { get; set; }
        public decimal CAPEXThisRequest { get; set; }
        public string AssetNo { get; set; }
        public string CostCenterInternalOrder { get; set; }
        public int IMSNumber { get; set; }
        public string CAPEXCodeGrape { get; set; }
        public bool FinanceLease { get; set; }
        public bool ContractualObligation { get; set; }
        public bool PurchaseOption { get; set; }
        public bool GlobalRealEstate { get; set; }
        public int NoOfYears { get; set; }

    }
    class CarCapexMatrix
    {
        public string CapexMatricDescription { get; set; }
        public decimal Y1 { get; set; }
        public decimal Y2 { get; set; }
        public decimal Y3 { get; set; }
        public decimal TotalSum { get; set; }
    }
}
