//-----------------------------------------------------------
// <copyright file="RequestSynchDAL.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using adidas.clb.job.UpdateTriggering.App_Data.BAL;
using adidas.clb.job.UpdateTriggering.Exceptions;
using adidas.clb.job.UpdateTriggering.Helpers;
using adidas.clb.job.UpdateTriggering.Models;
using adidas.clb.job.UpdateTriggering.Utility;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using System.Threading.Tasks;
namespace adidas.clb.job.UpdateTriggering.App_Data.DAL
{
    
    class RequestSynchDAL
    {
        //Application insights interface reference for logging the error details into Application Insight azure service.
        static IAppInsight InsightLogger { get { return AppInsightLogger.Instance; } }
        private UpdateTriggeringRules utRule;
        public RequestSynchDAL()
        {
            utRule = new UpdateTriggeringRules();
        }
        /// <summary>
        /// This method returns the missing request which are missed updates based on username
        /// </summary>
        /// <param name="UserName"></param>
        /// <returns></returns>
        //public List<RequestSynchEntity> GetAllMissingUpdateRequestsForuserBackend(string UserName)
        //{
        //    string callerMethodName = string.Empty;
        //    try
        //    {
        //        //Get Caller Method name from CallerInformation class
        //        callerMethodName = CallerInformation.TrackCallerMethodName();
        //        List<RequestSynchEntity> lstMissingUpdateRequests = new List<RequestSynchEntity>();
        //        CloudTable UserDeviceConfigurationTable = DataProvider.GetAzureTableInstance(ConfigurationManager.AppSettings["AzureTables.UserDeviceConfiguration"]);
        //        //Get all the requests associated with the user
        //        TableQuery<RequestSynchEntity> tquery = new TableQuery<RequestSynchEntity>().Where(TableQuery.GenerateFilterCondition(CoreConstants.AzureTables.PartitionKey, QueryComparisons.Equal, CoreConstants.AzureTables.UserBackendPK + UserName));
        //        List<RequestSynchEntity> allRequests = UserDeviceConfigurationTable.ExecuteQuery(tquery).ToList();
        //        foreach (RequestSynchEntity entity in allRequests)
        //        {
        //            //Checking, is the request update missing or not with the help of update triggering Rule R6
        //            if (utRule.IsRequestUpdateMissing(entity.UpdateTriggered, entity.ExpectedUpdate))
        //            {
        //                //adding missing update request to list
        //                lstMissingUpdateRequests.Add(entity);

        //            }
        //        }
        //        return lstMissingUpdateRequests;
        //    }
        //    catch (Exception exception)
        //    {
        //        InsightLogger.Exception(exception.Message, exception, callerMethodName);
        //        throw new DataAccessException(exception.Message, exception.InnerException);
        //    }
        //}
    }
}
