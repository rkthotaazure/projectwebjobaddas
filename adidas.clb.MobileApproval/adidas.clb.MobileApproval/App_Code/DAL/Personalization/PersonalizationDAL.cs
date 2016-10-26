using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using adidas.clb.MobileApproval.Models;
using adidas.clb.MobileApproval.Utility;

namespace adidas.clb.MobileApproval.App_Code.DAL.Personalization
{
    public class PersonalizationDAL
    {
        /// <summary>
        /// method to get user details
        /// </summary>
        /// <param name="userprops"></param>
        /// <returns></returns>        
        public UserEntity GetUser(String UserName)
        {
            CloudTable ReferenceDataTable = GetAzureTableInstance(CoreConstants.AzureTables.ReferenceData);
            TableOperation RetrieveUser = TableOperation.Retrieve<UserEntity>(CoreConstants.AzureTables.User, UserName);
            TableResult RetrievedResultUser = ReferenceDataTable.Execute(RetrieveUser);
            return (UserEntity)RetrievedResultUser.Result;            
        }

        /// <summary>
        /// method to get user details
        /// </summary>
        /// <param name="userprops"></param>
        /// <returns></returns>        
        public CloudTable GetAzureTableInstance(String TableName)
        {
            // Retrieve the storage account from the connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
            CloudConfigurationManager.GetSetting("AzureStorageConnectionString"));
            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            // Create the CloudTable object that represents the "backends" table.
            CloudTable table = tableClient.GetTableReference(TableName);
            return table;
        }
    }
}