//-----------------------------------------------------------
// <copyright file="PersonalizationDAL.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Web;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Queue;
using adidas.clb.MobileApproval.Models;
using adidas.clb.MobileApproval.Utility;
using adidas.clb.MobileApproval.Exceptions;
using Newtonsoft.Json;

namespace adidas.clb.MobileApproval.App_Code.DAL.Personalization
{
    /// <summary>
    /// The class which implements methods for data access layer of personalization.
    /// </summary>    
    public class PersonalizationDAL
    {
        /// <summary>
        /// method to get user entity with UserID
        /// </summary>
        /// <param name="UserID">takes userid as input</param>
        /// <returns>returns user entity</returns>                     
        public UserEntity GetUser(string UserID)
        {
            try
            {
                //call dataprovider method to retrieve entity from azure table
                UserEntity user=DataProvider.Retrieveentity<UserEntity>(CoreConstants.AzureTables.ReferenceData,CoreConstants.AzureTables.User, UserID);                
                return user;
            }
            catch (Exception exception)
            {                
                LoggerHelper.WriteToLog(exception + " - Error while retrieving user from ReferenceData azure table in DAL : "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new DataAccessException();
            }
        }

        /// <summary>
        /// method to create newuser
        /// </summary>
        /// <param name="user">takes user entity as input</param>                
        public void CreateUser(UserEntity user)
        {
            try
            {
                //call dataprovider method to insert entity into azure table
                DataProvider.InsertEntity<UserEntity>(CoreConstants.AzureTables.ReferenceData, user);
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error while creating user into ReferenceData azure table in DAL : "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new DataAccessException();
            }
        }

        /// <summary>
        /// method to update existing user props
        /// </summary>
        /// <param name="user">takes user entity as input</param>        
        public void UpdateUserProp(UserEntity user)
        {
            try
            {
                //call dataprovider method to update entity to azure table
                DataProvider.UpdateEntity<UserEntity>(CoreConstants.AzureTables.ReferenceData, user);
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error while updating user props into ReferenceData azure table in DAL : "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new DataAccessException();
            }
        }

        /// <summary>
        /// method to delete user entity
        /// </summary>
        /// <param name="user">takes userid as input</param>
        /// <returns>returns deleted user entity</returns>        
        public UserEntity DeleteUser(string UserID)
        {
            try
            {
                //call dataprovider method to delete entity from azure table            
                UserEntity deleteUserEntity = DataProvider.DeleteEntity<UserEntity>(CoreConstants.AzureTables.ReferenceData, CoreConstants.AzureTables.User, UserID);                
                return deleteUserEntity;
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error while deleting user from ReferenceData azure table in DAL : "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new DataAccessException();
            }
        }

        /// <summary>
        /// method to add user to be updated into updatetrigger queue
        /// </summary>
        /// <param name="updateTriggeringMessage">takes message object as input</param>
        public void AddUpdateTriggerMessageToQueue(UpdateTriggeringMessage updateTriggeringMessage)
        {
            try
            {                
                string message = JsonConvert.SerializeObject(updateTriggeringMessage);
                //call dataprovider method to add message to azure queue
                DataProvider.AddMessagetoQueue(CoreConstants.AzureQueues.UpdateTriggerQueueName, message);
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error while adding message to updatetriggering queue in DAL : "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new DataAccessException();
            }
        }
    }
}