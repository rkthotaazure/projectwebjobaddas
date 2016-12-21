//-----------------------------------------------------------
// <copyright file="synch.cs" company="adidas AG">
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
using adidas.clb.MobileApproval.App_Code.DAL.Synch;

namespace adidas.clb.MobileApproval.App_Code.BL.Synch
{
    /// <summary>
    /// The class which implements methods for business logic layer of synch.
    /// </summary>
    public class Synch
    {
        /// <summary>
        /// method to get list of backends associated to user
        /// </summary>
        /// <param name="userID">takes userid as input</param>
        /// <returns>returns list of userbackends</returns>
        public List<UserBackendEntity> GetUserBackendsList(string userID,List<string> userbackends)
        {
            try
            {
                //if userbackendid's not provided get all associated backends to user by default
                if(userbackends!=null)
                {
                    SynchDAL synchdDAL = new SynchDAL();
                    //calling data access layer method                
                    return synchdDAL.GetUserAllBackends(userID, userbackends);
                }
                else
                {
                    UserBackendDAL userbackenddal = new UserBackendDAL();
                    return userbackenddal.GetUserAllBackends(userID);
                }
                
            }
            catch (DataAccessException DALexception)
            {
                throw DALexception;
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error in BL while getting userbackend list per user : "
                       + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new BusinessLogicException();
            }
        }

