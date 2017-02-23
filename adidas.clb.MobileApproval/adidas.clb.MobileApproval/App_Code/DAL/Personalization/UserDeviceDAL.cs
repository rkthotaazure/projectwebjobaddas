//-----------------------------------------------------------
// <copyright file="UserDeviceDAL.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.ApplicationInsights;
using Microsoft.WindowsAzure.Storage.Table;
using adidas.clb.MobileApproval.Exceptions;
using adidas.clb.MobileApproval.Models;
using adidas.clb.MobileApproval.Utility;

namespace adidas.clb.MobileApproval.App_Code.DAL.Personalization
{
    /// <summary>
    /// The class which implements methods for data access layer of userdevice.
    /// </summary>
    public class UserDeviceDAL
    {
        //Application insights interface reference for logging the error details into Application Insight azure service.
        static IAppInsight InsightLogger { get { return AppInsightLogger.Instance; } }
        /// <summary>
        /// method to add user device
        /// </summary>
        /// <param name="deviceofuser">takes list of user devices to be associated to user</param>
        public void AddDevices(List<UserDeviceEntity> deviceofuser)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                //call dataprovider method to add entities to azure table
                DataProvider.AddEntities(CoreConstants.AzureTables.UserDeviceConfiguration, deviceofuser);
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                //LoggerHelper.WriteToLog(exception + " - Error while adding userdevices to  userdeviceconfig azure table in DAL : "
                //+ exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new DataAccessException();
            }
        }
        
        /// <summary>
        /// method to remove userdevices
        /// </summary>
        /// <param name="deviceofuser">takes list of user devices to be removed</param>
        public void RemoveDevices(List<UserDeviceEntity> deviceofuser)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                //call dataprovider method to delete entities from azure table
                DataProvider.RemoveEntities(CoreConstants.AzureTables.UserDeviceConfiguration, deviceofuser);
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                //LoggerHelper.WriteToLog(exception + " - Error while removing userdevices from  userdeviceconfig azure table in DAL : "
                //+ exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new DataAccessException();
            }
        }

        /// <summary>
        /// method to get userdevices
        /// </summary>
        /// <param name="UserID">takes userid as input</param>
        /// <returns>returns list of user devices associated to user</returns>        
        public List<UserDeviceEntity> GetUserAllDevices(string UserID)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                //generate query to gety all user associated devices
                TableQuery<UserDeviceEntity> query = new TableQuery<UserDeviceEntity>().Where(TableQuery.GenerateFilterCondition(CoreConstants.AzureTables.PartitionKey, QueryComparisons.Equal, string.Concat(CoreConstants.AzureTables.UserDevicePK, UserID)));
                //call dataprovider method to get entities from azure table
                List<UserDeviceEntity> alluserdevices = DataProvider.GetEntitiesList<UserDeviceEntity>(CoreConstants.AzureTables.UserDeviceConfiguration, query);
                return alluserdevices;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                //LoggerHelper.WriteToLog(exception + " - Error while retrieving all userdevices from  userdeviceconfig azure table in DAL : "
                //+ exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new DataAccessException();
            }
        }
        
        /// <summary>
        /// method to get single userdevice
        /// </summary>
        /// <param name="userID">takes userid as input</param>
        /// <param name="userDeviceID">takes userdevice id as input</param>
        /// <returns>returns user device entity</returns>
        public UserDeviceEntity GetUserDevice(string userID, string userDeviceID)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                //call dataprovider method to get entities from azure table
                UserDeviceEntity userdevice = DataProvider.Retrieveentity<UserDeviceEntity>(CoreConstants.AzureTables.UserDeviceConfiguration, string.Concat(CoreConstants.AzureTables.UserDevicePK, userID), userDeviceID);
                return userdevice;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                //LoggerHelper.WriteToLog(exception + " - Error while retrieving userdevice from  userdeviceconfig azure table in DAL : "
                //+ exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new DataAccessException();
            }
        }

        /// <summary>
        /// method to delete single userdevice
        /// </summary>
        /// <param name="userID">takes userid as input</param>
        /// <param name="userDeviceID">takes user device id as input</param>
        /// <returns>returns deleted user device entity</returns>
        public UserDeviceEntity DeleteUserDevice(string userID, string userDeviceID)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                //call dataprovider method to delete entities from azure table
                UserDeviceEntity deleteUserDeviceEntity = DataProvider.DeleteEntity<UserDeviceEntity>(CoreConstants.AzureTables.UserDeviceConfiguration, string.Concat(CoreConstants.AzureTables.UserDevicePK, userID), userDeviceID);
                return deleteUserDeviceEntity;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                //LoggerHelper.WriteToLog(exception + " - Error while deleting userdevice from  userdeviceconfig azure table in DAL : "
                //+ exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new DataAccessException();
            }
        }
    }
}