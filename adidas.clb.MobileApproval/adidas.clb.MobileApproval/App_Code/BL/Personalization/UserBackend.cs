//-----------------------------------------------------------
// <copyright file="Constants.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using adidas.clb.MobileApproval.Models;
using adidas.clb.MobileApproval.App_Code.DAL.Personalization;

namespace adidas.clb.MobileApproval.App_Code.BL.Personalization
{
    /// <summary>
    /// The class which implements methods for user personalization.
    /// </summary>
    public class UserBackend
    {
        /// <summary>
        /// method to updateexistinguserprops
        /// </summary>
        /// <param name="userprops"></param>
        /// <returns></returns>
        public String UpdateUserProp(UserEntity userprops)
        {
            //code here..
            return "username";
        }

        /// <summary>
        ///method to remove associate devices to user
        /// </summary>
        /// <param name="userprops"></param>
        /// <returns></returns>        
        public void RemoveDevices(String UserName, List<UserDeviceEntity> devicetouser)
        {
            //code here..
        }

        /// <summary>
        ///method to remove associate backends to user
        /// </summary>
        /// <param name="userprops"></param>
        /// <returns></returns>        
        public void RemoveBackends(String UserName, List<UserBackendEntity> Backendtouser)
        {
            //code here..
        }

        /// <summary>
        ///method to associate devices to user
        /// </summary>
        /// <param name="userprops"></param>
        /// <returns></returns>        
        public String AddDevices(List<UserDeviceEntity> devicetouser)
        {
            //code here..
            return "username";
        }

        /// <summary>
        ///method to update all device association properties to user
        /// </summary>
        /// <param name="userprops"></param>
        /// <returns></returns>        
        public void UpdateDevices(List<UserDeviceEntity> devicetouser)
        {
            //code here..
        }

        /// <summary>
        ///method to associate backends to user
        /// </summary>
        /// <param name="userprops"></param>
        /// <returns></returns>        
        public String AddBackends(List<UserBackendEntity> Backendtouser)
        {
            //code here..
            return "username";
        }

        /// <summary>
        ///method to update all backend association properties to user
        /// </summary>
        /// <param name="userprops"></param>
        /// <returns></returns>        
        public void UpdateBackends(List<UserBackendEntity> Backendtouser)
        {
            //code here..
        }

        /// <summary>
        ///method to get list of associated devices to user which were in user provided list
        /// </summary>
        /// <param name="userprops"></param>
        /// <returns></returns>        
        public List<UserDeviceEntity> GetAssociatedDevices(List<UserDeviceEntity> userprovideddevices)
        {
            //code here..
            return userprovideddevices;
        }

        /// <summary>
        ///method to get list of non-associated devices to user which were in user provided list
        /// </summary>
        /// <param name="userprops"></param>
        /// <returns></returns>        
        public List<UserDeviceEntity> GetNonAssociatedDevices(List<UserDeviceEntity> userprovideddevices, List<UserDeviceEntity> associateddevices)
        {
            //code here..
            return associateddevices;
        }

        /// <summary>
        ///method to get list of associated backends to user which were in user provided list
        /// </summary>
        /// <param name="userprops"></param>
        /// <returns></returns>        
        public List<UserBackendEntity> GetAssociatedBackends(List<UserBackendEntity> userprovidedbackends)
        {
            //code here..
            return userprovidedbackends;
        }

        /// <summary>
        ///method to get list of non-associated backends to user which were in user provided list
        /// </summary>
        /// <param name="userprops"></param>
        /// <returns></returns>        
        public List<UserBackendEntity> GetNonAssociatedBackends(List<UserBackendEntity> userprovidedbackends, List<UserBackendEntity> associatedbackends)
        {
            //code here..
            return associatedbackends;
        }

        /// <summary>
        ///method to get list of non-associated backends to user which were in user provided list
        /// </summary>
        /// <param name="userprops"></param>
        /// <returns></returns>        
        public List<BackendEntity> GetBackends()
        {
            UserBackendDAL userbackenddal = new UserBackendDAL();
            List < BackendEntity > backends = userbackenddal.GetBackends();
            return backends;
        }        
    }
}