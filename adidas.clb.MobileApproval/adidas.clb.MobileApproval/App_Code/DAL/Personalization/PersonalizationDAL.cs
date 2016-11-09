//-----------------------------------------------------------
// <copyright file="PersonalizationDAL.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Web;
using Microsoft.WindowsAzure.Storage.Table;
using adidas.clb.MobileApproval.Models;
using adidas.clb.MobileApproval.Utility;
using adidas.clb.MobileApproval.Exceptions;

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
        public UserEntity GetUser(String UserID)
        {
            try
            {
                //get's azure table instance
                CloudTable ReferenceDataTable = DataProvider.GetAzureTableInstance(CoreConstants.AzureTables.ReferenceData);
                TableOperation RetrieveUser = TableOperation.Retrieve<UserEntity>(CoreConstants.AzureTables.User, UserID);
                TableResult RetrievedResultUser = ReferenceDataTable.Execute(RetrieveUser);
                return (UserEntity)RetrievedResultUser.Result;
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
                //get's azure table instance
                CloudTable ReferenceDataTable = DataProvider.GetAzureTableInstance(CoreConstants.AzureTables.ReferenceData);
                TableOperation insertOperation = TableOperation.Insert(user);
                ReferenceDataTable.Execute(insertOperation);
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
                //get's azure table instance
                CloudTable ReferenceDataTable = DataProvider.GetAzureTableInstance(CoreConstants.AzureTables.ReferenceData);
                //adding ETag to user for replace operattion
                user.ETag = "*";
                TableOperation updateOperation = TableOperation.Replace(user);
                ReferenceDataTable.Execute(updateOperation);
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
        public UserEntity DeleteUser(String UserID)
        {
            try
            {
                //get's azure table instance
                CloudTable ReferenceDataTable = DataProvider.GetAzureTableInstance(CoreConstants.AzureTables.ReferenceData);
                TableOperation RetrieveUser = TableOperation.Retrieve<UserEntity>(CoreConstants.AzureTables.User, UserID);
                //get user entity with UserID
                TableResult RetrievedResultUser = ReferenceDataTable.Execute(RetrieveUser);                
                UserEntity deleteUserEntity = (UserEntity)RetrievedResultUser.Result;
                //delete retrieved user entity
                if (deleteUserEntity != null)
                {
                    TableOperation deleteOperation = TableOperation.Delete(deleteUserEntity);
                    ReferenceDataTable.Execute(deleteOperation);
                }
                return deleteUserEntity;
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error while deleting user from ReferenceData azure table in DAL : "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new DataAccessException();
            }
        }
    }
}