//-----------------------------------------------------------
// <copyright file="UserBackend.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using adidas.clb.MobileApproval.Exceptions;
using adidas.clb.MobileApproval.Models;
using adidas.clb.MobileApproval.Utility;
using adidas.clb.MobileApproval.App_Code.DAL.Personalization;

namespace adidas.clb.MobileApproval.App_Code.BL.Personalization
{
    /// <summary>
    /// The class which implements methods for business logic layer of userbackend
    /// </summary>
    public class UserBackend
    {
        /// <summary>
        ///method to add userbackends
        /// </summary>
        /// <param name="Backendtouser">takes list of user backends as input</param>              
        public void AddBackends(List<UserBackendEntity> Backendtouser)
        {
            try
            {
                UserBackendDAL userbackenddal = new UserBackendDAL();
                //calling data access layer method
                userbackenddal.AddBackends(Backendtouser);
            }
            catch (DataAccessException DALexception)
            {
                throw DALexception;
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error in BL while adding userbackends : "
                       + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new BusinessLogicException();
            }
        }
        
        /// <summary>
        ///method to remove userbackends
        /// </summary>
        /// <param name="UserID">takes userid as input</param>        
        public void RemoveBackends(String UserID)
        {
            try
            {
                UserBackendDAL userbackenddal = new UserBackendDAL();
                //to get all backends associated to user
                List<UserBackendEntity> alluserbackends = userbackenddal.GetUserAllBackends(UserID);
                //calling data access layer method to remove user backends
                userbackenddal.RemoveBackends(alluserbackends);
            }
            catch (DataAccessException DALexception)
            {
                throw DALexception;
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error in BL while removing userbackends : "
                       + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new BusinessLogicException();
            }
        }

        /// <summary>
        /// method to get all backends
        /// </summary>
        /// <returns>returns list of backends as personalization response form</returns>
        public PersonalizationResponseListDTO<BackendDTO> GetBackends()
        {
            try
            {
                UserBackendDAL userbackenddal = new UserBackendDAL();
                //calling data access layer method
                List<BackendEntity> backends = userbackenddal.GetBackends();
                List<BackendDTO> backendslistdto = new List<BackendDTO>();
                //backends entity converting to data transfer object
                foreach (BackendEntity backend in backends)
                {
                   
                    BackendDTO backenddto = DataProvider.ResponseObjectMapper<BackendDTO, BackendEntity>(backend);
                    backendslistdto.Add(backenddto);
                }
                var ResponseBackends = new PersonalizationResponseListDTO<BackendDTO>();
                ResponseBackends.result = backendslistdto;
                return ResponseBackends;
            }
            catch (DataAccessException DALexception)
            {
                throw  DALexception;
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error in BL while retreiving backends : "
                       + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new BusinessLogicException();
            }
        }

        /// <summary>
        /// method to get all userbackends
        /// </summary>
        /// <param name="UserID">takes userid as input</param>
        /// <returns>returns list of user backend</returns>
        public IEnumerable<UserBackendDTO> GetUserAllBackends(String UserID)
        {
            try
            {
                UserBackendDAL userbackenddal = new UserBackendDAL();
                //calling data access layer method to get user backends
                List<UserBackendEntity> alluserbackends = userbackenddal.GetUserAllBackends(UserID);
                //calling data access layer method to get user backend synch
                List<SynchEntity> allbackendsynch = userbackenddal.GetAllUserBackendsSynch(UserID);                
                List<UserBackendDTO> userbackendsdtolist = new List<UserBackendDTO>();
                //converting userdevice entity to userdevice data transfer object
                foreach (UserBackendEntity userbackendentity in alluserbackends)
                {
                    UserBackendDTO userbackenddto = UserBackendEntityDTOMapper(userbackendentity);
                    SynchEntity synchentity=allbackendsynch.Where(m => m.RowKey.Equals(userbackendentity.BackendID)).FirstOrDefault();
                    //if user backend synch available then convert to dto
                    if(synchentity!=null)
                    {
                        SynchDTO synchdto = DataProvider.ResponseObjectMapper<SynchDTO, SynchEntity>(synchentity);
                        userbackenddto.synch = synchdto;
                    }
                      
                    userbackendsdtolist.Add(userbackenddto);
                }

                return (IEnumerable<UserBackendDTO>)userbackendsdtolist;
            }
            catch (DataAccessException DALexception)
            {
                throw DALexception;
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error in BL while retreiving userbackends : "
                       + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new BusinessLogicException();
            }
        }

        /// <summary>
        /// method to insert single userbackend
        /// </summary>
        /// <param name="userbackendentity">takes personalization requset with userbackend</param>
        public void PostBackends(PersonalizationRequsetDTO personalizationrequset)
        {
            try
            {
                UserBackendDAL userbackenddal = new UserBackendDAL();
                //get userbackend from array of ienumearble items
                UserBackendDTO userbackendto = personalizationrequset.userbackends.FirstOrDefault();
                //if user backend available then post it by calling dal method
                if (userbackendto != null)
                {
                    userbackenddal.PostBackends(UserBackendDTOEntityMapper(userbackendto));
                }

            }
            catch (DataAccessException DALexception)
            {
                throw DALexception;
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error in BL while inserting single userbackend : "
                       + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new BusinessLogicException();
            }
        }

