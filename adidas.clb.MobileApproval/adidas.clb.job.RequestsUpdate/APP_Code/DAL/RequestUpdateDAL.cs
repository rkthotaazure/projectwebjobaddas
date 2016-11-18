//-----------------------------------------------------------
// <copyright file="RequestUpdateDAL.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using adidas.clb.job.RequestsUpdate.Exceptions;
using adidas.clb.job.RequestsUpdate.Models;
using adidas.clb.job.RequestsUpdate.Utility;
using Newtonsoft.Json;

namespace adidas.clb.job.RequestsUpdate.APP_Code.DAL
{
    /// <summary>
    /// class which implements methods for data access layer of RequestUpdate.
    /// </summary>
    public class RequestUpdateDAL
    {

        /// <summary>
        /// method to add Request entity to azure table
        /// </summary>
        /// <param name="request">takes request entity as input</param>
        public void AddRequest(RequsetEntity request)
        {
            try
            {
                //get's azure table instance            
                CloudTable ReferenceDataTable = DataProvider.GetAzureTableInstance(CoreConstants.AzureTables.RequestTransactions);
                //inserts entity in to azure table            
                TableOperation insertOperation = TableOperation.Insert(request);                
                ReferenceDataTable.Execute(insertOperation);
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error while inserting request into requestTransactions azure table in DAL : "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new DataAccessException();
            }
        }

        public void AddApproval(ApprovalEntity approval)
        {
            try
            {
                //get's azure table instance            
                CloudTable ReferenceDataTable = DataProvider.GetAzureTableInstance(CoreConstants.AzureTables.RequestTransactions);
                //inserts entity in to azure table            
                TableOperation insertOperation = TableOperation.Insert(approval);
                ReferenceDataTable.Execute(insertOperation);
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error while inserting approval into requestTransactions azure table in DAL : "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new DataAccessException();
            }
        }
        /// <summary>
        /// method to add approver entities to azure table
        /// </summary>
        /// <param name="approvers">takes approver entities as input</param>
        public void AddApprovers(List<ApproverEntity> approvers)
        {
            try
            {
                //get's azure table instance
                CloudTable UserDeviceConfigurationTable = DataProvider.GetAzureTableInstance(CoreConstants.AzureTables.RequestTransactions);
                TableBatchOperation batchOperation = new TableBatchOperation();
                //insert list of approver entities into batch operation
                foreach (ApproverEntity approverentity in approvers)
                {
                    batchOperation.Insert(approverentity);
                }
                UserDeviceConfigurationTable.ExecuteBatch(batchOperation);
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error while inserting approvers into requestTransactions azure table in DAL : "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new DataAccessException();
            }
        }

        /// <summary>
        /// method to add fields entities to azure table
        /// </summary>
        /// <param name="fields">takes fields entities as input</param>
        public void AddFields(List<FieldEntity> fields)
        {
            try
            {
                //get's azure table instance
                CloudTable UserDeviceConfigurationTable = DataProvider.GetAzureTableInstance(CoreConstants.AzureTables.RequestTransactions);
                TableBatchOperation batchOperation = new TableBatchOperation();
                //insert list of entities into batch operation
                foreach (FieldEntity fieldentity in fields)
                {
                    batchOperation.Insert(fieldentity);
                }
                UserDeviceConfigurationTable.ExecuteBatch(batchOperation);
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error while inserting fields into requestTransactions azure table in DAL : "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new DataAccessException();
            }
        }

