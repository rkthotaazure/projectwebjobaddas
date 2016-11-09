//-----------------------------------------------------------
// <copyright file="UserBackendDAL.cs" company="adidas AG">
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
    /// The class which implements methods for data access layer of userbackend.
    /// </summary>
    public class UserBackendDAL
    {
        /// <summary>
        /// method to add userbackends
        /// </summary>
        /// <param name="backendofuser">takes list of user backend entities</param>
        public void AddBackends(List<UserBackendEntity> backendofuser)
        {
            try
            {
                //get's azure table instance
                CloudTable UserBackendConfigurationTable = DataProvider.GetAzureTableInstance(CoreConstants.AzureTables.UserDeviceConfiguration);
                TableBatchOperation batchOperation = new TableBatchOperation();
                //insert list of entities into batch operation
                foreach (UserBackendEntity usrbackendentity in backendofuser)
                {
                    batchOperation.Insert(usrbackendentity);
                }
                UserBackendConfigurationTable.ExecuteBatch(batchOperation);
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error while adding userbackends to  userdeviceconfig azure table in DAL : "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new DataAccessException();
            }
        }
       
        /// <summary>
        /// method to remove user backends
        /// </summary>
        /// <param name="backendofuser">takes list of user backend entities to be removed</param>
        public void RemoveBackends(List<UserBackendEntity> backendofuser)
        {
            try
            {
                //get's azure table instance
                CloudTable UserBackendConfigurationTable = DataProvider.GetAzureTableInstance(CoreConstants.AzureTables.UserDeviceConfiguration);
                TableBatchOperation batchOperation = new TableBatchOperation();
                //insert list of entities into batch operation
                foreach (UserBackendEntity usrbackendentity in backendofuser)
                {
                    batchOperation.Add(TableOperation.Delete(usrbackendentity));
                }
                UserBackendConfigurationTable.ExecuteBatch(batchOperation);
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error while removing userbackends from  userdeviceconfig azure table in DAL : "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new DataAccessException();
            }
        }

        /// <summary>
        ///method to get list of all backends
        /// </summary>        
        /// <returns>returns list of user backend entities</returns>        
        public List<BackendEntity> GetBackends()
        {
            try
            {
                //get's azure table instance
                CloudTable UserDeviceConfigurationTable = DataProvider.GetAzureTableInstance(CoreConstants.AzureTables.ReferenceData);
                TableQuery<BackendEntity> query = new TableQuery<BackendEntity>().Where(TableQuery.GenerateFilterCondition(CoreConstants.AzureTables.PartitionKey, QueryComparisons.Equal, CoreConstants.AzureTables.Backend));
                List<BackendEntity> allBackends = UserDeviceConfigurationTable.ExecuteQuery(query).ToList();
                return allBackends;
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error while retrieving all backends from referencedata azure table in DAL : "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new DataAccessException();
            }
        }

        /// <summary>
        /// method to get userbackends
        /// </summary>
        /// <param name="UserID">takes userid as input</param>
        /// <returns> returns list of user backends associated to user</returns>
        public List<UserBackendEntity> GetUserAllBackends(String UserID)
        {
            try
            {
                //get's azure table instance
                CloudTable UserDeviceConfigurationTable = DataProvider.GetAzureTableInstance(CoreConstants.AzureTables.UserDeviceConfiguration);
                TableQuery<UserBackendEntity> query = new TableQuery<UserBackendEntity>().Where(TableQuery.GenerateFilterCondition(CoreConstants.AzureTables.PartitionKey, QueryComparisons.Equal, string.Concat(CoreConstants.AzureTables.UserBackendPK, UserID)));
                List<UserBackendEntity> alluserbackends = UserDeviceConfigurationTable.ExecuteQuery(query).ToList();
                return alluserbackends;
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error while retrieving userbackends from userdeviceconfig azure table in DAL : "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new DataAccessException();
            }
        }

        /// <summary>
        /// method to insert single userbackend
        /// </summary>
        /// <param name="userbackendentity">takes list of user backend entities</param>
        public void PostBackends(UserBackendEntity userbackendentity)
        {
            try
            {
                //get's azure table instance
                CloudTable UserDeviceConfigurationTable = DataProvider.GetAzureTableInstance(CoreConstants.AzureTables.UserDeviceConfiguration);
                TableOperation insertOperation = TableOperation.Insert(userbackendentity);
                UserDeviceConfigurationTable.Execute(insertOperation);
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error while inserting userbackend to userdeviceconfig azure table in DAL : "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new DataAccessException();
            }
        }

        /// <summary>
        /// method to get single userbackend
        /// </summary>
        /// <param name="userID"> takes user id as input</param>
        /// <param name="userBackendID">takes user backend id as input</param>
        /// <returns>returns user backend entity</returns>
        public UserBackendEntity GetUserBackend(String userID, String userBackendID)
        {
            try
            {
                //get's azure table instance
                CloudTable UserDeviceConfigurationTable = DataProvider.GetAzureTableInstance(CoreConstants.AzureTables.UserDeviceConfiguration);
                TableOperation retrieveUser = TableOperation.Retrieve<UserBackendEntity>(string.Concat(CoreConstants.AzureTables.UserBackendPK, userID), userBackendID);                
                TableResult retrievedResult = UserDeviceConfigurationTable.Execute(retrieveUser);               
                return (UserBackendEntity)retrievedResult.Result;
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error while retrieving userbackend from userdeviceconfig azure table in DAL : "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new DataAccessException();
            }
        }

        /// <summary>
        /// method to delete user backend
        /// </summary>
        /// <param name="userID"> takes userid as input</param>
        /// <param name="userBackendID">takes user backend id as input</param>
        /// <returns>returns deleted user backend entity</returns>
        public UserBackendEntity DeleteUserBackend(String userID, String userBackendID)
        {
            try
            {
                //get's azure table instance
                CloudTable UserDeviceConfigurationTable = DataProvider.GetAzureTableInstance(CoreConstants.AzureTables.UserDeviceConfiguration);
                TableOperation retrieveUserBackend = TableOperation.Retrieve<UserBackendEntity>(string.Concat(CoreConstants.AzureTables.UserBackendPK, userID), userBackendID);
                TableResult retrievedUser = UserDeviceConfigurationTable.Execute(retrieveUserBackend);
                UserBackendEntity deleteUserBackendEntity = (UserBackendEntity)retrievedUser.Result;
                //if user backend exists delete it
                if (deleteUserBackendEntity != null)
                {
                    TableOperation deleteOperation = TableOperation.Delete(deleteUserBackendEntity);
                    UserDeviceConfigurationTable.Execute(deleteOperation);
                }

                return deleteUserBackendEntity;
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error while deleting userbackend from userdeviceconfig azure table in DAL : "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new DataAccessException();
            }
        }

        /// <summary>
        /// method to get all userbackends synch
        /// </summary>
        /// <param name="UserID">takes user id as input</param>
        /// <returns>returns list of backends synch entity for user</returns>
        public List<SynchEntity> GetAllUserBackendsSynch(String UserID)
        {
            try
            {
                //get's azure table instance
                CloudTable UserDeviceConfigurationTable = DataProvider.GetAzureTableInstance(CoreConstants.AzureTables.UserDeviceConfiguration);
                TableQuery<SynchEntity> query = new TableQuery<SynchEntity>().Where(TableQuery.GenerateFilterCondition(CoreConstants.AzureTables.PartitionKey, QueryComparisons.Equal, string.Concat(CoreConstants.AzureTables.BackendSynchPK, UserID)));
                List<SynchEntity> alluserbackendsynch = UserDeviceConfigurationTable.ExecuteQuery(query).ToList();
                return alluserbackendsynch;
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error while retrieving all userbackendssynch from userdeviceconfig azure table in DAL : "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new DataAccessException();
            }
        }

        /// <summary>
        /// method to get user backend synch
        /// </summary>
        /// <param name="UserID">takes user id as input</param>
        /// /// <param name="UserBackendID">takes user backend id as input</param>
        /// <returns>returns backend synch entity for user</returns>
        public SynchEntity GetBackendSynch(String UserID, string UserBackendID)
        {
            try
            {
                //get's azure table instance
                CloudTable UserDeviceConfigurationTable = DataProvider.GetAzureTableInstance(CoreConstants.AzureTables.UserDeviceConfiguration);
                TableOperation retrieveUserBackendSynch = TableOperation.Retrieve<UserBackendEntity>(string.Concat(CoreConstants.AzureTables.BackendSynchPK, UserID), UserBackendID);
                TableResult retrievedResult = UserDeviceConfigurationTable.Execute(retrieveUserBackendSynch);
                return (SynchEntity)retrievedResult.Result;
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error while retrieving userbackendsynch from userdeviceconfig azure table in DAL : "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new DataAccessException();
            }
        }
    }
}