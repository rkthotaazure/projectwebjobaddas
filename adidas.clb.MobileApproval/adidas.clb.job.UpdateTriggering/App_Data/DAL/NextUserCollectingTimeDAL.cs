//-----------------------------------------------------------
// <copyright file="NextUserCollectingTimeDAL.cs" company="adidas AG">
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
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Newtonsoft.Json;
namespace adidas.clb.job.UpdateTriggering.App_Data.DAL
{
    class NextUserCollectingTimeDAL
    {
        //Application insights interface reference for logging the error details into Application Insight azure service.
        static IAppInsight InsightLogger { get { return AppInsightLogger.Instance; } }
        public static string azureTableReference = ConfigurationManager.AppSettings["AzureTables.ReferenceData"];
        public static string azureTableUserDeviceConfiguration = ConfigurationManager.AppSettings["AzureTables.UserDeviceConfiguration"];
        //RequestTransactions
        public static string azureTableRequestTransactions = ConfigurationManager.AppSettings["AzureTables.RequestTransactions"];
        //get NextCollectingTime Dividend from config
        public static int nextCollectingTimeDividend = Convert.ToInt32(ConfigurationManager.AppSettings["NextCollectingTimeDividend"]);
        //get default MinUpdateFrequency from config
        public static int defaultMinUpdateFrequency = Convert.ToInt32(ConfigurationManager.AppSettings["DefaultMinUpdateFrequency"]);
        private UserBackendDAL objdal;
        private UpdateTriggeringRules utRules;
        public NextUserCollectingTimeDAL()
        {
            objdal = new UserBackendDAL();
            utRules = new UpdateTriggeringRules();
        }
        /// <summary>
        /// Update next collecting time for each backend
        /// </summary>
        public void UpdateNextCollectingTime()
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                //get's azure table instance
                CloudTable UserDeviceConfigurationTable = DataProvider.GetAzureTableInstance(azureTableUserDeviceConfiguration);
                //Get all backends          
                List<BackendEntity> lstbackends = objdal.GetBackends();             
                //for each backend get minimum update frequency for all the userbackend's associated
                Parallel.ForEach<BackendEntity>(lstbackends, backend=>                
                {
                    string backendID = backend.RowKey;                    
                    //Get all the userbackends associated with the backend
                    TableQuery<UserBackendEntity> tquery = new TableQuery<UserBackendEntity>().Where(TableQuery.GenerateFilterCondition(CoreConstants.AzureTables.RowKey, QueryComparisons.Equal, backend.RowKey));
                    List<UserBackendEntity> allUserBackends = UserDeviceConfigurationTable.ExecuteQuery(tquery).ToList();
                    //get minimum update frequency from User Backend list
                    int minUpdateFrequency = allUserBackends.Min(r => r.DefaultUpdateFrequency);
                    InsightLogger.TrackEvent("adidas.clb.job.UpdateTriggering web job, Action :: Collecting the minimum Default Update Frequency of all the userbackends for the backend :" + backendID + " , Response :: Minimum Update Frquency :" + minUpdateFrequency );
                    //Get next collecting hours based on update Triggering Rule :: R1                    
                    int nextCollectingTimeInMinutes;
                    if (minUpdateFrequency < defaultMinUpdateFrequency)
                    {
                        nextCollectingTimeInMinutes = defaultMinUpdateFrequency;
                    }
                    else
                    {
                        nextCollectingTimeInMinutes = minUpdateFrequency / nextCollectingTimeDividend;
                    }
                    //update backend next collecting time in refernecedata table
                    this.InsertorUpdateBackendNextCollectingTime(backendID, nextCollectingTimeInMinutes, backend.AverageAllRequestsLatency, backend.LastAllRequestsLatency);
                  // this.objdal.CollectUsersNeedUpdateByBackend(backendID);
                });


            }
            catch (BusinessLogicException balexception)
            {
                throw balexception;
            }
            catch (DataAccessException dalexception)
            {
                throw dalexception;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                throw new DataAccessException(exception.Message, exception.InnerException);
            }

        }
        /// <summary>
        /// Update or insert NextCollectingTime of the backend
        /// </summary>
        /// <param name="backend"></param>
        /// <param name="minimumUpdateFrequency"></param>
        public void InsertorUpdateBackendNextCollectingTime(string backendID, int minimumUpdateFrequency, int avgAllRequestsLatency, int lastAllRequestsLatency)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                // Create a retrieve operation that takes a NextUserCollectingTime Entity.
                NextUserCollectingTimeEntity ObjNextCollectingTime = DataProvider.RetrieveEntity<NextUserCollectingTimeEntity>(CoreConstants.AzureTables.ReferenceData, CoreConstants.AzureTables.UpdateTriggerNextCollectingTime, backendID);
                if (ObjNextCollectingTime != null)
                {
                    //update the existing entity
                    DateTime lastCollectingTime = ObjNextCollectingTime.RegularUpdateLastCollectingTime;
                    DateTime nextCollectingTime = ObjNextCollectingTime.RegularUpdateNextCollectingTime;
                    DateTime MissedUpdateLastCollectingTime = ObjNextCollectingTime.MissingUpdateLastCollectingTime;
                    DateTime MissedUpdateNextCollectingTime = ObjNextCollectingTime.MissingUpdateNextCollectingTime;
                    //update  MinimumUpdateFrequency
                    ObjNextCollectingTime.MinimumUpdateFrequency = minimumUpdateFrequency;
                    //update  NextCollectingTime value based on new MinimumUpdateFrequency,last collecting Time
                    ObjNextCollectingTime.RegularUpdateNextCollectingTime = lastCollectingTime.AddMinutes(minimumUpdateFrequency);
                    //update Missing Update NextCollectingTime based on updatetriggering Rule R5
                    ObjNextCollectingTime.MissingUpdateNextCollectingTime = utRules.GetNextMissingCollectingTime(MissedUpdateLastCollectingTime, avgAllRequestsLatency, lastAllRequestsLatency);
                    //call dataprovider method to update entity to azure table
                    DataProvider.UpdateEntity<NextUserCollectingTimeEntity>(CoreConstants.AzureTables.ReferenceData, ObjNextCollectingTime);
                }
                else
                {
                    // Create a new NextUserCollectingTime entity.
                    NextUserCollectingTimeEntity nextCollectingTime = new NextUserCollectingTimeEntity(CoreConstants.AzureTables.UpdateTriggerNextCollectingTime, backendID);
                    nextCollectingTime.BackendID = backendID;
                    nextCollectingTime.MinimumUpdateFrequency = minimumUpdateFrequency;
                    DateTime collectingTime = DateTime.UtcNow;
                    nextCollectingTime.RegularUpdateLastCollectingTime = collectingTime;
                    nextCollectingTime.RegularUpdateNextCollectingTime = collectingTime.AddMinutes(minimumUpdateFrequency);
                    nextCollectingTime.MissingUpdateLastCollectingTime = collectingTime;
                    nextCollectingTime.MissingUpdateNextCollectingTime = utRules.GetNextMissingCollectingTime(collectingTime, avgAllRequestsLatency, lastAllRequestsLatency);
                    // Create the TableOperation object that inserts the NextUserCollectingTime entity.                                        
                    DataProvider.InsertEntity<NextUserCollectingTimeEntity>(CoreConstants.AzureTables.ReferenceData, nextCollectingTime);

                }
            }
            catch (BusinessLogicException balexception)
            {
                throw balexception;
            }
            catch (DataAccessException dalexception)
            {
                throw dalexception;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                throw new DataAccessException(exception.Message, exception.InnerException);
            }
        }
        /// <summary>
        /// update regular Next CollectingTime of the backend
        /// </summary>
        /// <param name="backendID"></param>
        /// <param name="minimumUpdateFrequency"></param>
        /// <param name="LastCollectingTime"></param>
        public void UpdateBackendRegularNextCollectingTime(string backendID, int minimumUpdateFrequency, DateTime LastCollectingTime)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                // Create a retrieve operation that takes a NextUserCollectingTime Entity.
                NextUserCollectingTimeEntity ObjUpdateNextCollectingTime = DataProvider.RetrieveEntity<NextUserCollectingTimeEntity>(CoreConstants.AzureTables.ReferenceData, CoreConstants.AzureTables.UpdateTriggerNextCollectingTime, backendID);
                if (ObjUpdateNextCollectingTime != null)
                {
                    //update the existing entity                   
                    DateTime nextCollectingTime = LastCollectingTime.AddMinutes(minimumUpdateFrequency);
                    //update  lastCollectingTime  with previous NextCollectingTime
                    ObjUpdateNextCollectingTime.RegularUpdateLastCollectingTime = LastCollectingTime;
                    //update  NextCollectingTime value based on new MinimumUpdateFrequency,last collecting Time
                    ObjUpdateNextCollectingTime.RegularUpdateNextCollectingTime = nextCollectingTime;
                    // Execute update operation.
                    DataProvider.UpdateEntity<NextUserCollectingTimeEntity>(CoreConstants.AzureTables.ReferenceData, ObjUpdateNextCollectingTime);
                }
            }
            catch (DataAccessException dalexception)
            {
                throw dalexception;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                throw new DataAccessException(exception.Message, exception.InnerException);
            }
        }
        /// <summary>
        /// update  Next CollectingTime of the backend which missed updates
        /// </summary>
        /// <param name="backendID"></param>
        /// <param name="missingUpdateLastCollectingTime"></param>
        /// <param name="avgAllRequestsLatency"></param>
        /// <param name="lastAllRequestsLatency"></param>
        public void UpdateMisseduserBackendNextCollectingTime(string backendID, DateTime missingUpdateLastCollectingTime)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                // Create a retrieve operation that takes a NextUserCollectingTime Entity.
                NextUserCollectingTimeEntity ObjMissedUpdateNextCollectingTime = DataProvider.RetrieveEntity<NextUserCollectingTimeEntity>(CoreConstants.AzureTables.ReferenceData, CoreConstants.AzureTables.UpdateTriggerNextCollectingTime, backendID);
                if (ObjMissedUpdateNextCollectingTime != null)
                {
                    //get backend details by backendid from userconfiguration
                    BackendEntity backendDetails = objdal.GetBackendDetailsByBackendID(backendID);
                    if (backendDetails != null)
                    {

                        //update  Missing Update Last CollectingTime  with previous Missing Update NextCollectingTime
                        ObjMissedUpdateNextCollectingTime.MissingUpdateLastCollectingTime = missingUpdateLastCollectingTime;
                        //update Missing Update NextCollectingTime based on updatetriggering Rule R5
                        ObjMissedUpdateNextCollectingTime.MissingUpdateNextCollectingTime = utRules.GetNextMissingCollectingTime(missingUpdateLastCollectingTime, backendDetails.AverageAllRequestsLatency, backendDetails.LastAllRequestsLatency);
                        // Execute update operation.
                        DataProvider.UpdateEntity<NextUserCollectingTimeEntity>(CoreConstants.AzureTables.ReferenceData, ObjMissedUpdateNextCollectingTime);
                    }

                }
            }
            catch (BusinessLogicException balexception)
            {
                throw balexception;
            }
            catch (DataAccessException dalexception)
            {
                throw dalexception;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                throw new DataAccessException(exception.Message, exception.InnerException);
            }
        }
        /// <summary>
        /// retrieving all backends from NextUserCollectingTimeEntity
        /// </summary>
        /// <returns></returns>
        public List<NextUserCollectingTimeEntity> GetBackendsNeedsUpdate()
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                List<NextUserCollectingTimeEntity> allBackends = DataProvider.RetrieveEntities<NextUserCollectingTimeEntity>(azureTableReference,CoreConstants.AzureTables.UpdateTriggerNextCollectingTime);
                return allBackends;
            }
            catch (DataAccessException dalexception)
            {
                throw dalexception;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                throw new DataAccessException(exception.Message, exception.InnerException);
            }
        }


    }
}
