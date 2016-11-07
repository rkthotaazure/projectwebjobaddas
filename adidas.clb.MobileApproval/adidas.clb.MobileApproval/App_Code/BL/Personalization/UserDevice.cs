//-----------------------------------------------------------
// <copyright file="UserDevice.cs" company="adidas AG">
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
    /// The class which implements methods for business logic layer of userdevice.
    /// </summary>
    public class UserDevice
    {
        /// <summary>
        ///method to add userdevices
        /// </summary>
        /// <param name="deviceofuser"></param>
        /// <returns></returns>        
        public void AddDevices(List<UserDeviceEntity> deviceofuser)
        {
            try
            {
                UserDeviceDAL userdevicedal = new UserDeviceDAL();
                userdevicedal.AddDevices(deviceofuser);
            }
            catch (DataAccessException DALexception)
            {
                throw DALexception;
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error in BL while adding userdevices : "
                       + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new BusinessLogicException();
            }
        }

        /// <summary>
        ///method to update userdevice
        /// </summary>
        /// <param name="deviceofuser"></param>
        /// <returns></returns>        
        public void UpdateDevices(List<UserDeviceEntity> deviceofuser)
        {
            try
            {
                UserDeviceDAL userdevicedal = new UserDeviceDAL();
                userdevicedal.UpdateDevices(deviceofuser);
            }
            catch (DataAccessException DALexception)
            {
                throw DALexception;
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error in BL while updating userdevices : "
                       + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new BusinessLogicException();
            }
        }

        /// <summary>
        ///method to remove userdevices
        /// </summary>
        /// <param name="UserID"></param>
        /// <param name="associateddevices"></param>
        /// <returns></returns>        
        public void RemoveDevices(String UserID)
        {
            try
            {
                UserDeviceDAL userdevicedal = new UserDeviceDAL();
                List<UserDeviceEntity> alluserdevices = userdevicedal.GetUserAllDevices(UserID);
                userdevicedal.RemoveDevices(alluserdevices);
            }
            catch (DataAccessException DALexception)
            {
                throw DALexception;
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error in BL while removing userdevices : "
                       + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new BusinessLogicException();
            }
        }

        /// <summary>
        /// method to get userdevices
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public IEnumerable<UserDeviceDTO> GetUserAllDevices(String UserID)
        {
            try
            {
                UserDeviceDAL userdevicedal = new UserDeviceDAL();
                List<UserDeviceEntity> alluserdevices = userdevicedal.GetUserAllDevices(UserID);
                //converting userdevice entity to userdevice userdevice dto
                List<UserDeviceDTO> userdevicesdtolist = new List<UserDeviceDTO>();
                foreach (UserDeviceEntity userdeviceentity in alluserdevices)
                {
                    userdevicesdtolist.Add(UserDeviceEntityDTOMapper(userdeviceentity));
                }
                return (IEnumerable<UserDeviceDTO>)userdevicesdtolist;
            }
            catch (DataAccessException DALexception)
            {
                throw DALexception;
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error in BL while retreiving userdevices : "
                       + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new BusinessLogicException();
            }
        }

        /// <summary>
        /// method to insert single userdevice
        /// </summary>
        /// <param name="userdeviceentity"></param>
        public void PostDevices(PersonalizationRequsetDTO personalizationrequset)
        {
            try
            {
                UserDeviceDAL userdevicedal = new UserDeviceDAL();
                //get userdevice from array of ienumearble items
                UserDeviceDTO userdevicedto = personalizationrequset.userdevices.FirstOrDefault();
                userdevicedal.PostDevices(UserDeviceDTOEntityMapper(userdevicedto));
            }
            catch (DataAccessException DALexception)
            {
                throw DALexception;
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error in BL while inserting single userdevice : "
                       + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new BusinessLogicException();
            }
        }

        /// <summary>
        /// method to get single userdevice
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="userDeviceID"></param>
        /// <returns></returns>
        public PersonalizationResponseDTO<UserDeviceDTO> GetUserDevice(String userID, String userDeviceID)
        {
            try
            {
                UserDeviceDAL userdevicedal = new UserDeviceDAL();
                UserDeviceEntity userdeviceentity = userdevicedal.GetUserDevice(userID, userDeviceID);
                //converting userdevice entity to Response data transfer object
                var ResponseUserDevice = new PersonalizationResponseDTO<UserDeviceDTO>();
                UserDeviceDTO userdevicedto = UserDeviceEntityDTOMapper(userdeviceentity);                
                ResponseUserDevice.result = userdevicedto;
                return ResponseUserDevice;
            }
            catch (DataAccessException DALexception)
            {
                throw DALexception;
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error in BL while retreiving single userdevice: "
                       + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new BusinessLogicException();
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
                UserDeviceDAL userdevicedal = new UserDeviceDAL();
                UserDeviceEntity userdeviceentity = userdevicedal.DeleteUserDevice(userID, userDeviceID);
                return userdeviceentity;
            }
            catch (DataAccessException DALexception)
            {
                throw DALexception;
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error in BL while deleting single userdevice : "
                       + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new BusinessLogicException();
            }

        }

        /// <summary>
        /// Converting userdevice input DTO to userdeviceentity with partitionkey, Rowkey
        /// </summary>
        /// <param name="userdevicesdto"></param>
        /// <returns></returns>
        public IEnumerable<UserDeviceEntity> UserDeviceEntityGenerator(IEnumerable<UserDeviceDTO> userdevicesdtolist)
        {
            List<UserDeviceEntity> userdeviceentitylist = new List<UserDeviceEntity>();
            foreach (UserDeviceDTO userdevicedto in userdevicesdtolist)
            {
                //converting input userbackend data transfer object list to entity list                
                userdeviceentitylist.Add(UserDeviceDTOEntityMapper(userdevicedto));
            }
            return (IEnumerable<UserDeviceEntity>)userdeviceentitylist;
        }

        /// <summary>
        /// method which converts userdevice dto to entity by using mapper, first maps devicedto with userdeviceentity
        /// </summary>
        /// <param name="userdevicedto"></param>
        /// <returns></returns>
        public UserDeviceEntity UserDeviceDTOEntityMapper(UserDeviceDTO userdevicedto)
        {
            //converting input device data transfer object(dto) list to userdevice entity list by using property mapper
            DeviceDTO devicedto = userdevicedto.device;
            UserDeviceEntity userdeviceentity = DataProvider.ResponseObjectMapper<UserDeviceEntity, DeviceDTO>(devicedto);
            userdeviceentity.UserID = userdevicedto.UserID;
            userdeviceentity.PartitionKey = string.Concat(CoreConstants.AzureTables.UserDevicePK, userdeviceentity.UserID);
            userdeviceentity.RowKey = userdeviceentity.DeviceID;
            return userdeviceentity;
        }

        /// <summary>
        /// method which converts userdevice entity to dto by using mapper
        /// </summary>
        /// <param name="userdeviceentity"></param>
        /// <returns></returns>
        public UserDeviceDTO UserDeviceEntityDTOMapper(UserDeviceEntity userdeviceentity)
        {
            UserDeviceDTO userdevicedto = DataProvider.ResponseObjectMapper<UserDeviceDTO, UserDeviceEntity>(userdeviceentity);
            DeviceDTO devicedto = DataProvider.ResponseObjectMapper<DeviceDTO, UserDeviceEntity>(userdeviceentity);
            userdevicedto.device = devicedto;
            return userdevicedto;
        }
    }
}