        /// <summary>
        /// method to get single userbackend
        /// </summary>
        /// <param name="userID">takes userid as input</param>
        /// <param name="userBackendID">takes user backend id as input</param>
        /// <returns>returns user backend with id associated to user in the form of personalization response</returns>
        public PersonalizationResponseDTO<UserBackendDTO> GetUserBackend(String userID, String userBackendID)
        {
            try
            {
                UserBackendDAL userbackenddal = new UserBackendDAL();
                UserBackendEntity userbackendentity = userbackenddal.GetUserBackend(userID, userBackendID);
                SynchEntity synch = userbackenddal.GetBackendSynch(userID, userBackendID);
                //converting userbackend entity to Response data transfer object
                var ResponseUserBackend = new PersonalizationResponseDTO<UserBackendDTO>();
                UserBackendDTO userbackenddto = UserBackendEntityDTOMapper(userbackendentity);
                //adding synch to user backend
                if (synch != null)
                {
                    SynchDTO synchdto = DataProvider.ResponseObjectMapper<SynchDTO, SynchEntity>(synch);
                    userbackenddto.synch = synchdto;
                }              
                ResponseUserBackend.result = userbackenddto;
                return ResponseUserBackend;
            }
            catch (DataAccessException DALexception)
            {
                throw DALexception;
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error in BL while retreiving single userbackend : "
                       + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new BusinessLogicException();
            }
        }

        /// <summary>
        /// method to delete single userbackend
        /// </summary>
        /// <param name="userID">takes userid as input</param>
        /// <param name="userBackendID">takes user backend id as input</param>
        /// <returns>returns deleted user backend entity</returns>
        public UserBackendEntity DeleteUserBackend(String userID, String userBackendID)
        {
            try
            {
                UserBackendDAL userbackenddal = new UserBackendDAL();
                //calling data access layer method 
                UserBackendEntity userbackendentity = userbackenddal.DeleteUserBackend(userID, userBackendID);
                return userbackendentity;
            }
            catch (DataAccessException DALexception)
            {
                throw DALexception;
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error in BL while deleting single userbackend : "
                       + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new BusinessLogicException();
            }
        }

        /// <summary>
        /// Converting userbackend input DTO to userbackendentity with partitionkey, Rowkey
        /// </summary>
        /// <param name="userbackendsdto">takes user backend dto as input</param>
        /// <returns>returns user backend entity</returns>        
        public IEnumerable<UserBackendEntity> UserBackendEntityGenerator(IEnumerable<UserBackendDTO> userbackendsdtolist)
        {
            List<UserBackendEntity> userbackendentitylist = new List<UserBackendEntity>();
            //mapper to convert entity to data transfer object
            foreach (UserBackendDTO userbackenddto in userbackendsdtolist)
            {
                userbackendentitylist.Add(UserBackendDTOEntityMapper(userbackenddto));
            }
            return (IEnumerable<UserBackendEntity>)userbackendentitylist;
        }

        /// <summary>
        /// method which converts userbackend dto to entity by using mapper, first maps backenddto with userbackendentity
        /// </summary>
        /// <param name="userbackenddto">takes user backend dto as input</param>
        /// <returns>returns user backend entity</returns>
        public UserBackendEntity UserBackendDTOEntityMapper(UserBackendDTO userbackenddto)
        {
            //converting input backend data transfer object(dto) list to userbackend entity list by using property mapper
            BackendDTO backenddto = userbackenddto.backend;
            UserBackendEntity userbackendentity = DataProvider.ResponseObjectMapper<UserBackendEntity, BackendDTO>(backenddto);
            //adding missing properties like userID, partitionkey and Rowkey to entity
            userbackendentity.UserID = userbackenddto.UserID;
            userbackendentity.PartitionKey = string.Concat(CoreConstants.AzureTables.UserBackendPK, userbackendentity.UserID);
            userbackendentity.RowKey = userbackendentity.BackendID;
            return userbackendentity;
        }

        /// <summary>
        /// method which converts userbackend entity to dto by using mapper
        /// </summary>
        /// <param name="userbackendentity">takes user backend entity as input</param>
        /// <returns>returns user backend dto</returns>
        public UserBackendDTO UserBackendEntityDTOMapper(UserBackendEntity userbackendentity)
        {
            //convertig entity to data transfer object using mapper
            UserBackendDTO userbackenddto = DataProvider.ResponseObjectMapper<UserBackendDTO, UserBackendEntity>(userbackendentity);
            BackendDTO backenddto = DataProvider.ResponseObjectMapper<BackendDTO, UserBackendEntity>(userbackendentity);
            userbackenddto.backend = backenddto;
            return userbackenddto;
        }
    }
}
