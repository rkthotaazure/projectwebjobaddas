//-----------------------------------------------------------
// <copyright file="UpdateTriggeringRules.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using adidas.clb.job.UpdateTriggering.Exceptions;
using adidas.clb.job.UpdateTriggering.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adidas.clb.job.UpdateTriggering.App_Data.BAL
{
    public  class UpdateTriggeringRules
    {
        //Application insights interface reference for logging the error details into Application Insight azure service.
        static IAppInsight InsightLogger { get { return AppInsightLogger.Instance; } }
        /// <summary>
        /// Update Triggering Rule R1 ::Get next collecting Time
        /// </summary>
        /// <param name="minUpdateFrequency"></param>
        /// <returns></returns>
        public int GetNextUserCollectingHours(int minUpdateFrequency)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                int nxtCollectingHours = Convert.ToInt32(ConvertDaysToHours(minUpdateFrequency)) / 2;
                return nxtCollectingHours;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                throw new BusinessLogicException(exception.Message, exception.InnerException);
            }
        }
        /// <summary>
        /// Update Triggering Rule R2 :: Is user backend needs update or not
        /// </summary>
        /// <param name="UserBackendUpdateTriggered"></param>
        /// <param name="UserBackendLastUpdate"></param>
        /// <param name="UserBackendUpdateFrequency"></param>
        /// <returns></returns>
        public  bool IsuserBackendNeedsUpdate(bool UserBackendUpdateTriggered, DateTime UserBackendLastUpdate, int UserBackendUpdateFrequency)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                bool IsNeedsUpdate = false;
                //Update triggering Rules (R2) :: UserNeedsUpdate=(NOT UserBackend.UpdateTriggered AND (UserBackend.lastUpdate + UserBackend.UpdateFrequency >Now))
                if ((!UserBackendUpdateTriggered) && (UserBackendLastUpdate.AddSeconds(ConvertMinutesToSeconds(UserBackendUpdateFrequency)) > DateTime.UtcNow))
                {
                    IsNeedsUpdate = true;
                }
                return IsNeedsUpdate;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                throw new BusinessLogicException(exception.Message, exception.InnerException);
            }
        }
      
        /// <summary>
        /// Update Triggering Rule R3 :: calculating backend backend expected update time
        /// //
        /// </summary>
        /// <param name="BackendAverageALLRequestLatency"></param>
        /// <param name="BackendLastALLRequestLatency"></param>
        /// <returns></returns>
        public DateTime GetUserBackendExpectedUpdate(double BackendAverageALLRequestLatency, int BackendLastALLRequestLatency)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                DateTime expectedUpdateTime;
                //Now + Max(Backend.AverageAllRequestsLatency;Backend.LastALLRequestLatency)*1.2
                expectedUpdateTime = DateTime.UtcNow.AddSeconds((Math.Max(BackendAverageALLRequestLatency, Convert.ToDouble(BackendLastALLRequestLatency)) * Convert.ToDouble(System.Configuration.ConfigurationManager.AppSettings["ConstantFraction"])));
                InsightLogger.TrackEvent("adidas.clb.job.UpdateTriggering web job :: ProcessQueueMessage, Action :: Compute and set Expected Updated Timestamp for Userbackend(UT Rule :: R3) ,  Response : Expected Update Time ::" + Convert.ToString(expectedUpdateTime));
                return expectedUpdateTime;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                throw new BusinessLogicException(exception.Message, exception.InnerException);
            }
        }
        /// <summary>
        /// Update Triggering Rule R4 :: calculating request expected update time
        /// </summary>
        /// <param name="BackendAverageRequestLatency"></param>
        /// <param name="BackendLastRequestLatency"></param>
        /// <returns></returns>
        public DateTime GetRequestExpectedUpdate(double BackendAverageRequestLatency, int BackendLastRequestLatency)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                DateTime requestExpectedUpdateTime;
                //Now + Max(Backend.AverageRequestsLatency;Backend.LastRequestLatency)*1.2
                requestExpectedUpdateTime = DateTime.UtcNow.AddSeconds((Math.Max(BackendAverageRequestLatency, Convert.ToDouble(BackendLastRequestLatency)) * Convert.ToDouble(System.Configuration.ConfigurationManager.AppSettings["ConstantFraction"])));
                InsightLogger.TrackEvent("adidas.clb.job.UpdateTriggering web job :: ProcessQueueMessage, Action :: Compute and set Expected Updated Timestamp for Request(UT Rule :: R4) ,  Response : Expected Update Time ::" + Convert.ToString(requestExpectedUpdateTime));
                return requestExpectedUpdateTime;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);

                throw new BusinessLogicException(exception.Message, exception.InnerException);
            }
        }
      
        /// <summary>
        /// Update Triggering Rule R5 :: Get next missing collection time for the user
        /// </summary>
        /// <param name="MissedUpdateLastCollectingTime"></param>
        /// <param name="BackendAverageAllRequestsLatency"></param>
        /// <param name="BackendLastAllRequestsLatency"></param>
        /// <returns></returns>
        public DateTime GetNextMissingCollectingTime(DateTime MissedUpdateLastCollectingTime,int BackendAverageAllRequestsLatency, int BackendLastAllRequestsLatency)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                DateTime nextMissingCollectingTime;
                //Now + Max(Backend.AverageRequestsLatency;Backend.LastRequestLatency)*1.2
                nextMissingCollectingTime = MissedUpdateLastCollectingTime.AddSeconds((Math.Max(BackendAverageAllRequestsLatency, BackendLastAllRequestsLatency)) / 2);
                return nextMissingCollectingTime;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);

                throw new BusinessLogicException(exception.Message, exception.InnerException);
            }
        }
        /// <summary>
        /// Update Triggering Rule R6 :: checking user backend update is missing or not
        /// </summary>
        /// <param name="UserBackendUpdateTriggered"></param>
        /// <param name="UserBackendExpectedUpdate"></param>
        /// <returns></returns>
        public bool IsUserUpdateMissing(bool UserBackendUpdateTriggered, DateTime UserBackendExpectedUpdate)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                bool IsUpdateMissing=false;
                //Rule R6 :: UserBackend.UpdateTriggered AND (NOW >UserBackend.ExpectedUpdate)
                if ((UserBackendUpdateTriggered) && (DateTime.UtcNow > UserBackendExpectedUpdate))
                {
                    IsUpdateMissing = true;
                }
                return IsUpdateMissing;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);

                throw new BusinessLogicException(exception.Message, exception.InnerException);
            }
        }
        /// <summary>
        /// Update Triggering Rule R6 :: checking request update is missing or not
        /// </summary>
        /// <param name="RequestUpdateTriggered"></param>
        /// <param name="RequestExpectedUpdate"></param>
        /// <returns></returns>
        public bool IsRequestUpdateMissing(bool RequestUpdateTriggered, DateTime RequestExpectedUpdate)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                bool IsUpdateMissing = false;
                //Rule R6 :: UserBackend.UpdateTriggered AND (NOW >UserBackend.ExpectedUpdate)
                if ((RequestUpdateTriggered) && (DateTime.UtcNow > RequestExpectedUpdate))
                {
                    IsUpdateMissing = true;
                }
                return IsUpdateMissing;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);

                throw new BusinessLogicException(exception.Message, exception.InnerException);
            }
        }
        /// <summary>
        /// This method Convert Days To Minutes
        /// </summary>
        /// <param name="days"></param>
        /// <returns></returns>
        public static double ConvertDaysToMinutes(double days)
        {
            return TimeSpan.FromDays(days).TotalMinutes;
        }
        /// <summary>
        /// This method Convert Days to Hours
        /// </summary>
        /// <param name="days"></param>
        /// <returns></returns>
        public static double ConvertDaysToHours(double days)
        {
            return TimeSpan.FromDays(days).TotalHours;
        }
        /// <summary>
        /// This method Convert Days to Seconds
        /// </summary>
        /// <param name="days"></param>
        /// <returns></returns>
        public static double ConvertDaysToSeconds(double days)
        {
            return TimeSpan.FromDays(days).TotalSeconds;
        }
        public static double ConvertMinutesToSeconds(double minutes)
        {
            return TimeSpan.FromMinutes(minutes).TotalSeconds;
        }

    }
}
