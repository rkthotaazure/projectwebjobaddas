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
                //call dataprovider method to add entities to azure table
                DataProvider.AddEntities(CoreConstants.AzureTables.UserDeviceConfiguration, backendofuser);
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
                //call dataprovider method to delete entities from azure table
                DataProvider.RemoveEntities(CoreConstants.AzureTables.UserDeviceConfiguration, backendofuser);
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
                //generate query to retrive backends 
                TableQuery<BackendEntity> query = new TableQuery<BackendEntity>().Where(TableQuery.GenerateFilterCondition(CoreConstants.AzureTables.PartitionKey, QueryComparisons.Equal, CoreConstants.AzureTables.Backend));
                //call dataprovider method to get entities from azure table
                List<BackendEntity> allBackends = DataProvider.GetEntitiesList<BackendEntity>(CoreConstants.AzureTables.ReferenceData, query);
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
        public List<UserBackendEntity> GetUserAllBackends(string UserID)
        {
            try
            {
                //generate query to get all user associated backends
                TableQuery<UserBackendEntity> query = new TableQuery<UserBackendEntity>().Where(TableQuery.GenerateFilterCondition(CoreConstants.AzureTables.PartitionKey, QueryComparisons.Equal, string.Concat(CoreConstants.AzureTables.UserBackendPK, UserID)));
                //call dataprovider method to get entities from azure table
                List<UserBackendEntity> alluserbackends = DataProvider.GetEntitiesList<UserBackendEntity>(CoreConstants.AzureTables.UserDeviceConfiguration, query);
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
        /// method to get single userbackend
        /// </summary>
        /// <param name="userID"> takes user id as input</param>
        /// <param name="userBackendID">takes user backend id as input</param>
        /// <returns>returns user backend entity</returns>
        public UserBackendEntity GetUserBackend(string userID, string userBackendID)
        {
            try
            {
                //call dataprovider method to get entities from azure table
                UserBackendEntity userbackend = DataProvider.Retrieveentity<UserBackendEntity>(CoreConstants.AzureTables.UserDeviceConfiguration, string.Concat(CoreConstants.AzureTables.UserBackendPK, userID), userBackendID);
                return userbackend;
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
        public UserBackendEntity DeleteUserBackend(string userID, string userBackendID)
        {
            try
            {
                //call dataprovider method to delete entities from azure table
                UserBackendEntity deleteUserBackendEntity = DataProvider.DeleteEntity<UserBackendEntity>(CoreConstants.AzureTables.UserDeviceConfiguration, string.Concat(CoreConstants.AzureTables.UserBackendPK, userID), userBackendID);
                
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
        public List<SynchEntity> GetAllUserBackendsSynch(string UserID)
        {
            try
            {
                //generate query to get user backend synch from azure table
                TableQuery<SynchEntity> query = new TableQuery<SynchEntity>().Where(TableQuery.GenerateFilterCondition(CoreConstants.AzureTables.PartitionKey, QueryComparisons.Equal, string.Concat(CoreConstants.AzureTables.BackendSynchPK, UserID)));
                //call dataprovider method to get entities from azure table
                List<SynchEntity> alluserbackendsynch = DataProvider.GetEntitiesList<SynchEntity>(CoreConstants.AzureTables.UserDeviceConfiguration, query);
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
        public SynchEntity GetBackendSynch(string UserID, string UserBackendID)
        {
            try
            {
                //call dataprovider method to get entities from azure table
                SynchEntity synchentity = DataProvider.Retrieveentity<SynchEntity>(CoreConstants.AzureTables.UserDeviceConfiguration, (string.Concat(CoreConstants.AzureTables.BackendSynchPK, UserID)), UserBackendID);
                return synchentity;
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