        /// <summary>
        /// method to add Request PDF to Blob
        /// </summary>
        /// <param name="urivalue">takes temp blob uri as input</param>
        /// <returns>returns request pdf blob uri</returns>
        public Uri AddRequestPDFToBlob(Uri urivalue, string RequestID)
        {
            try
            {
                //storage credentials for temp storage blob
                StorageCredentials storageCredentials = new StorageCredentials(CloudConfigurationManager.GetSetting(CoreConstants.AzureBlob.AzureTempBlobStorageAccountName), CloudConfigurationManager.GetSetting(CoreConstants.AzureBlob.AzureTempBlobStorageAccountKey));
                //get temp storage blob with uri
                CloudBlockBlob blob = new CloudBlockBlob(urivalue, storageCredentials);
                //read pdf from temp storage blob
                using (MemoryStream ms = new MemoryStream())
                {
                    blob.DownloadToStream(ms);
                    //get Requet update blob storage instance                
                    CloudBlobContainer container = DataProvider.GetAzureBlobContainerInstance(CoreConstants.AzureBlob.BlobRequsetPDF);
                    string blobname = string.Concat(CoreConstants.AzureBlob.RequestPDF, RequestID, CoreConstants.AzureBlob.RequestPDFExtension);
                    CloudBlockBlob targetblob = container.GetBlockBlobReference(blobname);
                    //properties to upload to blob as type PDF
                    targetblob.Properties.ContentType = CoreConstants.AzureBlob.PDFContentType;
                    ms.Position = 0;
                    //upload request pdf to blob
                    targetblob.UploadFromStream(ms);
                    return targetblob.Uri;
                }
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error while inserting request update PDF into requestpdf azure blob in DAL : "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new DataAccessException();
            }
        }

        /// <summary>
        /// method to add Request PDF uri to Request entity
        /// </summary>
        /// <param name="urivalue">takes temp blob uri as input</param>        
        public void AddPDFUriToRequest(Uri urivalue, string RequestID)
        {
            try
            {
                //get's azure table instance
                CloudTable UserDeviceConfigurationTable = DataProvider.GetAzureTableInstance(CoreConstants.AzureTables.RequestTransactions);
                // Create a retrieve operation that takes a customer entity.
                TableOperation retrieveOperation = TableOperation.Retrieve<RequsetEntity>(CoreConstants.AzureTables.RequestsPK, RequestID);
                // Execute the operation.
                TableResult retrievedRequest = UserDeviceConfigurationTable.Execute(retrieveOperation);
                // Assign the result to a CustomerEntity object.
                RequsetEntity updateEntity = (RequsetEntity)retrievedRequest.Result;
                //check for null
                if (updateEntity != null)
                {
                    // Add the PDFUri.
                    updateEntity.PDFUri = urivalue.ToString();
                    // Create the Replace TableOperation.
                    TableOperation updateOperation = TableOperation.Replace(updateEntity);
                    // Execute the operation.
                    UserDeviceConfigurationTable.Execute(updateOperation);
                }

            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error while updating request update PDF uri into request entitty in DAL : "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new DataAccessException();
            }
        }

        /// <summary>
        /// method to get get user backend
        /// </summary>
        /// <param name="userid">takes userid as input</param>
        /// <param name="backendid">takes backendid as input</param>
        /// <returns>returns user backend</returns>
        public UserBackendEntity GetUserBackend(string userid, string backendid)
        {
            try
            {
                //get's azure table instance
                CloudTable UserDeviceConfigurationTable = DataProvider.GetAzureTableInstance(CoreConstants.AzureTables.UserDeviceConfiguration);
                // Create a retrieve operation that takes a customer entity.
                TableOperation retrieveOperation = TableOperation.Retrieve<UserBackendEntity>(string.Concat(CoreConstants.AzureTables.UserBackendPK, userid), backendid);
                // Execute the operation.
                TableResult retrievedRequest = UserDeviceConfigurationTable.Execute(retrieveOperation);
                // Assign the result to a CustomerEntity object.
                UserBackendEntity userbackend = (UserBackendEntity)retrievedRequest.Result;
                return userbackend;
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error while getting userbackend entity in DAL : "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new DataAccessException();
            }
        }

