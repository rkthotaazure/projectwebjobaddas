//-----------------------------------------------------------
// <copyright file="UserDeviceDAL.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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
        /// <summary>
        /// method to add user device
        /// </summary>
        /// <param name="deviceofuser"></param>
        public void AddDevices(List<UserDeviceEntity> deviceofuser)
        {
            try
            {
                CloudTable UserDeviceConfigurationTable = DataProvider.GetAzureTableInstance(CoreConstants.AzureTables.UserDeviceConfiguration);
                TableBatchOperation batchOperation = new TableBatchOperation();
                foreach (UserDeviceEntity usrdeviceentity in deviceofuser)
                {
                    batchOperation.Insert(usrdeviceentity);
                }
                UserDeviceConfigurationTable.ExecuteBatch(batchOperation);
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error while adding userdevices to  userdeviceconfig azure table in DAL : "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new DataAccessException();
            }
        }

        /// <summary>
        /// method to update userdevice
        /// </summary>
        /// <param name="deviceofuser"></param>
        public void UpdateDevices(List<UserDeviceEntity> deviceofuser)
        {
            try
            {
                CloudTable UserDeviceConfigurationTable = DataProvider.GetAzureTableInstance(CoreConstants.AzureTables.UserDeviceConfiguration);
                TableBatchOperation batchOperation = new TableBatchOperation();
                foreach (UserDeviceEntity usrdeviceentity in deviceofuser)
                {
                    batchOperation.Add(TableOperation.Replace(usrdeviceentity));
                }
                UserDeviceConfigurationTable.ExecuteBatch(batchOperation);
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error while updaing userdevices to  userdeviceconfig azure table in DAL : "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new DataAccessException();
            }
        }

        /// <summary>
        /// method to remove userdevices
        /// </summary>
        /// <param name="deviceofuser"></param>
        public void RemoveDevices(List<UserDeviceEntity> deviceofuser)
        {
            try
            {
                CloudTable UserDeviceConfigurationTable = DataProvider.GetAzureTableInstance(CoreConstants.AzureTables.UserDeviceConfiguration);
                TableBatchOperation batchOperation = new TableBatchOperation();
                foreach (UserDeviceEntity usrdeviceentity in deviceofuser)
                {
                    batchOperation.Add(TableOperation.Delete(usrdeviceentity));
                }
                UserDeviceConfigurationTable.ExecuteBatch(batchOperation);
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error while removing userdevices from  userdeviceconfig azure table in DAL : "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new DataAccessException();
            }
        }

        /// <summary>
        /// method to get userdevices
        /// </summary>
        /// <param name="userprops"></param>
        /// <returns></returns>        
        public List<UserDeviceEntity> GetUserAllDevices(String UserID)
        {
            try
            {
                CloudTable UserDeviceConfigurationTable = DataProvider.GetAzureTableInstance(CoreConstants.AzureTables.UserDeviceConfiguration);
                TableQuery<UserDeviceEntity> query = new TableQuery<UserDeviceEntity>().Where(TableQuery.GenerateFilterCondition(CoreConstants.AzureTables.PartitionKey, QueryComparisons.Equal, string.Concat(CoreConstants.AzureTables.UserDevicePK, UserID)));
                List<UserDeviceEntity> alluserdevices = UserDeviceConfigurationTable.ExecuteQuery(query).ToList();
                return alluserdevices;
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error while retrieving all userdevices from  userdeviceconfig azure table in DAL : "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new DataAccessException();
            }
        }

        /// <summary>
        /// method to insert single userdevice
        /// </summary>
        /// <param name="userdeviceentity"></param>
        public void PostDevices(UserDeviceEntity userdeviceentity)
        {
            try
            {
                CloudTable UserDeviceConfigurationTable = DataProvider.GetAzureTableInstance(CoreConstants.AzureTables.UserDeviceConfiguration);
                TableOperation insertOperation = TableOperation.Insert(userdeviceentity);
                UserDeviceConfigurationTable.Execute(insertOperation);
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error while inserting userdevice to  userdeviceconfig azure table in DAL : "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new DataAccessException();
            }
        }

        /// <summary>
        /// method to get single userdevice
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="userDeviceID"></param>
        /// <returns></returns>
        public UserDeviceEntity GetUserDevice(String userID, String userDeviceID)
        {
            try
            {

                CloudTable UserDeviceConfigurationTable = DataProvider.GetAzureTableInstance(CoreConstants.AzureTables.UserDeviceConfiguration);
                TableOperation retrieveUser = TableOperation.Retrieve<UserDeviceEntity>(string.Concat(CoreConstants.AzureTables.UserDevicePK, userID), userDeviceID);
                TableResult retrievedResult = UserDeviceConfigurationTable.Execute(retrieveUser);
                return (UserDeviceEntity)retrievedResult.Result;
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error while retrieving userdevice from  userdeviceconfig azure table in DAL : "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new DataAccessException();
            }
        }

        /// <summary>
        /// method to delete single userdevice
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="userDeviceID"></param>
        /// <returns></returns>
        public UserDeviceEntity DeleteUserDevice(String userID, String userDeviceID)
        {
            try
            {
                CloudTable UserDeviceConfigurationTable = DataProvider.GetAzureTableInstance(CoreConstants.AzureTables.UserDeviceConfiguration);
                TableOperation retrieveUserDevice = TableOperation.Retrieve<UserDeviceEntity>(string.Concat(CoreConstants.AzureTables.UserDevicePK, userID), userDeviceID);
                TableResult retrievedUser = UserDeviceConfigurationTable.Execute(retrieveUserDevice);
                UserDeviceEntity deleteUserDeviceEntity = (UserDeviceEntity)retrievedUser.Result;
                if (deleteUserDeviceEntity != null)
                {
                    TableOperation deleteOperation = TableOperation.Delete(deleteUserDeviceEntity);
                    UserDeviceConfigurationTable.Execute(deleteOperation);
                }
                return deleteUserDeviceEntity;
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error while deleting userdevice from  userdeviceconfig azure table in DAL : "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new DataAccessException();
            }
        }
    }
}