//-----------------------------------------------------------
// <copyright file="GeneratePDF.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using adidas.clb.job.UpdateTriggering.App_Data.DAL;
using adidas.clb.job.UpdateTriggering.Helpers;
using adidas.clb.job.UpdateTriggering.Models;
using adidas.clb.job.UpdateTriggering.Exceptions;
using adidas.clb.job.UpdateTriggering.Utility;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.html.simpleparser;
namespace adidas.clb.job.UpdateTriggering.App_Data.BAL
{
    class GeneratePDF
    {
        static IAppInsight InsightLogger { get { return AppInsightLogger.Instance; } }
        //Variable for GetStorePDFDetailsDAL object
        private GetStorePDFDetailsDAL objstorePDF;
        //Variable for GetCarPDFDetailsDAL object
        private GetCarPDFDetailsDAL objCARPDF;
        public GeneratePDF()
        {
            objstorePDF = new GetStorePDFDetailsDAL();
            objCARPDF = new GetCarPDFDetailsDAL();         
        }
        /// <summary>
        /// This method Generates PDf file for store based on requestID
        /// </summary>
        /// <param name="requestID"></param>
        public void GeneratePDFForStoreApproval(string requestID)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                InsightLogger.TrackStartEvent(callerMethodName);
                //read Font name,size,heading summary font dvalues from app.config
                string pdfCellFontName = ConfigurationManager.AppSettings["PDFCellFontName"];
                int pdfCellFontSize = Convert.ToInt32(ConfigurationManager.AppSettings["PDFCellFontSize"]);
                int pdfCellSummaryFontSize = Convert.ToInt32(ConfigurationManager.AppSettings["PDFSummarryCellFontSize"]);
                //Call the stored procedure for gettting the  pdf details from Store backend
                //clone the result set into StoreApprovalModel class
                StoreApprovalModel objStoreDetails = objstorePDF.GetPDFDetailsFromStore(requestID);
                StoreApprovalBasicInfo objstorebasicInfo = null; 
                List<StoreExecutiveSummary> lststoresummary = null;
                //checking StoreApprovalModel has null or not
                if (objStoreDetails != null)
                {
                    //stored procedure returns two result sets
                    //1.Store basic information
                    if (objStoreDetails.StoreBasicInformation != null)
                    {
                        objstorebasicInfo = objStoreDetails.StoreBasicInformation;
                    }
                    //2.Executive summary details
                    if (objStoreDetails.StoreSummaryDetails != null)
                    {
                        lststoresummary = objStoreDetails.StoreSummaryDetails.ToList();
                    }                    
                    
                    //Create folder with requestid name in application Environment.CurrentDirectory and read the folder path
                    string PdfBucketPath = GetPDFPath(requestID);
                    //Pdf file name requestid + yyyyMMddHHmmss
                    string pdfFileName = requestID + DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                    //create pdf filewith pdfFileName
                    System.IO.FileStream fs = new FileStream(PdfBucketPath + "\\" + pdfFileName + ".pdf", FileMode.Create);
                    //define pdf design style
                    Document document = new Document(PageSize.A4, 88f, 88f, 10f, 10f);
                    Font NormalFont = FontFactory.GetFont(pdfCellFontName, 12, Font.NORMAL, BaseColor.BLACK);

                    PdfWriter writer = PdfWriter.GetInstance(document, fs);
                    Phrase phrase = null;
                    PdfPCell cell = new PdfPCell();
                    PdfPTable table = null;
                    BaseColor color;

                    document.Open();
                    //getting Header logo path
                    if (string.IsNullOrEmpty(AzCopyConfig.ImagePath))
                    {
                        AzCopyConfig.LoadImageFromBlob();
                    }
                    string imageURL = AzCopyConfig.ImagePath + @"\" + ConfigurationManager.AppSettings["ImageBlobReference"];

                    //Header Table
                    table = new PdfPTable(2);
                    table.TotalWidth = 475f;
                    table.LockedWidth = true;
                    float[] headwidths = new float[] { 230f, 245f };
                    table.SetWidths(headwidths);
                    //adding header logo
                    cell = ImageCell(imageURL, 100f, PdfPCell.ALIGN_CENTER);
                    table.AddCell(cell);

                    //Heading Text
                    phrase = new Phrase();
                    phrase.Add(new Chunk(ConfigurationManager.AppSettings["BPMOnlineHeading1"] + "\n\n", FontFactory.GetFont(pdfCellFontName, 16, Font.BOLD, BaseColor.DARK_GRAY)));
                    phrase.Add(new Chunk("\n", FontFactory.GetFont(pdfCellFontName, 8, Font.NORMAL, BaseColor.BLACK)));

                    cell = PhraseCell(phrase, PdfPCell.ALIGN_CENTER);
                    cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    //Separater Line
                    color = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#A9A9A9"));
                    DrawLine(writer, 10f, document.Top - 50f, document.PageSize.Width - 25f, document.Top - 50f, color);
                    //adding header tabel to pdf document
                    document.Add(table);

                    //table for displaying store basic information
                    table = new PdfPTable(4);
                    table.HorizontalAlignment = 0;
                    table.SpacingBefore = 5f;
                    table.TotalWidth = 475f;
                    table.LockedWidth = true;
                    float[] basewidths = new float[] { 90f, 136f, 90f, 136f };
                    table.SetWidths(basewidths);


                    //HEading 2 Text
                    phrase = new Phrase();
                    phrase.Add(new Chunk(ConfigurationManager.AppSettings["BPMOnlineHeading2"], FontFactory.GetFont(pdfCellFontName, 10, Font.BOLD, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_CENTER);
                    cell.Colspan = 4;
                    cell.PaddingTop = 5f;
                    cell.PaddingBottom = 10f;
                    table.AddCell(cell);
                    //writing store basic information into table format
                    //DisplayBPMID Property Label
                    phrase = new Phrase();
                    phrase.Add(new Chunk("BPM ID", FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.BOLD, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.PaddingBottom = 5f;
                    cell.BorderWidth = 0.3f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //DisplayBPMID Property Value
                    phrase = new Phrase();
                    phrase.Add(new Chunk(objstorebasicInfo.DisplayBPMID, FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.NORMAL, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.PaddingBottom = 5f;
                    cell.BorderWidth = 0.3f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //CaseID Property Label
                    phrase = new Phrase();
                    phrase.Add(new Chunk(nameof(objstorebasicInfo.CaseID), FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.BOLD, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    cell.PaddingBottom = 5f;
                    cell.BorderWidth = 0.3f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //CaseID Property Value
                    phrase = new Phrase();
                    phrase.Add(new Chunk(Convert.ToString(objstorebasicInfo.CaseID), FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.NORMAL, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    cell.PaddingBottom = 5f;
                    cell.BorderWidth = 0.3f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //RequestName Property Label
                    phrase = new Phrase();
                    phrase.Add(new Chunk("Store", FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.BOLD, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    cell.PaddingBottom = 5f;
                    cell.BorderWidth = 0.3f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //RequestName Property Value
                    phrase = new Phrase();
                    phrase.Add(new Chunk(objstorebasicInfo.RequestName, FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.NORMAL, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    cell.PaddingBottom = 5f;
                    cell.BorderWidth = 0.3f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //MarketName Property Label
                    phrase = new Phrase();
                    phrase.Add(new Chunk("Market", FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.BOLD, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    cell.PaddingBottom = 5f;
                    cell.BorderWidth = 0.3f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //MarketName Property Value
                    phrase = new Phrase();
                    phrase.Add(new Chunk(objstorebasicInfo.MarketName, FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.NORMAL, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    cell.PaddingBottom = 5f;
                    cell.BorderWidth = 0.3f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //ProjectName Property Label
                    phrase = new Phrase();
                    phrase.Add(new Chunk("Type", FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.BOLD, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    cell.PaddingBottom = 5f;
                    cell.BorderWidth = 0.3f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //ProjectName Property Value
                    phrase = new Phrase();
                    phrase.Add(new Chunk(objstorebasicInfo.ProjectName, FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.NORMAL, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    cell.PaddingBottom = 5f;
                    cell.BorderWidth = 0.3f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //BrandName Property Label
                    phrase = new Phrase();
                    phrase.Add(new Chunk("Brand Name", FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.BOLD, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    cell.PaddingBottom = 5f;
                    cell.BorderWidth = 0.3f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //BrandName Property value
                    phrase = new Phrase();
                    phrase.Add(new Chunk(objstorebasicInfo.BrandName, FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.NORMAL, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    cell.PaddingBottom = 5f;
                    cell.BorderWidth = 0.3f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //SecurityDeposit Property Label
                    phrase = new Phrase();
                    phrase.Add(new Chunk("Security Deposit", FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.BOLD, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    cell.PaddingBottom = 5f;
                    cell.BorderWidth = 0.3f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //SecurityDeposit Property value
                    phrase = new Phrase();
                    phrase.Add(new Chunk(Convert.ToString(objstorebasicInfo.SecurityDeposit), FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.NORMAL, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    cell.PaddingBottom = 5f;
                    cell.BorderWidth = 0.3f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //TotalInvestment Property Label
                    phrase = new Phrase();
                    phrase.Add(new Chunk("Total Investment", FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.BOLD, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    cell.PaddingBottom = 5f;
                    cell.BorderWidth = 0.3f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //TotalInvestment Property Value
                    phrase = new Phrase();
                    phrase.Add(new Chunk(Convert.ToString(objstorebasicInfo.TotalInvestment), FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.NORMAL, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    cell.PaddingBottom = 5f;
                    cell.BorderWidth = 0.3f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //KeyMoney Property label
                    phrase = new Phrase();
                    phrase.Add(new Chunk("Key Money", FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.BOLD, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    cell.PaddingBottom = 5f;
                    cell.BorderWidth = 0.3f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //KeyMoney Property Value
                    phrase = new Phrase();
                    phrase.Add(new Chunk(Convert.ToString(objstorebasicInfo.KeyMoney), FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.NORMAL, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    cell.PaddingBottom = 5f;
                    cell.BorderWidth = 0.3f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //CAPEX Property Label
                    phrase = new Phrase();
                    phrase.Add(new Chunk("CAPEX", FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.BOLD, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    cell.PaddingBottom = 5f;
                    cell.BorderWidth = 0.3f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //CAPEX Property Value
                    phrase = new Phrase();
                    phrase.Add(new Chunk(Convert.ToString(objstorebasicInfo.Brand), FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.NORMAL, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    cell.PaddingBottom = 5f;
                    cell.BorderWidth = 0.3f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //StoreTypeName Property Label
                    phrase = new Phrase();
                    phrase.Add(new Chunk("Store Type Name", FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.BOLD, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    cell.PaddingBottom = 5f;
                    cell.BorderWidth = 0.3f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //StoreTypeName Property Value
                    phrase = new Phrase();
                    phrase.Add(new Chunk(objstorebasicInfo.StoreTypeName, FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.NORMAL, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    cell.PaddingBottom = 5f;
                    cell.BorderWidth = 0.3f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //NetSellingSpace Property Label
                    phrase = new Phrase();
                    phrase.Add(new Chunk("Net Selling Space", FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.BOLD, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    cell.PaddingBottom = 5f;
                    cell.BorderWidth = 0.3f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //NetSellingSpace Property value
                    phrase = new Phrase();
                    phrase.Add(new Chunk(Convert.ToString(objstorebasicInfo.NetSellingSpace), FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.NORMAL, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    cell.PaddingBottom = 5f;
                    cell.BorderWidth = 0.3f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //OpeningDate Property Label
                    phrase = new Phrase();
                    phrase.Add(new Chunk("Opening Date", FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.BOLD, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    cell.PaddingBottom = 5f;
                    cell.BorderWidth = 0.3f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //OpeningDate Property Value
                    string openingDate = string.Empty;
                    if (objstorebasicInfo.OpeningDate != null)
                    {
                        DateTime odate = (DateTime)(objstorebasicInfo.OpeningDate);
                        openingDate = odate.ToString("dd/MM/yyyy");
                    }
                    phrase = new Phrase();
                    phrase.Add(new Chunk(openingDate, FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.NORMAL, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    cell.PaddingBottom = 5f;
                    cell.BorderWidth = 0.3f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //LeaseEndDate Property Label
                    phrase = new Phrase();
                    phrase.Add(new Chunk("Lease End Date", FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.BOLD, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    cell.PaddingBottom = 5f;
                    cell.BorderWidth = 0.3f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //LeaseEndDate Property Value
                    string leaseEndDate = string.Empty;
                    if (objstorebasicInfo.LeaseEndDate != null)
                    {
                        //Converting datetime to dd/MM/yyyy format
                        DateTime ldate = (DateTime)(objstorebasicInfo.LeaseEndDate);
                        leaseEndDate = ldate.ToString("dd/MM/yyyy");
                    }
                    phrase = new Phrase();
                    phrase.Add(new Chunk(leaseEndDate, FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.NORMAL, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    cell.PaddingBottom = 5f;
                    cell.BorderWidth = 0.3f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //LeasingPeriodDec Property Label
                    phrase = new Phrase();
                    phrase.Add(new Chunk("Leasing Period Dec", FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.BOLD, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    cell.PaddingBottom = 5f;
                    cell.BorderWidth = 0.3f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //LeasingPeriodDec Property Value
                    phrase = new Phrase();
                    phrase.Add(new Chunk(Convert.ToString(objstorebasicInfo.LeasingPeriodDec), FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.NORMAL, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    cell.PaddingBottom = 5f;
                    cell.BorderWidth = 0.3f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //CancelPeriod Property Label
                    phrase = new Phrase();
                    phrase.Add(new Chunk("Cancel Period", FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.BOLD, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    cell.PaddingBottom = 5f;
                    cell.BorderWidth = 0.3f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //CancelPeriod Property Value
                    phrase = new Phrase();
                    phrase.Add(new Chunk(Convert.ToString(objstorebasicInfo.CancelPeriod), FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.NORMAL, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    cell.PaddingBottom = 5f;
                    cell.BorderWidth = 0.3f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //LeaseBreakOption Property Label
                    phrase = new Phrase();
                    phrase.Add(new Chunk("Lease Break Option", FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.BOLD, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    cell.PaddingBottom = 5f;
                    cell.BorderWidth = 0.3f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //LeaseBreakOption Property Value
                    phrase = new Phrase();
                    phrase.Add(new Chunk(Convert.ToString(objstorebasicInfo.LeaseBreakOption), FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.NORMAL, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    cell.PaddingBottom = 5f;
                    cell.BorderWidth = 0.3f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //CapexSpendYear Property Label
                    phrase = new Phrase();
                    phrase.Add(new Chunk("Capex Spend Year", FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.BOLD, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    cell.PaddingBottom = 5f;
                    cell.BorderWidth = 0.3f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //CapexSpendYear Property Value
                    phrase = new Phrase();
                    phrase.Add(new Chunk(Convert.ToString(objstorebasicInfo.CapexSpendYear), FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.NORMAL, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    cell.PaddingBottom = 5f;
                    cell.BorderWidth = 0.3f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //GrossLeasedArea Property Label
                    phrase = new Phrase();
                    phrase.Add(new Chunk("Gross Leased Area", FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.BOLD, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    cell.PaddingBottom = 5f;
                    cell.BorderWidth = 0.3f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //GrossLeasedArea Property Value
                    phrase = new Phrase();
                    phrase.Add(new Chunk(Convert.ToString(objstorebasicInfo.GrossLeasedArea), FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.NORMAL, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.VerticalAlignment = PdfPCell.ALIGN_LEFT;
                    cell.PaddingBottom = 5f;
                    cell.BorderWidth = 0.3f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //Empty Cell
                    phrase = new Phrase();
                    phrase.Add(new Chunk(" ", FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.BOLD, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    cell.PaddingBottom = 5f;
                    cell.BorderWidth = 0.3f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //Empty Cell
                    phrase = new Phrase();
                    phrase.Add(new Chunk(" ", FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.NORMAL, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.VerticalAlignment = PdfPCell.ALIGN_LEFT;
                    cell.PaddingBottom = 5f;
                    cell.BorderWidth = 0.3f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);

                    //add table to document
                    document.Add(table);
                    //Adding Store Executive financial Summary
                    //Adding new table to document
                    table = new PdfPTable(8);
                    table.HorizontalAlignment = 0;
                    table.SpacingBefore = 5f;
                    table.TotalWidth = 475f;
                    table.LockedWidth = true;
                    float[] widths = new float[] { 110f, 46f, 46f, 46f, 46f, 46f, 46f, 70f };
                    table.SetWidths(widths);
                    //Heading
                    phrase = new Phrase();
                    phrase.Add(new Chunk(ConfigurationManager.AppSettings["BPMOnlineHeading3"], FontFactory.GetFont(pdfCellFontName, 10, Font.BOLD, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_CENTER);
                    cell.Colspan = 8;
                    cell.PaddingTop = 10f;
                    cell.PaddingBottom = 10f;
                    table.AddCell(cell);
                    //writing executive summary values to table
                    string y0 = string.Empty;
                    string y1 = string.Empty;
                    string y2 = string.Empty;
                    string y3 = string.Empty;
                    string y4 = string.Empty;
                    string y5 = string.Empty;
                    if (lststoresummary!=null && lststoresummary.Count > 0)
                    {
                        //from list getting years values
                        y0 = lststoresummary[0].Y0;
                        y1 = lststoresummary[0].Y1;
                        y2 = lststoresummary[0].Y2;
                        y3 = lststoresummary[0].Y3;
                        y4 = lststoresummary[0].Y4;
                        y5 = lststoresummary[0].Y5;

                    }
                    //writing StoreExecutiveSummary columns header labels into table header row
                    //StoreExecutiveSummary Description Label
                    phrase = new Phrase();
                    phrase.Add(new Chunk("Description", FontFactory.GetFont(pdfCellFontName, pdfCellSummaryFontSize, Font.BOLD, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_CENTER);
                    cell.PaddingBottom = 5f;
                    cell.BorderWidth = 0.3f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //StoreExecutiveSummary Y0Val Label
                    phrase = new Phrase();
                    phrase.Add(new Chunk("Y0" + "\n" + y0, FontFactory.GetFont(pdfCellFontName, pdfCellSummaryFontSize, Font.BOLD, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_CENTER);
                    cell.PaddingBottom = 5f;
                    cell.BorderWidth = 0.3f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //StoreExecutiveSummary Y1Val Label
                    phrase = new Phrase();
                    phrase.Add(new Chunk("Y1" + "\n" + y1, FontFactory.GetFont(pdfCellFontName, pdfCellSummaryFontSize, Font.BOLD, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_CENTER);
                    cell.PaddingBottom = 5f;
                    cell.BorderWidth = 0.3f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //StoreExecutiveSummary Y2Val  Label
                    phrase = new Phrase();
                    phrase.Add(new Chunk("Y2" + "\n" + y2, FontFactory.GetFont(pdfCellFontName, pdfCellSummaryFontSize, Font.BOLD, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_CENTER);
                    cell.PaddingBottom = 5f;
                    cell.BorderWidth = 0.3f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //StoreExecutiveSummary Y3Val Label
                    phrase = new Phrase();
                    phrase.Add(new Chunk("Y3" + "\n" + y3, FontFactory.GetFont(pdfCellFontName, pdfCellSummaryFontSize, Font.BOLD, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_CENTER);
                    cell.PaddingBottom = 5f;
                    cell.BorderWidth = 0.3f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //StoreExecutiveSummary Y4Val value Label
                    phrase = new Phrase();
                    phrase.Add(new Chunk("Y4" + "\n" + y4, FontFactory.GetFont(pdfCellFontName, pdfCellSummaryFontSize, Font.BOLD, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_CENTER);
                    cell.PaddingBottom = 5f;
                    cell.BorderWidth = 0.3f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //StoreExecutiveSummary Y5Val Label
                    phrase = new Phrase();
                    phrase.Add(new Chunk("Y5" + "\n" + y5, FontFactory.GetFont(pdfCellFontName, pdfCellSummaryFontSize, Font.BOLD, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_CENTER);
                    cell.PaddingBottom = 5f;
                    cell.BorderWidth = 0.3f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //StoreExecutiveSummary GRSVal Label
                    phrase = new Phrase();
                    phrase.Add(new Chunk("GRAPE" + "\n" + "Ave values in EUR", FontFactory.GetFont(pdfCellFontName, pdfCellSummaryFontSize, Font.BOLD, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_CENTER);
                    cell.PaddingBottom = 5f;
                    cell.BorderWidth = 0.3f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //if StoreExecutiveSummary is not null
                    if (lststoresummary != null)
                    {
                        foreach (StoreExecutiveSummary saExecutiveSummary in lststoresummary)
                        {
                            //writing all the StoreExecutiveSummary rows into table from lststoresummary
                            //Description value
                            phrase = new Phrase();
                            phrase.Add(new Chunk(saExecutiveSummary.Description, FontFactory.GetFont(pdfCellFontName, pdfCellSummaryFontSize, Font.BOLD, BaseColor.BLACK)));
                            cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                            cell.BorderWidth = 0.3f;
                            cell.BorderColor = BaseColor.BLACK;
                            cell.PaddingBottom = 5f;
                            table.AddCell(cell);
                            //Y0Val value
                            phrase = new Phrase();
                            phrase.Add(new Chunk(saExecutiveSummary.Y0Val, FontFactory.GetFont(pdfCellFontName, pdfCellSummaryFontSize, Font.NORMAL, BaseColor.BLACK)));
                            cell = PhraseCell(phrase, PdfPCell.ALIGN_CENTER);
                            cell.BorderWidth = 0.3f;
                            cell.BorderColor = BaseColor.BLACK;
                            cell.PaddingBottom = 5f;
                            table.AddCell(cell);
                            //Y1Val value
                            phrase = new Phrase();
                            phrase.Add(new Chunk(saExecutiveSummary.Y1Val, FontFactory.GetFont(pdfCellFontName, pdfCellSummaryFontSize, Font.NORMAL, BaseColor.BLACK)));
                            cell = PhraseCell(phrase, PdfPCell.ALIGN_CENTER);
                            cell.BorderWidth = 0.3f;
                            cell.BorderColor = BaseColor.BLACK;
                            cell.PaddingBottom = 5f;
                            table.AddCell(cell);
                            //Y2Val value
                            phrase = new Phrase();
                            phrase.Add(new Chunk(saExecutiveSummary.Y2Val, FontFactory.GetFont(pdfCellFontName, pdfCellSummaryFontSize, Font.NORMAL, BaseColor.BLACK)));
                            cell = PhraseCell(phrase, PdfPCell.ALIGN_CENTER);
                            cell.BorderWidth = 0.3f;
                            cell.BorderColor = BaseColor.BLACK;
                            cell.PaddingBottom = 5f;
                            table.AddCell(cell);
                            //Y3Val value
                            phrase = new Phrase();
                            phrase.Add(new Chunk(saExecutiveSummary.Y3Val, FontFactory.GetFont(pdfCellFontName, pdfCellSummaryFontSize, Font.NORMAL, BaseColor.BLACK)));
                            cell = PhraseCell(phrase, PdfPCell.ALIGN_CENTER);
                            cell.BorderWidth = 0.3f;
                            cell.BorderColor = BaseColor.BLACK;
                            cell.PaddingBottom = 5f;
                            table.AddCell(cell);
                            //Y4Val value
                            phrase = new Phrase();
                            phrase.Add(new Chunk(saExecutiveSummary.Y4Val, FontFactory.GetFont(pdfCellFontName, pdfCellSummaryFontSize, Font.NORMAL, BaseColor.BLACK)));
                            cell = PhraseCell(phrase, PdfPCell.ALIGN_CENTER);
                            cell.BorderWidth = 0.3f;
                            cell.BorderColor = BaseColor.BLACK;
                            cell.PaddingBottom = 5f;
                            table.AddCell(cell);
                            //Y5Val value
                            phrase = new Phrase();
                            phrase.Add(new Chunk(saExecutiveSummary.Y5Val, FontFactory.GetFont(pdfCellFontName, pdfCellSummaryFontSize, Font.NORMAL, BaseColor.BLACK)));
                            cell = PhraseCell(phrase, PdfPCell.ALIGN_CENTER);
                            cell.BorderWidth = 0.3f;
                            cell.BorderColor = BaseColor.BLACK;
                            cell.PaddingBottom = 5f;
                            table.AddCell(cell);
                            //GRSVal value
                            phrase = new Phrase();
                            phrase.Add(new Chunk(saExecutiveSummary.GRSVal, FontFactory.GetFont(pdfCellFontName, pdfCellSummaryFontSize, Font.NORMAL, BaseColor.BLACK)));
                            cell = PhraseCell(phrase, PdfPCell.ALIGN_CENTER);
                            cell.BorderWidth = 0.3f;
                            cell.BorderColor = BaseColor.BLACK;
                            cell.PaddingBottom = 5f;
                            table.AddCell(cell);
                        }
                    }
                    

                    //add Store Executive financial Summary tabel to pdf document
                    document.Add(table);
                    document.Close();
                    // Close the writer instance
                    writer.Close();
                    // Always close open filehandles explicity
                    fs.Close();
                    //upload file into azure blob container
                    if (this.executeazCopyExe(PdfBucketPath, requestID, pdfFileName))
                    {
                        //if file upload successsfully into blob container then delete the directory and files 
                        this.clearfiles(PdfBucketPath);
                    }
                }
                InsightLogger.TrackEndEvent(callerMethodName);

            }
            catch (DataAccessException dalexception)
            {
                throw dalexception;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);               
                throw new BusinessLogicException(exception.Message, exception.InnerException);
            }
        }
        /// <summary>
        /// This method will create a folder with requestid and provide the path for saving the pdf file into it.
        /// </summary>
        /// <param name="RequestID"></param>
        /// <returns></returns>
        public string GetPDFPath(string RequestID)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                InsightLogger.TrackStartEvent(callerMethodName);
                string Pdfpath = string.Empty;
                string functionsPath = Path.Combine(Environment.CurrentDirectory, "PDFFiles");
                if (!Directory.Exists(functionsPath))
                {
                    Directory.CreateDirectory(functionsPath);
                }
                //if RequestID folder already exists then will delete the directory 
                Pdfpath = Path.Combine(functionsPath, RequestID);
                
                System.IO.DirectoryInfo requestIDdir = new System.IO.DirectoryInfo(Pdfpath);
                if (requestIDdir.Exists)
                {
                    foreach (FileInfo file in requestIDdir.GetFiles())
                    {
                        file.Delete();
                    }
                    System.IO.Directory.Delete(Pdfpath, true);
                }
                //create directory with requestID name
                Directory.CreateDirectory(Pdfpath);              
                InsightLogger.TrackEndEvent(callerMethodName);
                return Pdfpath;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                throw new BusinessLogicException(exception.Message, exception.InnerException);
            }
        }
        /// <summary>
        /// This method creates imagecell in pdf table in pdf document
        /// </summary>
        /// <param name="path"></param>
        /// <param name="scale"></param>
        /// <param name="align"></param>
        /// <returns></returns>
        private static PdfPCell ImageCell(string path, float scale, int align)
        {
            try
            {
                //define style for image cell
                iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(path);
                image.ScalePercent(scale);
                PdfPCell cell = new PdfPCell(image);
                cell.BorderColor = BaseColor.WHITE;
                cell.BackgroundColor = BaseColor.GRAY;
                cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                cell.HorizontalAlignment = align;
                cell.PaddingBottom = 0f;
                cell.PaddingTop = 0f;
                return cell;
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error in GeneratePDF:: ImageCell() method : "
               + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new BusinessLogicException(exception.Message, exception.InnerException);
            }
            
        }
        /// <summary>
        /// This method draws separate line in  pdf document
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="color"></param>
        private static void DrawLine(PdfWriter writer, float x1, float y1, float x2, float y2, BaseColor color)
        {
            try
            {
                //define stlye for separate line
                PdfContentByte contentByte = writer.DirectContent;
                contentByte.SetColorStroke(color);
                contentByte.MoveTo(x1, y1);
                contentByte.LineTo(x2, y2);
                contentByte.Stroke();
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error in GeneratePDF:: DrawLine() method : "
               + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new BusinessLogicException(exception.Message, exception.InnerException);
            }
            
        }
        /// <summary>
        /// This method create and align the pdf table cell
        /// </summary>
        /// <param name="phrase"></param>
        /// <param name="align"></param>
        /// <returns></returns>
        private static PdfPCell PhraseCell(Phrase phrase, int align)
        {
            try
            {
                //foramte tabel cell with style
                PdfPCell cell = new PdfPCell(phrase);
                cell.BorderColor = BaseColor.WHITE;
                cell.VerticalAlignment = PdfPCell.ALIGN_TOP;
                cell.HorizontalAlignment = align;
                cell.PaddingBottom = 2f;
                cell.PaddingTop = 0f;
                return cell;
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error in GeneratePDF:: PhraseCell() method : "
               + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new BusinessLogicException(exception.Message, exception.InnerException);
            }
            
        }
        /// <summary>
        /// This methodupload the pdf file into azure blob container with the help of aZcopy.exe
        /// </summary>
        /// <param name="pdfFilePath"></param>
        /// <param name="requestID"></param>
        /// <param name="reqPdfFileName"></param>
        /// <returns></returns>
        public bool executeazCopyExe(string pdfFilePath, string requestID,string reqPdfFileName)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                InsightLogger.TrackStartEvent(callerMethodName);
                //get azure blob container url from app.config file
                string blobUrl = ConfigurationManager.AppSettings["BlobUrl"];
                //get azure storage account key from app.config file
                string blobkey = ConfigurationManager.AppSettings["BlobAccountKey"];
                //get azcopy.exe file path 
                if (string.IsNullOrEmpty(AzCopyConfig.AzCopyPath))
                {
                    AzCopyConfig.LoadAzCopyConfigFromBlob();
                }
                string azCopyPath = AzCopyConfig.AzCopyPath;
                string EXEPath = Path.Combine(azCopyPath, @"aZCopy\AzCopy.exe");
                Process proc;
                proc = new Process();
                proc.StartInfo.FileName = EXEPath;
                bool isFileUpload = false;
                try
                {
                    //process to trigger azcopy.exe from cmd line and push the files from specified path to azure storage blob container
                    proc.StartInfo.CreateNoWindow = true;
                    proc.StartInfo.UseShellExecute = false;
                    proc.Start();
                    proc.WaitForExit();

                    System.Diagnostics.Process process = new System.Diagnostics.Process();
                    System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                    startInfo.FileName = "cmd.exe";
                    startInfo.WorkingDirectory = Path.Combine(azCopyPath, @"aZCopy");
                    startInfo.UseShellExecute = false;
                    startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                    startInfo.CreateNoWindow = true;
                    startInfo.RedirectStandardOutput = true;
                    startInfo.RedirectStandardInput = true;
                    process.StartInfo = startInfo;
                    process.Start();
                    System.IO.StreamReader SR = process.StandardOutput;
                    System.IO.StreamWriter SW = process.StandardInput;
                    string tempGETCMD = null;
                    SW.WriteLine(@"AzCopy " + pdfFilePath + " " + blobUrl + " /destkey:" + blobkey + @" /S /V");
                    //exits command prompt window
                    SW.WriteLine("exit");
                    //returns results of the command window
                    tempGETCMD = SR.ReadToEnd();

                    SW.Close();
                    SR.Close();
                    isFileUpload = true;
                    string pdfblobUrl = blobUrl + @"/" + reqPdfFileName + ".pdf";
                    //adding pdfurl details to RequestPDF object
                    RequestPDFAddress objRequestPDFAddress = new RequestPDFAddress()
                    {
                        RequestID = requestID,
                        PDF_URL = pdfblobUrl
                    };
                    //convert RequestPDF object into json string
                    string requestPdfUrlDetails = JsonConvert.SerializeObject(objRequestPDFAddress);
                    //insert pdf blob url details into queue
                    this.AddPdfUrlDetailsToQueue(requestPdfUrlDetails);
                    InsightLogger.TrackEndEvent(callerMethodName);
                    return isFileUpload;
                }

                catch (Exception exception)
                {
                    InsightLogger.Exception(exception.Message, exception, callerMethodName);
                    throw new BusinessLogicException(exception.Message, exception.InnerException);
                }
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                throw new BusinessLogicException(exception.Message, exception.InnerException);
            }

        }
        /// <summary>
        /// This method deletes the files and directory from specified path
        /// </summary>
        /// <param name="pdfPath"></param>
        private void clearfiles(string pdfPath)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                InsightLogger.TrackStartEvent(callerMethodName);
                //delete directory and it's files from given path
                System.IO.DirectoryInfo Pdfdir = new System.IO.DirectoryInfo(pdfPath);
                if (Pdfdir.Exists)
                {
                    foreach (FileInfo file in Pdfdir.GetFiles())
                    {
                        file.Delete();
                    }
                    System.IO.Directory.Delete(pdfPath, true);
                }
                InsightLogger.TrackEndEvent(callerMethodName);

            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                throw new BusinessLogicException(exception.Message, exception.InnerException);

            }
        }
        /// <summary>
        /// This method inserts the pdfUrl messages into queue
        /// </summary>
        /// <param name="pdfDetails"></param>
        private void AddPdfUrlDetailsToQueue(string pdfDetails)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                InsightLogger.TrackStartEvent(callerMethodName);
                // Create the queue client.
                CloudQueueClient cqdocClient = AzureQueues.GetQueueClient();
                // Retrieve a reference to a queue.
                CloudQueue queuedoc = AzureQueues.GetRequestPDFQueue(cqdocClient);
                // Async enqueue the message                           
                CloudQueueMessage message = new CloudQueueMessage(pdfDetails);
                queuedoc.AddMessage(message);
                InsightLogger.TrackEndEvent(callerMethodName);
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                throw new BusinessLogicException(exception.Message, exception.InnerException);

            }
        }
        /// <summary>
        /// This method Generates PDf file for CAR based on requestID
        /// </summary>
        /// <param name="requestID"></param>
        public void GeneratePDFForCARApproval(string requestID)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                InsightLogger.TrackStartEvent(callerMethodName);
                //read Font name,size,heading summary font dvalues from app.config
                string pdfCellFontName = ConfigurationManager.AppSettings["PDFCellFontName"];
                int pdfCellFontSize = Convert.ToInt32(ConfigurationManager.AppSettings["PDFCellFontSize"]);
                int pdfCellSummaryFontSize = Convert.ToInt32(ConfigurationManager.AppSettings["PDFSummarryCellFontSize"]);
                //Call the stored procedure for gettting the  pdf details from Store backend
                //clone the result set into CarApprovalModel class
                CarApprovalModel objCarDetails = objCARPDF.GetPDFDetailsFromCAR(requestID);
                CarSummary objCarSummary;
                List<CarCapexMatrix> lstCarCapex = null;
                //checking CarApprovalModel result has null or not
                if (objCarDetails != null)
                {
                    //stored procedure returns two result sets
                    //1.Car basic information
                    objCarSummary = objCarDetails.CarBasicInformation;
                    //2.Car Capex Matrix details
                    lstCarCapex = objCarDetails.CarCapexMatrixDetails.ToList();
                    //Create folder with requestid name in application Environment.CurrentDirectory and read the folder path
                    string PdfBucketPath = GetPDFPath(requestID);
                    //Pdf file name requestid + yyyyMMddHHmmss
                    string pdfFileName = requestID + DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                    //create pdf filewith requestid namme and writes the details
                    System.IO.FileStream fs = new FileStream(PdfBucketPath + "\\" + pdfFileName + ".pdf", FileMode.Create);
                    //define pdf design style
                    Document document = new Document(PageSize.A4, 88f, 88f, 10f, 10f);
                    Font NormalFont = FontFactory.GetFont(pdfCellFontName, 12, Font.NORMAL, BaseColor.BLACK);

                    PdfWriter writer = PdfWriter.GetInstance(document, fs);
                    Phrase phrase = null;
                    PdfPCell cell = new PdfPCell();
                    PdfPTable table = null;
                    BaseColor color;

                    document.Open();
                    //getting Header logo path
                    if (string.IsNullOrEmpty(AzCopyConfig.ImagePath))
                    {
                        AzCopyConfig.LoadImageFromBlob();
                    }
                    string imageURL = AzCopyConfig.ImagePath + @"\" + ConfigurationManager.AppSettings["ImageBlobReference"];

                    //Header Table
                    table = new PdfPTable(2);
                    table.TotalWidth = 475f;
                    table.LockedWidth = true;
                    float[] headwidths = new float[] { 230f, 245f };
                    table.SetWidths(headwidths);


                    cell = ImageCell(imageURL, 100f, PdfPCell.ALIGN_CENTER);
                    table.AddCell(cell);

                    //Heading Text
                    phrase = new Phrase();
                    phrase.Add(new Chunk(ConfigurationManager.AppSettings["CARBackendPDFHeading1"] + "\n\n", FontFactory.GetFont(pdfCellFontName, 16, Font.BOLD, BaseColor.DARK_GRAY)));
                    phrase.Add(new Chunk("\n", FontFactory.GetFont(pdfCellFontName, 8, Font.NORMAL, BaseColor.BLACK)));

                    cell = PhraseCell(phrase, PdfPCell.ALIGN_CENTER);
                    cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    //Separater Line
                    color = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#A9A9A9"));
                    DrawLine(writer, 10f, document.Top - 50f, document.PageSize.Width - 25f, document.Top - 50f, color);
                    //DrawLine(writer, 10f, document.Top - 80f, document.PageSize.Width - 25f, document.Top - 80f, color);
                    document.Add(table);


                    table = new PdfPTable(2);
                    table.HorizontalAlignment = 0;
                    table.SpacingBefore = 5f;
                    table.TotalWidth = 475f;
                    table.LockedWidth = true;
                    float[] basewidths = new float[] { 180f,275f };
                    table.SetWidths(basewidths);


                    //Heading 2 Text
                    phrase = new Phrase();
                    phrase.Add(new Chunk(ConfigurationManager.AppSettings["CARBackendPDFHeading2"], FontFactory.GetFont(pdfCellFontName, 10, Font.BOLD, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_CENTER);
                    cell.Colspan = 2;
                    cell.PaddingTop = 5f;
                    cell.PaddingBottom = 10f;
                    table.AddCell(cell);
                    //writing store basic information into table format
                    //DateOfRequest Label
                    phrase = new Phrase();
                    phrase.Add(new Chunk("Date of Request", FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.BOLD, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.PaddingBottom = 8f;
                    cell.PaddingTop = 8f; 
                    cell.BorderWidthTop = 0.3f;
                    cell.BorderWidthLeft = 0.3f;                   
                    cell.BorderWidthRight = 0f;
                    cell.BorderWidthBottom = 0f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //DateOfRequest Label value
                    string requestDate = string.Empty;
                    if (objCarSummary.DateofRequest != null)
                    {
                        DateTime dateofRequest =(DateTime)(objCarSummary.DateofRequest);
                        requestDate = dateofRequest.ToString("dd/MM/yyyy");
                    }                   
                    phrase = new Phrase();
                    phrase.Add(new Chunk(":   ", FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.BOLD, BaseColor.BLACK)));
                    phrase.Add(new Chunk(requestDate, FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.NORMAL, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.PaddingTop = 8f;
                    cell.PaddingBottom = 8f;  
                    cell.BorderWidthTop = 0.3f;
                    cell.BorderWidthRight = 0.3f;
                    cell.BorderWidthLeft = 0f;                
                    cell.BorderWidthBottom = 0f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);

                    //Brand Description
                    phrase = new Phrase();
                    phrase.Add(new Chunk("Brand", FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.BOLD, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    cell.PaddingBottom = 8f;  
                    cell.BorderWidthLeft = 0.3f;
                    cell.BorderWidthTop = 0f;
                    cell.BorderWidthRight = 0f;
                    cell.BorderWidthBottom = 0f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);

                    //Brand Value
                    phrase = new Phrase();
                    phrase.Add(new Chunk(":   ", FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.BOLD, BaseColor.BLACK)));
                    phrase.Add(new Chunk(objCarSummary.BrandDescription, FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.NORMAL, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    cell.PaddingBottom = 8f;  
                    cell.BorderWidthLeft = 0f;
                    cell.BorderWidthTop = 0f;
                    cell.BorderWidthRight = 0.3f;
                    cell.BorderWidthBottom = 0f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //MarketDescription label
                    phrase = new Phrase();
                    phrase.Add(new Chunk("Market / Function", FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.BOLD, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    cell.PaddingBottom = 8f;  
                    cell.BorderWidthLeft = 0.3f;
                    cell.BorderWidthTop = 0f;
                    cell.BorderWidthRight = 0f;
                    cell.BorderWidthBottom = 0f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //MarketDescription value
                    phrase = new Phrase();
                    phrase.Add(new Chunk(":   ", FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.BOLD, BaseColor.BLACK)));
                    phrase.Add(new Chunk(objCarSummary.MarketDescription, FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.NORMAL, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    cell.PaddingBottom = 8f;  
                    cell.BorderWidthLeft = 0f;
                    cell.BorderWidthTop = 0f;
                    cell.BorderWidthRight = 0.3f;
                    cell.BorderWidthBottom = 0f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);

                    //Country Label
                    phrase = new Phrase();
                    phrase.Add(new Chunk("Country", FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.BOLD, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    cell.PaddingBottom = 8f;  
                    cell.BorderWidthLeft = 0.3f;
                    cell.BorderWidthTop = 0f;
                    cell.BorderWidthRight = 0f;
                    cell.BorderWidthBottom = 0f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //Country Description value
                    phrase = new Phrase();
                    phrase.Add(new Chunk(":   ", FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.BOLD, BaseColor.BLACK)));
                    phrase.Add(new Chunk(objCarSummary.CountryDescription, FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.NORMAL, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    cell.PaddingBottom = 8f;  
                    cell.BorderWidthLeft = 0f;
                    cell.BorderWidthTop = 0f;
                    cell.BorderWidthRight = 0.3f;
                    cell.BorderWidthBottom = 0f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //InvestmentType Label
                    phrase = new Phrase();
                    phrase.Add(new Chunk("Investment Type", FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.BOLD, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    cell.PaddingBottom = 8f;  
                    cell.BorderWidthLeft = 0.3f;
                    cell.BorderWidthTop = 0f;
                    cell.BorderWidthRight = 0f;
                    cell.BorderWidthBottom = 0f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //InvestmentType Value
                    phrase = new Phrase();
                    phrase.Add(new Chunk(":   ", FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.BOLD, BaseColor.BLACK)));
                    phrase.Add(new Chunk(objCarSummary.InvestmentTypeDescription, FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.NORMAL, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    cell.PaddingBottom = 8f;  
                    cell.BorderWidthLeft = 0f;
                    cell.BorderWidthTop = 0f;
                    cell.BorderWidthRight = 0.3f;
                    cell.BorderWidthBottom = 0f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //EstimatedStartDate Label
                    phrase = new Phrase();
                    phrase.Add(new Chunk("Estimated Start Date", FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.BOLD, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    cell.PaddingBottom = 8f;  
                    cell.BorderWidthLeft = 0.3f;
                    cell.BorderWidthTop = 0f;
                    cell.BorderWidthRight = 0f;
                    cell.BorderWidthBottom = 0f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //EstimatedStartDate value
                    string estimatedStartDate = string.Empty;
                    if (objCarSummary.EstimatedStartDate != null)
                    {
                        DateTime startdate = (DateTime)(objCarSummary.EstimatedStartDate);
                        estimatedStartDate = startdate.ToString("dd/MM/yyyy");
                    }
                    phrase = new Phrase();
                    phrase.Add(new Chunk(":   ", FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.BOLD, BaseColor.BLACK)));
                    phrase.Add(new Chunk(estimatedStartDate, FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.NORMAL, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    cell.PaddingBottom = 8f;  
                    cell.BorderWidthLeft = 0f;
                    cell.BorderWidthTop = 0f;
                    cell.BorderWidthRight = 0.3f;
                    cell.BorderWidthBottom = 0f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //EstimatedCompletionDate Lable
                    phrase = new Phrase();
                    phrase.Add(new Chunk("Estimated Completion Date", FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.BOLD, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    cell.PaddingBottom = 8f;  
                    cell.BorderWidthLeft = 0.3f;
                    cell.BorderWidthTop = 0f;
                    cell.BorderWidthRight = 0f;
                    cell.BorderWidthBottom = 0f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //EstimatedCompletionDate Value
                    string estimatedCompletionDate = string.Empty;
                    if (objCarSummary.EstimatedCompletionDate != null)
                    {
                        DateTime completionDate = (DateTime)(objCarSummary.EstimatedCompletionDate);
                        estimatedCompletionDate = completionDate.ToString("dd/MM/yyyy");
                    }
                    phrase = new Phrase();
                    phrase.Add(new Chunk(":   ", FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.BOLD, BaseColor.BLACK)));
                    phrase.Add(new Chunk(estimatedCompletionDate, FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.NORMAL, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    cell.PaddingBottom = 8f;  
                    cell.BorderWidthLeft = 0f;
                    cell.BorderWidthTop = 0f;
                    cell.BorderWidthRight = 0.3f;
                    cell.BorderWidthBottom = 0f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //Budgeted Label
                    phrase = new Phrase();
                    phrase.Add(new Chunk("Budgeted", FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.BOLD, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    cell.PaddingBottom = 8f;  
                    cell.BorderWidthLeft = 0.3f;
                    cell.BorderWidthTop = 0f;
                    cell.BorderWidthRight = 0f;
                    cell.BorderWidthBottom = 0f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //Budgeted Label Value
                    phrase = new Phrase();
                    phrase.Add(new Chunk(":   ", FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.BOLD, BaseColor.BLACK)));
                    phrase.Add(new Chunk(Convert.ToString(objCarSummary.Budgeted), FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.NORMAL, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    cell.PaddingBottom = 8f;  
                    cell.BorderWidthLeft = 0f;
                    cell.BorderWidthTop = 0f;
                    cell.BorderWidthRight = 0.3f;
                    cell.BorderWidthBottom = 0f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //Capex Label
                    phrase = new Phrase();
                    phrase.Add(new Chunk("Project Total Capex", FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.BOLD, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    cell.PaddingBottom = 8f;  
                    cell.BorderWidthLeft = 0.3f;
                    cell.BorderWidthTop = 0f;
                    cell.BorderWidthRight = 0f;
                    cell.BorderWidthBottom = 0f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //Capex Label value
                    phrase = new Phrase();
                    phrase.Add(new Chunk(":   ", FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.BOLD, BaseColor.BLACK)));
                    phrase.Add(new Chunk(Convert.ToString(objCarSummary.Capex), FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.NORMAL, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    cell.PaddingBottom = 8f;  
                    cell.BorderWidthLeft = 0f;
                    cell.BorderWidthTop = 0f;
                    cell.BorderWidthRight = 0.3f;
                    cell.BorderWidthBottom = 0f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //CapexLocal Label
                    phrase = new Phrase();
                    phrase.Add(new Chunk("Project Total Capex(Local Currency)", FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.BOLD, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    cell.PaddingBottom = 8f;  
                    cell.BorderWidthLeft = 0.3f;
                    cell.BorderWidthTop = 0f;
                    cell.BorderWidthRight = 0f;
                    cell.BorderWidthBottom = 0f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //CapexLocal Label Value
                    phrase = new Phrase();
                    phrase.Add(new Chunk(":   ", FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.BOLD, BaseColor.BLACK)));
                    phrase.Add(new Chunk(Convert.ToString(objCarSummary.CapexLocal), FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.NORMAL, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    cell.PaddingBottom = 8f;  
                    cell.BorderWidthLeft = 0f;
                    cell.BorderWidthTop = 0f;
                    cell.BorderWidthRight = 0.3f;
                    cell.BorderWidthBottom = 0f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //LocalCurrency Label
                    phrase = new Phrase();
                    phrase.Add(new Chunk("Local Currency", FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.BOLD, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    cell.PaddingBottom = 8f;  
                    cell.BorderWidthLeft = 0.3f;
                    cell.BorderWidthTop = 0f;
                    cell.BorderWidthRight = 0f;
                    cell.BorderWidthBottom = 0f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //LocalCurrency Label Value
                    phrase = new Phrase();
                    phrase.Add(new Chunk(":   ", FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.BOLD, BaseColor.BLACK)));
                    phrase.Add(new Chunk(Convert.ToString(objCarSummary.LocalCurency), FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.NORMAL, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    cell.PaddingBottom = 8f;  
                    cell.BorderWidthLeft = 0f;
                    cell.BorderWidthTop = 0f;
                    cell.BorderWidthRight = 0.3f;
                    cell.BorderWidthBottom = 0f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //SpenttodateEUR Label
                    phrase = new Phrase();
                    phrase.Add(new Chunk("Spent/Commited to date", FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.BOLD, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    cell.PaddingBottom = 8f;  
                    cell.BorderWidthLeft = 0.3f;
                    cell.BorderWidthTop = 0f;
                    cell.BorderWidthRight = 0f;
                    cell.BorderWidthBottom = 0f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //SpenttodateEUR Label value
                    phrase = new Phrase();
                    phrase.Add(new Chunk(":   ", FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.BOLD, BaseColor.BLACK)));
                    phrase.Add(new Chunk(Convert.ToString(objCarSummary.SpenttodateEUR), FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.NORMAL, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    cell.PaddingBottom = 8f;  
                    cell.BorderWidthLeft = 0f;
                    cell.BorderWidthTop = 0f;
                    cell.BorderWidthRight = 0.3f;
                    cell.BorderWidthBottom = 0f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //CAPEXThisRequest Label
                    phrase = new Phrase();
                    phrase.Add(new Chunk("CAPEX this request", FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.BOLD, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    cell.PaddingBottom = 8f;  
                    cell.BorderWidthLeft = 0.3f;
                    cell.BorderWidthTop = 0f;
                    cell.BorderWidthRight = 0f;
                    cell.BorderWidthBottom = 0f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //CAPEXThisRequest Label Value
                    phrase = new Phrase();
                    phrase.Add(new Chunk(":   ", FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.BOLD, BaseColor.BLACK)));
                    phrase.Add(new Chunk(Convert.ToString(objCarSummary.CAPEXThisRequest), FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.NORMAL, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    cell.PaddingBottom = 8f;  
                    cell.BorderWidthLeft = 0f;
                    cell.BorderWidthTop = 0f;
                    cell.BorderWidthRight = 0.3f;
                    cell.BorderWidthBottom = 0f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //FinanceLease Label
                    phrase = new Phrase();
                    phrase.Add(new Chunk("Finance Lease", FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.BOLD, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    cell.PaddingBottom = 8f;  
                    cell.BorderWidthLeft = 0.3f;
                    cell.BorderWidthTop = 0f;
                    cell.BorderWidthRight = 0f;
                    cell.BorderWidthBottom = 0f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //FinanceLease Label Value
                    phrase = new Phrase();
                    phrase.Add(new Chunk(":   ", FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.BOLD, BaseColor.BLACK)));
                    phrase.Add(new Chunk(Convert.ToString(objCarSummary.FinanceLease), FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.NORMAL, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    cell.PaddingBottom = 8f;  
                    cell.BorderWidthRight = 0.3f;
                    cell.BorderWidthTop = 0f;
                    cell.BorderWidthLeft = 0f;
                    cell.BorderWidthBottom = 0f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //PurchaseOption Label
                    phrase = new Phrase();
                    phrase.Add(new Chunk("Purchase Option", FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.BOLD, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                     cell.PaddingBottom = 8f;                   
                    cell.BorderWidthLeft = 0.3f;
                    cell.BorderWidthTop = 0f;
                    cell.BorderWidthRight = 0f;
                    cell.BorderWidthBottom = 0f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //PurchaseOption Label Value
                    phrase = new Phrase();
                    phrase.Add(new Chunk(":   ", FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.BOLD, BaseColor.BLACK)));
                    phrase.Add(new Chunk(Convert.ToString(objCarSummary.PurchaseOption), FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.NORMAL, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    cell.PaddingBottom = 8f;  
                    cell.BorderWidthLeft = 0f;
                    cell.BorderWidthTop = 0f;
                    cell.BorderWidthRight = 0.3f;
                    cell.BorderWidthBottom = 0f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //GlobalRealEstate Label
                    phrase = new Phrase();
                    phrase.Add(new Chunk("Operating Lease/Global Real Estate", FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.BOLD, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    cell.PaddingBottom = 8f;  
                    cell.BorderWidthLeft = 0.3f;
                    cell.BorderWidthTop = 0f;
                    cell.BorderWidthRight = 0f;
                    cell.BorderWidthBottom = 0f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //GlobalRealEstate Label Value
                    phrase = new Phrase();
                    phrase.Add(new Chunk(":   ", FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.BOLD, BaseColor.BLACK)));
                    phrase.Add(new Chunk(Convert.ToString(objCarSummary.GlobalRealEstate), FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.NORMAL, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    cell.PaddingBottom = 8f;  
                    cell.BorderWidthLeft = 0f;
                    cell.BorderWidthTop = 0f;
                    cell.BorderWidthRight = 0.3f;
                    cell.BorderWidthBottom = 0f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //NoOfYears Label
                    phrase = new Phrase();
                    phrase.Add(new Chunk("No.Of Years", FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.BOLD, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    cell.PaddingBottom = 8f;  
                    cell.BorderWidthLeft= 0.3f;
                    cell.BorderWidthBottom = 0.3f;
                    cell.BorderWidthRight = 0f;
                    cell.BorderWidthTop = 0f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //NoOfYears Label value
                    phrase = new Phrase();
                    phrase.Add(new Chunk(":   ", FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.BOLD, BaseColor.BLACK)));
                    phrase.Add(new Chunk(Convert.ToString(objCarSummary.NoOfYears), FontFactory.GetFont(pdfCellFontName, pdfCellFontSize, Font.NORMAL, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                    cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    cell.PaddingBottom = 8f;  
                    cell.BorderWidthLeft = 0f;
                    cell.BorderWidthTop = 0f;
                    cell.BorderWidthRight = 0.3f;
                    cell.BorderWidthBottom = 0.3f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);                    
                    //add summary table information to pdf document
                    document.Add(table);

                    //Adding car capex matrix details
                    table = new PdfPTable(5);
                    table.HorizontalAlignment = 0;
                    table.SpacingBefore = 5f;
                    table.TotalWidth = 475f;
                    table.LockedWidth = true;
                    float[] widths = new float[] { 110f, 90f, 90f, 90f, 90f };
                    table.SetWidths(widths);
                    //Header
                    phrase = new Phrase();
                    phrase.Add(new Chunk(ConfigurationManager.AppSettings["CARBackendPDFHeading3"], FontFactory.GetFont(pdfCellFontName, 10, Font.BOLD, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_CENTER);
                    cell.Colspan = 5;
                    cell.PaddingTop = 10f;
                    cell.PaddingBottom = 10f;
                    table.AddCell(cell);


                    //writing car matrix table columns header
                   
                    phrase = new Phrase();
                    phrase.Add(new Chunk("CAPEX Marix", FontFactory.GetFont(pdfCellFontName, pdfCellSummaryFontSize, Font.BOLD, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_CENTER);
                    cell.PaddingTop = 5f;
                    cell.PaddingBottom = 8f;  
                    cell.BorderWidth = 0.3f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                   
                    phrase = new Phrase();
                    phrase.Add(new Chunk("Current Year", FontFactory.GetFont(pdfCellFontName, pdfCellSummaryFontSize, Font.BOLD, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_CENTER);
                    cell.PaddingTop = 5f;
                    cell.PaddingBottom = 8f;  
                    cell.BorderWidth = 0.3f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);

                    phrase = new Phrase();
                    phrase.Add(new Chunk("Year + 1", FontFactory.GetFont(pdfCellFontName, pdfCellSummaryFontSize, Font.BOLD, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_CENTER);
                    cell.PaddingTop = 5f;
                    cell.PaddingBottom = 8f;  
                    cell.BorderWidth = 0.3f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);


                    phrase = new Phrase();
                    phrase.Add(new Chunk("Year + 2", FontFactory.GetFont(pdfCellFontName, pdfCellSummaryFontSize, Font.BOLD, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_CENTER);
                    cell.PaddingTop = 5f;
                    cell.PaddingBottom = 8f;  
                    cell.BorderWidth = 0.3f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);

                    phrase = new Phrase();
                    phrase.Add(new Chunk("Total(Sum)", FontFactory.GetFont(pdfCellFontName, pdfCellSummaryFontSize, Font.BOLD, BaseColor.BLACK)));
                    cell = PhraseCell(phrase, PdfPCell.ALIGN_CENTER);
                    cell.PaddingTop = 5f;
                    cell.PaddingBottom = 8f;  
                    cell.BorderWidth = 0.3f;
                    cell.BorderColor = BaseColor.BLACK;
                    table.AddCell(cell);
                    //writing car matrix table rows 
                    foreach (CarCapexMatrix carCapexMatrix in lstCarCapex)
                    {
                        //Capex Matrix Description value
                        phrase = new Phrase();
                        phrase.Add(new Chunk(carCapexMatrix.CapexMatricDescription, FontFactory.GetFont(pdfCellFontName, pdfCellSummaryFontSize, Font.NORMAL, BaseColor.BLACK)));
                        cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
                        cell.BorderWidth = 0.3f;
                        cell.BorderColor = BaseColor.BLACK;
                        cell.PaddingBottom = 8f;  
                        table.AddCell(cell);
                        //Year +1 value
                        phrase = new Phrase();
                        phrase.Add(new Chunk(Convert.ToString(carCapexMatrix.Y1), FontFactory.GetFont(pdfCellFontName, pdfCellSummaryFontSize, Font.NORMAL, BaseColor.BLACK)));
                        cell = PhraseCell(phrase, PdfPCell.ALIGN_CENTER);
                        cell.BorderWidth = 0.3f;
                        cell.BorderColor = BaseColor.BLACK;
                        cell.PaddingBottom = 8f;  
                        table.AddCell(cell);
                        //Year + 2 value
                        phrase = new Phrase();
                        phrase.Add(new Chunk(Convert.ToString(carCapexMatrix.Y2), FontFactory.GetFont(pdfCellFontName, pdfCellSummaryFontSize, Font.NORMAL, BaseColor.BLACK)));
                        cell = PhraseCell(phrase, PdfPCell.ALIGN_CENTER);
                        cell.BorderWidth = 0.3f;
                        cell.BorderColor = BaseColor.BLACK;
                        cell.PaddingBottom = 8f;  
                        table.AddCell(cell);
                        //Year + 3 value
                        phrase = new Phrase();
                        phrase.Add(new Chunk(Convert.ToString(carCapexMatrix.Y3), FontFactory.GetFont(pdfCellFontName, pdfCellSummaryFontSize, Font.NORMAL, BaseColor.BLACK)));
                        cell = PhraseCell(phrase, PdfPCell.ALIGN_CENTER);
                        cell.BorderWidth = 0.3f;
                        cell.BorderColor = BaseColor.BLACK;
                        cell.PaddingBottom = 8f;  
                        table.AddCell(cell);
                        //Total Sum Value
                        phrase = new Phrase();
                        phrase.Add(new Chunk(Convert.ToString(carCapexMatrix.TotalSum), FontFactory.GetFont(pdfCellFontName, pdfCellSummaryFontSize, Font.NORMAL, BaseColor.BLACK)));
                        cell = PhraseCell(phrase, PdfPCell.ALIGN_CENTER);
                        cell.BorderWidth = 0.3f;
                        cell.BorderColor = BaseColor.BLACK;
                        cell.PaddingBottom = 8f;  
                        table.AddCell(cell);                        
                    }
                    //add capex matrix table to pdf document
                    document.Add(table);
                    //close the document
                    document.Close();
                    // Close the writer instance
                    writer.Close();
                    // Always close open filehandles explicity
                    fs.Close();

                    //upload file into azure blob container
                    if (this.executeazCopyExe(PdfBucketPath, requestID, pdfFileName))
                    {
                        //if file upload successsfully into blob container then delete the directory and files 
                        this.clearfiles(PdfBucketPath);
                    }
                }
                InsightLogger.TrackEndEvent(callerMethodName);
            }
            catch (DataAccessException dalexception)
            {
                throw dalexception;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                throw new BusinessLogicException(exception.Message, exception.InnerException);
            }
        }
    }
}