        /// <summary>
        /// method to add message to queue to trigger userbackend update
        /// </summary>
        /// <param name="userbackend">takes userbackend as input</param>
        public void TriggerUserBackendUpdate(UserBackendEntity userbackend)
        {
            try
            {
                UpdateTriggeringMessage updateTriggerMessage = new UpdateTriggeringMessage();
                UserUpdateMsg usermsg = new UserUpdateMsg();
                usermsg.UserID = userbackend.UserID;                               
                List<UpdateTriggerBackend> updatetriggerbackendlist = new List<UpdateTriggerBackend>();
                //adding backend to message object
                UpdateTriggerBackend triggerbackend = new UpdateTriggerBackend();
                triggerbackend.BackendID = userbackend.BackendID;
                updateTriggerMessage.ChangeAfter = userbackend.LastUpdate;
                updatetriggerbackendlist.Add(triggerbackend);
                usermsg.Backends = updatetriggerbackendlist;
                //creating list to add users                
                List<UserUpdateMsg> usermsglist = new List<UserUpdateMsg>();
                usermsglist.Add(usermsg);
                updateTriggerMessage.Users = usermsglist;
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
        /// method to add message to queue to trigger request update
        /// </summary>
        /// <param name="request">takes request as input</param>
        public void TriggerRequestUpdate(RequestEntity request)
        {
            try
            {
                UpdateTriggeringMessage updateTriggerMessage = new UpdateTriggeringMessage();
                List<RequestUpdateMsg> updatetriggerrequestlist = new List<RequestUpdateMsg>();
                RequestUpdateMsg triggerrequset = new RequestUpdateMsg();
                //adding request to message object
                Request requestobj = new Request();
                requestobj.ID = request.id;
                triggerrequset.request = requestobj;
                //adding backend to queue message
                UpdateTriggerBackend backendobj = new UpdateTriggerBackend();
                backendobj.BackendID = request.BackendID;
                triggerrequset.request.Backend = backendobj;
                //add requests to list which will be added to message
                updatetriggerrequestlist.Add(triggerrequset);
                updateTriggerMessage.Requests = updatetriggerrequestlist;
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
        /// method to get all requets of user
        /// </summary>
        /// <param name="userID">takes userid as input</param>
        /// <returns>reurns list of requests for user</returns>
        public List<RequestEntity> GetUserRequests(string userID,string requeststatus)
        {
            try
            {
                //if requeststatus is null, get requests with defaultstatus inprogress.
                if(string.IsNullOrEmpty(requeststatus))
                {
                    requeststatus = CoreConstants.AzureTables.InProgress;
                }
                SynchDAL synchDAL = new SynchDAL();
                //calling data access layer method                
                return synchDAL.GetUserRequests(userID, requeststatus);
            }
            catch (DataAccessException DALexception)
            {
                throw DALexception;
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error in BL while getting all requets per user  : "
                       + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new BusinessLogicException();
            }
        }

        /// <summary>
        /// method to get userbackend
        /// </summary>
        /// <param name="userID">takes userid as input</param>
        /// <param name="backendID">takes backendid as input</param>
        /// <returns>returns userbackend</returns>
        public UserBackendEntity GetUserBackend(string userID, string backendID)
        {
            try
            {
                UserBackendDAL userbackendDAL = new UserBackendDAL();
                //calling data access layer method                
                return userbackendDAL.GetUserBackend(userID, backendID);
            }
            catch (DataAccessException DALexception)
            {
                throw DALexception;
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error in BL while getting userbackend : "
                       + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new BusinessLogicException();
            }
        }

        /// <summary>
        /// method to get Requests per userbackend
        /// </summary>
        /// <param name="userID">takes userid as input</param>
        /// <param name="backendID">takes backendid as input</param>
        /// <returns>returns Requests associated to userbackend</returns>
        public List<RequestEntity> GetUserBackendRequests(string userID, string backendID,string requeststatus)
        {
            try
            {
                //if requeststatus is null, get requests with defaultstatus inprogress.
                if (string.IsNullOrEmpty(requeststatus))
                {
                    requeststatus = CoreConstants.AzureTables.InProgress;
                }
                SynchDAL synchDAL = new SynchDAL();
                //calling data access layer method                
                return synchDAL.GetUserBackendRequests(userID, backendID,requeststatus);
            }
            catch (DataAccessException DALexception)
            {
                throw DALexception;
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error in BL while getting requests per userbackend : "
                       + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new BusinessLogicException();
            }
        }

        /// <summary>
        /// method to get request with id
        /// </summary>
        /// <param name="requestID">takes requestid as input</param>
        /// <returns>returns request</returns>
        public RequestEntity GetRequest(string userID,string requestID)
        {
            try
            {
                SynchDAL synchDAL = new SynchDAL();
                //calling data access layer method                
                return synchDAL.GetRequest(userID,requestID);
            }
            catch (DataAccessException DALexception)
            {
                throw DALexception;
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error in BL while getting request : "
                       + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new BusinessLogicException();
            }
        }
        
        /// <summary>
        /// method to get approvers list per request
        /// </summary>
        /// <param name="requestID">takes request id as input</param>
        /// <returns>returns list of approvers</returns>
        public List<ApproverDTO> GetApprovers(string requestID)
        {
            try
            {
                SynchDAL synchDAL = new SynchDAL();
                //calling data access layer method 
                List<ApproverEntity> approversentitylist = synchDAL.GetApprovers(requestID);
                List<ApproverDTO> approvers = new List<ApproverDTO>();
                //loop through approvers list entity to convert to approvers dto
                foreach (ApproverEntity approver in approversentitylist)
                {
                    ApproverDTO approverdto = DataProvider.ResponseObjectMapper<ApproverDTO, ApproverEntity>(approver);
                    approvers.Add(approverdto);
                }
                return approvers;
            }
            catch (DataAccessException DALexception)
            {
                throw DALexception;
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error in BL while getting approvers per request : "
                       + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new BusinessLogicException();
            }
        }

        /// <summary>
        /// method to get fields for request
        /// </summary>
        /// <param name="requestID">takes requestid as input</param>
        /// <returns>returns list of fields</returns>
        public List<FieldDTO> GetFields(string requestID)
        {
            try
            {
                SynchDAL synchDAL = new SynchDAL();
                //calling data access layer method 
                List<FieldEntity> fieldsentitylist = synchDAL.GetFields(requestID);
                List<FieldDTO> fileds = new List<FieldDTO>();
                //loop through fields list entity to convert to fields dto
                foreach (FieldEntity field in fieldsentitylist)
                {
                    FieldDTO fielddto = DataProvider.ResponseObjectMapper<FieldDTO, FieldEntity>(field);
                    fileds.Add(fielddto);
                }
                return fileds;
            }
            catch (DataAccessException DALexception)
            {
                throw DALexception;
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error in BL while getting fields per request : "
                       + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new BusinessLogicException();
            }
        }

        /// <summary>
        /// method to add or update userbackend synch
        /// </summary>
        /// <param name="userbackend"></param>
                        
        public void AddUpdateBackendSynch(UserBackendEntity userbackend)
        {
            try
            {
                SynchDAL synchDAL = new SynchDAL();
                SynchEntity backendsynch=synchDAL.GetUserBackendSynch(userbackend.UserID,userbackend.BackendID);
                //backend synch available then update
                if(backendsynch!=null)
                {
                    //last synch frequency
                    backendsynch.lastSynchFreq = (backendsynch.LastSynch - DateTime.Now).Days;
                    //update best synch frequency
                    if(backendsynch.lastSynchFreq<backendsynch.bestSynchFreq)
                    {
                        backendsynch.bestSynchFreq = backendsynch.lastSynchFreq;
                    }
                    backendsynch.avgSynchFreq = (backendsynch.lastSynchFreq + (backendsynch.SynchCount * backendsynch.avgSynchFreq)) / (backendsynch.SynchCount + 1);
                    backendsynch.SynchCount = backendsynch.SynchCount + 1;
                    backendsynch.LastSynch = DateTime.Now;
                    //calling data access layer method                
                    synchDAL.AddUpdateBackendSynch(backendsynch);
                }
                else
                {
                    SynchEntity newbackendsynch = new SynchEntity();
                    newbackendsynch.PartitionKey = string.Concat(CoreConstants.AzureTables.BackendSynchPK, userbackend.UserID);
                    newbackendsynch.RowKey = userbackend.BackendID;
                    newbackendsynch.LastSynch = DateTime.Now;
                    newbackendsynch.SynchCount = 1;
                    //calling data access layer method                
                    synchDAL.AddUpdateBackendSynch(newbackendsynch);
                }
                
            }
            catch (DataAccessException DALexception)
            {
                throw DALexception;
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error in BL while adding userbackend synch  : "
                       + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new BusinessLogicException();
            }
        }

        /// <summary>
        /// method add or update request synch
        /// </summary>
        /// <param name="request"></param>
        public void AddUpdateRequestSynch(RequestEntity request)
        {
            try
            {
                SynchDAL synchDAL = new SynchDAL(); 
                RequestSynchEntity requestsynch= synchDAL.GetRequestSynch(CoreConstants.AzureTables.RequestSynchPK, request.id);
                if (requestsynch != null)
                {
                    //last synch frequency
                    requestsynch.LastChange = DateTime.Now;
                    //calling data access layer method                
                    synchDAL.AddUpdateRequestSynch(requestsynch);
                }
                else
                {
                    RequestSynchEntity newrequestsynch = new RequestSynchEntity();
                    newrequestsynch.PartitionKey = CoreConstants.AzureTables.RequestSynchPK;
                    newrequestsynch.RowKey = request.id;
                    newrequestsynch.LastChange = DateTime.Now;
                    //calling data access layer method                
                    synchDAL.AddUpdateRequestSynch(newrequestsynch);
                }                
            }
            catch (DataAccessException DALexception)
            {
                throw DALexception;
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error in BL while adding request synch  : "
                       + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new BusinessLogicException();
            }
        }
    }
}