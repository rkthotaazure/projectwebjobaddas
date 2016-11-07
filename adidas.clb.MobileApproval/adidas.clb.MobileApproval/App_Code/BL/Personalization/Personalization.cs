//-----------------------------------------------------------
// <copyright file="Personalization.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Web;
using adidas.clb.MobileApproval.Exceptions;
using adidas.clb.MobileApproval.Models;
using adidas.clb.MobileApproval.Utility;
using adidas.clb.MobileApproval.App_Code.DAL.Personalization;

namespace adidas.clb.MobileApproval.App_Code.BL.Personalization
{
    /// <summary>
    /// The class which implements methods for business logic layer of personalization.
    /// </summary>
    public class Personalization
    {
        /// <summary>
        /// method to check user availability
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>        
        public Boolean CheckUser(String UserID)
        {
            try
            {
                PersonalizationDAL personalizationdal = new PersonalizationDAL();
                UserEntity user = personalizationdal.GetUser(UserID);
                //if user exists return true
                if (user != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error in BL while checking user existance : "
                       + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new BusinessLogicException();
            }
        }

        /// <summary>
        /// method to create newuser
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>        
        public void CreateUser(UserEntity user)
        {
            try
            {
                PersonalizationDAL personalizationdal = new PersonalizationDAL();
                personalizationdal.CreateUser(user);
            }
            catch (DataAccessException DALexception)
            {
                throw DALexception;
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error in BL while creating user : "
                       + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new BusinessLogicException();
            }
        }

        /// <summary>
        /// method to update existing userprops
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public void UpdateUserProp(UserEntity user)
        {
            try
            {
                PersonalizationDAL personalizationdal = new PersonalizationDAL();
                personalizationdal.UpdateUserProp(user);
            }
            catch (DataAccessException DALexception)
            {
                throw DALexception;
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error in BL while updating user props: "
                       + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new BusinessLogicException();
            }
        }

        /// <summary>
        /// method to trigger user requsets update
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>        
        public void TriggerUserRequests(String UserID)
        {
            //code here..
        }

        /// <summary>        
        /// method to calculate sync waiting time      
        /// </summary>
        /// <param name="Backendtouser"></param>
        /// <param name="MaxSyncReplySize"></param>
        /// <returns></returns>
        public int CalcSynchTime(IEnumerable<UserBackendEntity> Backendtouser)
        {            
            return Rules.Rule1(Backendtouser); ;
        }

        /// <summary>
        /// method to get user details
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>        
        public UserDTO GetUser(String UserID)
        {
            try
            {
                PersonalizationDAL personalizationdal = new PersonalizationDAL();
                UserEntity user = personalizationdal.GetUser(UserID);
                //converting userentity to user data transfer object                
                UserDTO userdto= DataProvider.ResponseObjectMapper<UserDTO, UserEntity>(user);
                //getting devices and backends associated to that user
                UserBackend userbackend = new UserBackend();
                UserDevice userdevice = new UserDevice();
                IEnumerable<UserDeviceDTO> userdevices = userdevice.GetUserAllDevices(UserID);
                IEnumerable<UserBackendDTO> userbackends = userbackend.GetUserAllBackends(UserID);
                userdto.userbackends = userbackends;
                userdto.userdevices = userdevices;
                return userdto;
            }
            catch (DataAccessException DALexception)
            {
                throw DALexception;
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error in BL while retrieving user : "
                       + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new BusinessLogicException();
            }
        }

        /// <summary>
        /// method to delete user details
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>        
        public UserEntity DeleteUser(String UserID)
        {
            try
            {
                PersonalizationDAL personalizationdal = new PersonalizationDAL();
                UserEntity deleteuser = personalizationdal.DeleteUser(UserID);
                return deleteuser;
            }
            catch (DataAccessException DALexception)
            {
                throw DALexception;
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error in BL while deleting user : "
                       + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new BusinessLogicException();
            }
        }

        public UserEntity UserEntityGenerator(UserDTO userdto)
        {
            UserEntity userentity = DataProvider.ResponseObjectMapper<UserEntity, UserDTO>(userdto);
            userentity.PartitionKey = CoreConstants.AzureTables.User;
            userentity.RowKey = userdto.UserID;
            return userentity;
        }
    }

}
