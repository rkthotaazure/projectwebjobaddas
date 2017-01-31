//-----------------------------------------------------------
// <copyright file="Personalization.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Configuration;
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
        /// <param name="UserID">takes userid as input</param>
        /// <returns>returns true if user exists else false</returns>        
        public Boolean CheckUser(string UserID)
        {
            try
            {
                PersonalizationDAL personalizationdal = new PersonalizationDAL();
                //calling data access layer method               
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
        /// <param name="user">takes user entity to create new user</param>               
        public void CreateUser(UserEntity user)
        {
            try
            {
                PersonalizationDAL personalizationdal = new PersonalizationDAL();
                //calling data access layer method                
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
        /// <param name="user">takes user entity to update existing user</param>        
        public void UpdateUserProp(UserEntity user)
        {
            try
            {
                PersonalizationDAL personalizationdal = new PersonalizationDAL();
                //calling data access layer method
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

        //// <summary>
        /// method to trigger user requsets update
        /// </summary>
        /// <param name="UserID">takes userid as input</param>
        /// <param name="userbackendslist">takes userbackends list as input</param>
        public void TriggerUserRequests(string userID, IEnumerable<UserBackendDTO> userbackendslist)
        {
            try
            {
                UpdateTriggeringMessage updateTriggerMessage = new UpdateTriggeringMessage();                
                UserUpdateMsg usermsg = new UserUpdateMsg();
                usermsg.UserID = userID;              
                List<UpdateTriggerBackend> updatetriggerbackendlist = new List<UpdateTriggerBackend>();
                //adding each user backend details to list for adding to message
                foreach (UserBackendDTO userbackend in userbackendslist)
                {
                    UpdateTriggerBackend triggerbackend = new UpdateTriggerBackend();
                    triggerbackend.BackendID = userbackend.backend.BackendID;
                    updatetriggerbackendlist.Add(triggerbackend);

                }
                usermsg.Backends = updatetriggerbackendlist;
                //creating list to add users                
                List<UserUpdateMsg> usermsglist = new List<UserUpdateMsg>();
                usermsglist.Add(usermsg);
                updateTriggerMessage.Users = usermsglist;
                updateTriggerMessage.GetPDFs = Convert.ToBoolean(ConfigurationManager.AppSettings[CoreConstants.Config.GetPDFs]);
                //calling data access layer method to add message to queue
                PersonalizationDAL personalizationdal = new PersonalizationDAL();
                personalizationdal.AddUpdateTriggerMessageToQueue(updateTriggerMessage);
            }
            catch (DataAccessException DALexception)
            {
                throw DALexception;
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error in BL while formatting updatetriggering message : "
                       + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new BusinessLogicException();
            }
        }

        /// <summary>        
        /// method to calculate sync waiting time      
        /// </summary>
        /// <param name="Backendtouser">takes backends associated to user</param>       
        /// <returns>returns synch waiting time</returns>
        public int CalcSynchTime(IEnumerable<UserBackendEntity> Backendtouser)
        {
            //calling rules to caliculate synch time 
            if (Backendtouser != null)
            {
                return Rules.SynchWaitingTime(Backendtouser); ;
            }
            else
            {
                return 0;
            }

        }

        /// <summary>
        /// method to get user details
        /// </summary>
        /// <param name="UserID">takes user id as input</param>
        /// <returns> returns user along with associated devices and backends </returns>        
        public UserDTO GetUser(string UserID)
        {
            try
            {
                PersonalizationDAL personalizationdal = new PersonalizationDAL();
                UserEntity user = personalizationdal.GetUser(UserID);
                if (user != null)
                {
                    //converting userentity to user data transfer object                
                    UserDTO userdto = DataProvider.ResponseObjectMapper<UserDTO, UserEntity>(user);
                    //getting devices and backends associated to that user
                    UserBackend userbackend = new UserBackend();
                    UserDevice userdevice = new UserDevice();
                    ///getting devices and backends associated to that user
                    IEnumerable<UserDeviceDTO> userdevices = userdevice.GetUserAllDevices(UserID);
                    IEnumerable<UserBackendDTO> userbackends = userbackend.GetUserAllBackends(UserID);
                    userdto.userbackends = userbackends;
                    userdto.userdevices = userdevices;
                    return userdto;
                }
                else
                {
                    return null;
                }
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
        /// <param name="UserID">takes userid as input</param>
        /// <returns>returns deleted user entity</returns>        
        public UserEntity DeleteUser(string UserID)
        {
            try
            {
                PersonalizationDAL personalizationdal = new PersonalizationDAL();
                //calling data access layer method
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

        /// <summary>
        ///to create user entity from user data transfer object
        /// </summary>
        /// <param name="userdto">takes user dto as input</param>
        /// <returns>returns user entity</returns>
        public UserEntity UserEntityGenerator(UserDTO userdto)
        {
            //calling mapper to map propertiews from dto to entity
            UserEntity userentity = DataProvider.ResponseObjectMapper<UserEntity, UserDTO>(userdto);
            userentity.PartitionKey = CoreConstants.AzureTables.User;
            userentity.RowKey = userdto.UserID;
            return userentity;
        }
    }

}