        /// <summary>
        /// method to get open requests count for user backend
        /// </summary>
        /// <param name="userid">takes userid as input</param>
        /// <param name="backendid">takes backendid as input</param>
        /// <returns>returns count of open requets per user backend</returns>
        public int GetOpenRequestsCount(string userid, string backendid)
        {
            try
            {
                //get's azure table instance
                CloudTable UserDeviceConfigurationTable = DataProvider.GetAzureTableInstance(CoreConstants.AzureTables.RequestTransactions);
                //adding filters to get count of open requests
                string partitionFilter = TableQuery.GenerateFilterCondition(CoreConstants.AzureTables.PartitionKey, QueryComparisons.Equal, string.Concat(CoreConstants.AzureTables.RequestsPK, userid));
                string rowfilter = TableQuery.GenerateFilterCondition(CoreConstants.AzureTables.BackendId, QueryComparisons.Equal, backendid);
                string statusfilter = TableQuery.GenerateFilterCondition(CoreConstants.AzureTables.Status, QueryComparisons.Equal, CoreConstants.AzureTables.InProgress);
                string finalFilter = TableQuery.CombineFilters(TableQuery.CombineFilters(partitionFilter, TableOperators.And, rowfilter), TableOperators.And, statusfilter);
                //selecting only few columnas as we are just caliculating count of open requests
                var query = new TableQuery<RequsetEntity>()
                {
                    SelectColumns = new List<string>()
                    {
                        CoreConstants.AzureTables.PartitionKey, CoreConstants.AzureTables.BackendId
                    }
                }.Where(TableQuery.GenerateFilterCondition(CoreConstants.AzureTables.RequestsPK, QueryComparisons.Equal, CoreConstants.AzureTables.FieldPK));
                //executing query to find count of open requests
                int openrequests = UserDeviceConfigurationTable.ExecuteQuery<RequsetEntity>(query, null).Count();
                return openrequests;
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error while getting open requets count in DAL : "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new DataAccessException();
            }

        }

        /// <summary>
        /// method to get count of open approvals per userbackend
        /// </summary>
        /// <param name="userid">takes userid as input</param>
        /// <param name="backendid">takes backendid as input</param>
        /// <returns>returns count of open approvals per user backend</returns>
        public int GetOpenApprovalsCount(string userid, string backendid)
        {
            try
            {
                //get's azure table instance
                CloudTable UserDeviceConfigurationTable = DataProvider.GetAzureTableInstance(CoreConstants.AzureTables.RequestTransactions);
                //adding filters to get count of open approvals
                string partitionFilter = TableQuery.GenerateFilterCondition(CoreConstants.AzureTables.PartitionKey, QueryComparisons.Equal, string.Concat(CoreConstants.AzureTables.ApprovalPK,userid));
                string rowfilter = TableQuery.GenerateFilterCondition(CoreConstants.AzureTables.BackendId, QueryComparisons.Equal, backendid);
                string statusfilter = TableQuery.GenerateFilterCondition(CoreConstants.AzureTables.Status, QueryComparisons.Equal, CoreConstants.AzureTables.Waiting);
                string finalFilter = TableQuery.CombineFilters(TableQuery.CombineFilters(partitionFilter, TableOperators.And, rowfilter), TableOperators.And, statusfilter);
                //selecting only few columnas as we are just caliculating count of open approvals
                var query = new TableQuery<RequsetEntity>()
                {
                    SelectColumns = new List<string>()
                    {
                        CoreConstants.AzureTables.PartitionKey, CoreConstants.AzureTables.BackendId
                    }
                }.Where(TableQuery.GenerateFilterCondition(CoreConstants.AzureTables.RequestsPK, QueryComparisons.Equal, CoreConstants.AzureTables.FieldPK));
                //executing query to find count of open requests
                int openapprovals = UserDeviceConfigurationTable.ExecuteQuery<RequsetEntity>(query, null).Count();
                return openapprovals;
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error while getting open approvals count in DAL : "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new DataAccessException();
            }
        }

