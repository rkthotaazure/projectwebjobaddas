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
    public class UserBackendDAL
    {
        /// <summary>
        ///method to get list all backends
        /// </summary>
        /// <param name="userprops"></param>
        /// <returns></returns>        
        public List<BackendEntity> GetBackends()
        {
            PersonalizationDAL personalizationdal = new PersonalizationDAL();
            CloudTable ReferenceDataTable = personalizationdal.GetAzureTableInstance(CoreConstants.AzureTables.ReferenceData);
            TableQuery<BackendEntity> query = new TableQuery<BackendEntity>().Where(TableQuery.GenerateFilterCondition(CoreConstants.AzureTables.PartitionKey, QueryComparisons.Equal, CoreConstants.AzureTables.Backend));
            List<BackendEntity> allBackends = (List<BackendEntity>)ReferenceDataTable.ExecuteQuery(query);
            return allBackends;            
        }        
    }
}