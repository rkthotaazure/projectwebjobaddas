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
        /// <param name="Backendtouser"></param>
        /// <returns></returns>        
        public void AddBackends(List<UserBackendEntity> Backendtouser)
        {
            try
            {
                UserBackendDAL userbackenddal = new UserBackendDAL();
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
        ///method to update userbackends
        /// </summary>
        /// <param name="Backendtouser"></param>
        /// <returns></returns>        
        public void UpdateBackends(List<UserBackendEntity> Backendtouser)
        {
            try
            {
                UserBackendDAL userbackenddal = new UserBackendDAL();
                userbackenddal.UpdateBackends(Backendtouser);
            }
            catch (DataAccessException DALexception)
            {
                throw DALexception;
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error in BL while updating userbackends : "
                       + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new BusinessLogicException();
            }
        }

        /// <summary>
        ///method to remove userbackends
        /// </summary>
        /// <param name="UserID"></param>
        /// <param name="associatedbackends"></param>
        /// <returns></returns>        
        public void RemoveBackends(String UserID)
        {
            try
            {
                UserBackendDAL userbackenddal = new UserBackendDAL();
                //to get all backends associated to user
                List<UserBackendEntity> alluserbackends = userbackenddal.GetUserAllBackends(UserID);
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
        /// <returns></returns>
        public PersonalizationResponseListDTO<BackendDTO> GetBackends()
        {
            try
            {
                UserBackendDAL userbackenddal = new UserBackendDAL();
                List<BackendEntity> backends = userbackenddal.GetBackends();
                List<BackendDTO> backendslistdto = new List<BackendDTO>();
                foreach(BackendEntity backend in backends)
                {
                    //backendsentity converting to response dto
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
        /// <param name="UserID"></param>
        /// <returns></returns>
        public IEnumerable<UserBackendDTO> GetUserAllBackends(String UserID)
        {
            try
            {
                UserBackendDAL userbackenddal = new UserBackendDAL();
                List<UserBackendEntity> alluserbackends = userbackenddal.GetUserAllBackends(UserID);
                List<SynchEntity> allbackendsynch = userbackenddal.GetAllUserBackendsSynch(UserID);
                //converting userdevice entity to userdevice data transfer object
                List<UserBackendDTO> userbackendsdtolist = new List<UserBackendDTO>();
                foreach (UserBackendEntity userbackendentity in alluserbackends)
                {
                    UserBackendDTO userbackenddto = UserBackendEntityDTOMapper(userbackendentity);
                    SynchEntity synchentity=allbackendsynch.Where(m => m.RowKey.Equals(userbackendentity.BackendID)).FirstOrDefault();
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
        /// <param name="userbackendentity"></param>
        public void PostBackends(PersonalizationRequsetDTO personalizationrequset)
        {
            try
            {
                UserBackendDAL userbackenddal = new UserBackendDAL();
                //get userbackend from array of ienumearble items
                UserBackendDTO userbackendto = personalizationrequset.userbackends.FirstOrDefault();
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
        /// <param name="userID"></param>
        /// <param name="userBackendID"></param>
        /// <returns></returns>
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
        /// <param name="userID"></param>
        /// <param name="userBackendID"></param>
        /// <returns></returns>
        public UserBackendEntity DeleteUserBackend(String userID, String userBackendID)
        {
            try
            {
                UserBackendDAL userbackenddal = new UserBackendDAL();
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
        /// <param name="userbackendsdto"></param>
        /// <returns></returns>        
        public IEnumerable<UserBackendEntity> UserBackendEntityGenerator(IEnumerable<UserBackendDTO> userbackendsdtolist)
        {
            List<UserBackendEntity> userbackendentitylist = new List<UserBackendEntity>();
            foreach (UserBackendDTO userbackenddto in userbackendsdtolist)
            {
                userbackendentitylist.Add(UserBackendDTOEntityMapper(userbackenddto));
            }
            return (IEnumerable<UserBackendEntity>)userbackendentitylist;
        }

        /// <summary>
        /// method which converts userbackend dto to entity by using mapper, first maps backenddto with userbackendentity
        /// </summary>
        /// <param name="userbackenddto"></param>
        /// <returns></returns>
        public UserBackendEntity UserBackendDTOEntityMapper(UserBackendDTO userbackenddto)
        {
            //converting input backend data transfer object(dto) list to userbackend entity list by using property mapper
            BackendDTO backenddto = userbackenddto.backend;
            UserBackendEntity userbackendentity = DataProvider.ResponseObjectMapper<UserBackendEntity, BackendDTO>(backenddto);
            userbackendentity.UserID = userbackenddto.UserID;
            userbackendentity.PartitionKey = string.Concat(CoreConstants.AzureTables.UserBackendPK, userbackendentity.UserID);
            userbackendentity.RowKey = userbackendentity.BackendID;
            return userbackendentity;
        }

        /// <summary>
        /// method which converts userbackend entity to dto by using mapper
        /// </summary>
        /// <param name="userbackendentity"></param>
        /// <returns></returns>
        public UserBackendDTO UserBackendEntityDTOMapper(UserBackendEntity userbackendentity)
        {
            UserBackendDTO userbackenddto = DataProvider.ResponseObjectMapper<UserBackendDTO, UserBackendEntity>(userbackendentity);
            BackendDTO backenddto = DataProvider.ResponseObjectMapper<BackendDTO, UserBackendEntity>(userbackendentity);
            userbackenddto.backend = backenddto;
            return userbackenddto;
        }
    }
}
