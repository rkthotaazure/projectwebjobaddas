//-----------------------------------------------------------
// <copyright file="Rules.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using adidas.clb.MobileApproval.Models;

namespace adidas.clb.MobileApproval.Utility
{
    /// <summary>
    /// The class which implements methods for Rules.
    /// </summary>
    public static class Rules
    {
        /// <summary>
        /// method to caliculate synch waiting time
        /// </summary>
        /// <param name="userbackends">takes userbackends as input</param>
        /// <returns></returns>
        public static int SynchWaitingTime(IEnumerable<BackendEntity> backends)
        {
            int MaxLatency = backends.Max(m => Math.Max(m.AverageAllRequestsLatency, m.LastAllRequestsLatency));
            return Convert.ToInt32(MaxLatency * 1.2);
        }

        /// <summary>
        /// method to caliculate extended depth
        /// </summary>
        /// <param name="query">takes qury as input</param>
        /// <param name="userbackendslist">takes user backends as input</param>
        /// <param name="maxsynchreplysize">takes maxsynch reply size as input</param>
        /// <returns>returns true/false  for extendeddepth</returns>
        public static Boolean ExtendedDepthperAllBackends(SynchRequestDTO query, List<UserBackendEntity> userbackendslist, int maxsynchreplysize)
        {
            Boolean depth = (query.parameters.depth.overview || query.parameters.depth.genericInfo || query.parameters.depth.approvers);
            int totalrequestsize = 0;
            foreach (UserBackendEntity userbackend in userbackendslist)
            {
                totalrequestsize = totalrequestsize + (userbackend.AverageRequestSize * userbackend.OpenRequests);
            }
            return (depth && (totalrequestsize < maxsynchreplysize) || maxsynchreplysize == 0);
        }

        /// <summary>
        /// method to check backend updated
        /// </summary>
        /// <param name="userbackend">takes user backend as input</param>
        /// <returns>returns whether backend updated or not</returns>
        public static Boolean IsBackendUpdated(UserBackendEntity userbackend, SynchRequestDTO query)
        {
            if(userbackend.LastUpdate!=null)
            {
                return (!userbackend.UpdateTriggered) && (userbackend.DefaultUpdateFrequency > 0) && (!query.parameters.forceUpdate) && (userbackend.LastUpdate.Value.AddMinutes(userbackend.DefaultUpdateFrequency) > DateTime.Now);
            }
            else
            {
                return (!userbackend.UpdateTriggered) && (userbackend.DefaultUpdateFrequency > 0) && (!query.parameters.forceUpdate);
            }            
        }

        /// <summary>
        /// method to check backend update inprogress
        /// </summary>
        /// <param name="userbackend">takes user backend as input</param>
        /// <returns>returns whether backend update is in progress or not</returns>
        public static Boolean IsBackendUpdateInProgress(UserBackendEntity userbackend)
        {
            return userbackend.UpdateTriggered;
        }

        /// <summary>
        /// method to find backend retry time.
        /// </summary>
        /// <param name="userbackend">takes user backend as input</param>
        /// <returns>returns backend retry time.</returns>
        public static double BackendRetryTime(UserBackendEntity userbackend)
        {
            return Math.Max(userbackend.AverageAllRequestsLatency, userbackend.LastAllRequestsLatency) * 1.2;
        }

        /// <summary>
        /// method to check request updated
        /// </summary>
        /// <param name="request">takes user request as input</param>
        /// <param name="userbackend">takes user request as input</param>
        /// <returns>returns whether request updated or not</returns>
        public static Boolean IsRequestUpdated(RequestEntity request, int userbackendupdatefrequency)
        {
            if(request.LastUpdate!=null)
            {
                return (!request.UpdateTriggered) && (request.LastUpdate.Value.AddMinutes(userbackendupdatefrequency) > DateTime.Now);
            }
            else
            {
                return (!request.UpdateTriggered);
            }
        }

        /// <summary>
        /// method to check request update inprogress
        /// </summary>
        /// <param name="request">takes user request as input</param>
        /// <returns>returns whether request update is in progress or not</returns>
        public static Boolean IsRequestUpdateInProgress(RequestEntity request)
        {
            return request.UpdateTriggered;
        }

        /// <summary>
        /// method to find request retry time
        /// </summary>
        /// <param name="userbackend">takes userbackend as input</param>
        /// <returns>returns request retry time.</returns>
        public static double RequestRetryTime(UserBackendEntity userbackend)
        {
            return Math.Max(userbackend.AverageRequestLatency, userbackend.LastRequestLatency) * 1.2;
        }

        /// <summary>
        /// method to deicede only the filtered requests are sent back or all
        /// </summary>
        /// <param name="query">takes qury as input</param>
        /// <param name="request">takes request as input</param>
        /// <returns>returns whether filtered requests are sent back or all</returns>
        public static Boolean IsTargetRequest(SynchRequestDTO query, RequestEntity request)
        {
            //code here
            return true;
        }

        /// <summary>
        /// method to find extenddepth per backend
        /// </summary>
        /// <param name="userbackend">takes userbackend as input</param>
        /// <param name="maxsynchreplysize">takes maxsynchreplysize as input</param>
        /// <returns>returns extended depth per userbackend</returns>
        public static Boolean ExtendedDepthperBackend(UserBackendEntity userbackend, int maxsynchreplysize)
        {

            return ((userbackend.AverageRequestSize * userbackend.OpenRequests) < maxsynchreplysize) || maxsynchreplysize == 0;
        }

        /// <summary>
        /// method to filter out the requests changed/updated since the Last synch 
        /// </summary>
        /// <param name="query">takes qury as input</param>
        /// <param name="request">takes request as input</param>
        /// <returns>returns whether request is changed after last synch</returns>
        public static Boolean IsRequestATarget(SynchRequestDTO query, RequestEntity request)
        {

            return true;
        }

        /// <summary>
        /// method to get extended depth
        /// </summary>
        /// <param name="userbackend">takes userbackend as input</param>
        /// <param name="maxsynchreplysize">takes maxsynchreplysize as input</param>
        /// <returns>returns extended depth</returns>
        public static Boolean ExtendedDepth(UserBackendEntity userbackend, int maxsynchreplysize)
        {

            return (userbackend.AverageRequestSize < maxsynchreplysize) || maxsynchreplysize == 0;
        }
    }
}