        /// <summary>
        /// method to get count of urgent approvals per userbackend
        /// </summary>
        /// <param name="userid">takes userid as input</param>
        /// <param name="backendid">takes backendid as input</param>
        /// <returns>returns count of urgent approvals per user backend</returns>
        public int GetUrgentApprovalsCount(string userid, string backendid)
        {
            try
            {
                //get's azure table instance
                CloudTable UserDeviceConfigurationTable = DataProvider.GetAzureTableInstance(CoreConstants.AzureTables.RequestTransactions);
                //adding filters to get count of open approvals
                string partitionFilter = TableQuery.GenerateFilterCondition(CoreConstants.AzureTables.PartitionKey, QueryComparisons.Equal, string.Concat(CoreConstants.AzureTables.ApprovalPK, userid));
                string rowfilter = TableQuery.GenerateFilterCondition(CoreConstants.AzureTables.BackendId, QueryComparisons.Equal, backendid);
                string statusfilter = TableQuery.GenerateFilterConditionForDate(CoreConstants.AzureTables.DueDate, QueryComparisons.LessThanOrEqual, DateTime.Today);
                string finalFilter = TableQuery.CombineFilters(TableQuery.CombineFilters(partitionFilter, TableOperators.And, rowfilter), TableOperators.And, statusfilter);
                //selecting only few columnas as we are just caliculating count of urgent approvals
                var query = new TableQuery<RequsetEntity>()
                {
                    SelectColumns = new List<string>()
                    {
                        CoreConstants.AzureTables.PartitionKey, CoreConstants.AzureTables.BackendId
                    }
                }.Where(TableQuery.GenerateFilterCondition(CoreConstants.AzureTables.RequestsPK, QueryComparisons.Equal, CoreConstants.AzureTables.FieldPK));
                //executing query to find count of urgent requests
                int urgentapprovals = UserDeviceConfigurationTable.ExecuteQuery<RequsetEntity>(query, null).Count();
                return urgentapprovals;
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error while getting urgent approvals count in DAL : "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new DataAccessException();
            }
        }

        /// <summary>
        /// method to update userbackend
        /// </summary>
        /// <param name="userbackend">takes userbackend entity as input</param>
        public void UpdateUserBackend(UserBackendEntity userbackend)
        {
            try
            {
                //get's azure table instance
                CloudTable UserDeviceConfigurationTable = DataProvider.GetAzureTableInstance(CoreConstants.AzureTables.UserDeviceConfiguration);
                // Create the Replace TableOperation.
                TableOperation updateOperation = TableOperation.Replace(userbackend);
                // Execute the operation.
                UserDeviceConfigurationTable.Execute(updateOperation);
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error while updating userbackend to azure table in DAL : "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new DataAccessException();
            }
        }

        /// <summary>
        /// method to get backend entity
        /// </summary>
        /// <param name="backendid">tackes backendId as input</param>
        /// <returns>returns backend entity</returns>
        public BackendEntity GetBackend(string backendid)
        {
            try
            {
                //get's azure table instance
                CloudTable UserDeviceConfigurationTable = DataProvider.GetAzureTableInstance(CoreConstants.AzureTables.ReferenceData);
                // Create a retrieve operation that takes a customer entity.
                TableOperation retrieveOperation = TableOperation.Retrieve<BackendEntity>(CoreConstants.AzureTables.BackendPK, backendid);
                // Execute the operation.
                TableResult retrievedRequest = UserDeviceConfigurationTable.Execute(retrieveOperation);
                // Assign the result to a CustomerEntity object.
                BackendEntity backend = (BackendEntity)retrievedRequest.Result;
                return backend;
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error while getting backend from azure table in DAL : "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new DataAccessException();
            }
        }

        /// <summary>
        /// method to update backend
        /// </summary>
        /// <param name="backend">takes backend entity as input</param>
        public void UpdateBackend(BackendEntity backend)
        {
            try
            {
                //get's azure table instance
                CloudTable UserDeviceConfigurationTable = DataProvider.GetAzureTableInstance(CoreConstants.AzureTables.ReferenceData);
                // Create the Replace TableOperation.
                TableOperation updateOperation = TableOperation.Replace(backend);
                // Execute the operation.
                UserDeviceConfigurationTable.Execute(updateOperation);
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error while updating backend to azure table in DAL : "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new DataAccessException();
            }
        }
    }
}
