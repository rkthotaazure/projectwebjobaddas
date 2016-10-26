//-----------------------------------------------------------
// <copyright file="Constants.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using adidas.clb.MobileApproval.Models;
using adidas.clb.MobileApproval.Utility;
using adidas.clb.MobileApproval.App_Code.DAL.Personalization;

namespace adidas.clb.MobileApproval.App_Code.BL.Personalization
{
    /// <summary>
    /// The class which implements methods for user personalization.
    /// </summary>
    public class Personalization
    {
        /// <summary>
        /// method to check user availability
        /// </summary>
        /// <param name="userprops"></param>
        /// <returns></returns>        
        public Boolean CheckUser(String UserName)
        {
            //code here..
            return true;
        }

        /// <summary>
        /// method to create newuser
        /// </summary>
        /// <param name="userprops"></param>
        /// <returns></returns>        
        public String CreateUser(UserEntity user)
        {
            //code here..
            return "username";
        }

        /// <summary>
        /// method to trigger user requsets update
        /// </summary>
        /// <param name="userprops"></param>
        /// <returns></returns>        
        public void TriggerUserRequests(String UserName)
        {
            //code here..
        }

        /// <summary>
        /// method to calculate sync waiting time
        /// </summary>
        /// <param name="userprops"></param>
        /// <returns></returns>        
        public int CalcSynchTime(List<UserBackendEntity> Backendtouser, int MaxSyncReplySize)
        {
            //code here..
            return 10;
        }

        /// <summary>
        /// method to get user details
        /// </summary>
        /// <param name="userprops"></param>
        /// <returns></returns>        
        public UserEntity GetUser(String UserName)
        {
            //code here..
            PersonalizationDAL personalizationdal = new PersonalizationDAL();
            UserEntity user=personalizationdal.GetUser(UserName);
            return user;
        }

        

    }

}
