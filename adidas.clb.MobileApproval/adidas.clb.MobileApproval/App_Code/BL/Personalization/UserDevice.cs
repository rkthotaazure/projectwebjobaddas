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
        /// <param name="deviceofuser">takes list of user devices as input</param>             
        public void AddDevices(List<UserDeviceEntity> deviceofuser)
        {
            try
            {
                UserDeviceDAL userdevicedal = new UserDeviceDAL();
                //calling data access layer method
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
        ///method to remove userdevices
        /// </summary>
        /// <param name="UserID">takes userid as input</param>        
        public void RemoveDevices(string UserID)
        {
            try
            {
                UserDeviceDAL userdevicedal = new UserDeviceDAL();
                //get all user devices to remove
                List<UserDeviceEntity> alluserdevices = userdevicedal.GetUserAllDevices(UserID);
                if (alluserdevices != null)
                {
                    //calling data access layer method to remove
                    userdevicedal.RemoveDevices(alluserdevices);
                }
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
        /// <param name="UserID">takes userid as input</param>
        /// <returns>returns list of user device</returns>
        public IEnumerable<UserDeviceDTO> GetUserAllDevices(string UserID)
        {
            try
            {
                UserDeviceDAL userdevicedal = new UserDeviceDAL();
                //calling data access layer method
                List<UserDeviceEntity> alluserdevices = userdevicedal.GetUserAllDevices(UserID);
                List<UserDeviceDTO> userdevicesdtolist = new List<UserDeviceDTO>();
                //check for null
                if (alluserdevices != null)
                {
                    //converting userdevice entity to userdevice dto
                    foreach (UserDeviceEntity userdeviceentity in alluserdevices)
                    {
                        userdevicesdtolist.Add(UserDeviceEntityDTOMapper(userdeviceentity));
                    }

                }
                else
                {
                    userdevicesdtolist = null;
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
        /// method to get single userdevice
        /// </summary>
        /// <param name="userID">takes userid as input</param>
        /// <param name="userDeviceID">takes user device id as input</param>
        /// <returns>returns user device with id associated to user in the form of personalization response</returns>
        public PersonalizationResponseDTO<UserDeviceDTO> GetUserDevice(string userID, string userDeviceID)
        {
            try
            {
                UserDeviceDAL userdevicedal = new UserDeviceDAL();
                UserDeviceEntity userdeviceentity = userdevicedal.GetUserDevice(userID, userDeviceID);
                //converting userdevice entity to Response data transfer object
                var ResponseUserDevice = new PersonalizationResponseDTO<UserDeviceDTO>();
                if (userdeviceentity != null)
                {
                    UserDeviceDTO userdevicedto = UserDeviceEntityDTOMapper(userdeviceentity);
                    ResponseUserDevice.result = userdevicedto;
                }
                else
                {
                    ResponseUserDevice.result = null;
                }                           
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
        /// <param name="userID">takes userid as input</param>
        /// <param name="userDeviceID">takes user device id as input</param>
        /// <returns>returns deleted user backend entity</returns>
        public UserDeviceEntity DeleteUserDevice(string userID, string userDeviceID)
        {
            try
            {
                UserDeviceDAL userdevicedal = new UserDeviceDAL();
                //calling data access layer method 
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
        /// <param name="userdevicesdto">takes user device dto as input<</param>
        /// <returns>returns user device entity</returns>
        public IEnumerable<UserDeviceEntity> UserDeviceEntityGenerator(IEnumerable<UserDeviceDTO> userdevicesdtolist)
        {            
            List<UserDeviceEntity> userdeviceentitylist = new List<UserDeviceEntity>();
            //mapper to convert entity to data transfer object
            foreach (UserDeviceDTO userdevicedto in userdevicesdtolist)
            {                                
                userdeviceentitylist.Add(UserDeviceDTOEntityMapper(userdevicedto));
            }
            return (IEnumerable<UserDeviceEntity>)userdeviceentitylist;
        }

        /// <summary>
        /// method which converts userdevice dto to entity by using mapper, first maps devicedto with userdeviceentity
        /// </summary>
        /// <param name="userdevicedto">takes user device dto as input</param>
        /// <returns>returns user backend entity</returns>
        public UserDeviceEntity UserDeviceDTOEntityMapper(UserDeviceDTO userdevicedto)
        {
            //converting input device data transfer object(dto) list to userdevice entity list by using property mapper
            DeviceDTO devicedto = userdevicedto.device;
            UserDeviceEntity userdeviceentity = DataProvider.ResponseObjectMapper<UserDeviceEntity, DeviceDTO>(devicedto);
            //adding missing properties like userID, partitionkey and Rowkey to entity
            userdeviceentity.DeviceID = string.Concat(userdevicedto.UserID, CoreConstants.AzureTables.UnderScore, userdevicedto.device.DeviceName);
            userdeviceentity.UserID = userdevicedto.UserID;
            userdeviceentity.PartitionKey = string.Concat(CoreConstants.AzureTables.UserDevicePK, userdeviceentity.UserID);
            userdeviceentity.RowKey = userdeviceentity.DeviceID;
            return userdeviceentity;
        }

        /// <summary>
        /// method which converts userdevice entity to dto by using mapper
        /// </summary>
        /// <param name="userdeviceentity">takes user device entity as input</param>
        /// <returns>returns user device dto</returns>
        public UserDeviceDTO UserDeviceEntityDTOMapper(UserDeviceEntity userdeviceentity)
        {
            //convertig entity to data transfer object using mapper
            UserDeviceDTO userdevicedto = DataProvider.ResponseObjectMapper<UserDeviceDTO, UserDeviceEntity>(userdeviceentity);
            DeviceDTO devicedto = DataProvider.ResponseObjectMapper<DeviceDTO, UserDeviceEntity>(userdeviceentity);
            userdevicedto.device = devicedto;
            return userdevicedto;
        }
    }
}