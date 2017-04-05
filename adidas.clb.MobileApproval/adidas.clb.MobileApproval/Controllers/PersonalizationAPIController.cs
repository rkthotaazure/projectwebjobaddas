//-----------------------------------------------------------
// <copyright file="PersonalizationAPIController.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using adidas.clb.MobileApproval.Exceptions;
using adidas.clb.MobileApproval.Models;
using adidas.clb.MobileApproval.Utility;
using adidas.clb.MobileApproval.App_Code.BL.Personalization;

namespace adidas.clb.MobileApproval.Controllers
{
    /// <summary>
    /// The controller class which implements action methods for personalization
    /// </summary> 
   [Authorize]
    public class PersonalizationAPIController : ApiController
    {
        //Application insights interface reference for logging the error details into Application Insight azure service.
        static IAppInsight InsightLogger { get { return AppInsightLogger.Instance; } }
        /// <summary>
        /// action method to get all backends
        /// </summary>
        /// <returns>Returns list of backends</returns>        
        [Route("api/personalizationapi/backends")]
        public HttpResponseMessage GetBackends()
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                InsightLogger.TrackEvent("PersonalizationAPIController :: endpoint : api/personalizationapi/backends, action : starting method");
                UserBackend userBackend = new UserBackend();                
                PersonalizationResponseListDTO<BackendDTO> allBackends = userBackend.GetBackends();
                InsightLogger.TrackEvent("PersonalizationAPIController :: endpoint : api/personalizationapi/backends, action : getting backends, response: successs");
                //if backends exists return result other wise return status code as NotFound
                if (allBackends.result != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, allBackends);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, DataProvider.PersonalizationResponseError<UserBackendDTO>("400", "User does not have backends", ""));
                }
            }
            catch (DataAccessException dalexception)
            {
                InsightLogger.Exception(dalexception.Message, dalexception, callerMethodName);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserBackendDTO>("400", dalexception.Message, dalexception.Message));
            }
            catch (BusinessLogicException blexception)
            {
                InsightLogger.Exception(blexception.Message, blexception, callerMethodName);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserBackendDTO>("400", blexception.Message, blexception.Message));
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                //LoggerHelper.WriteToLog(exception + " - exception in controller action while retrieving all backends : "
                //      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserBackendDTO>("400", exception.Message, exception.StackTrace));
            }
        }

        /// <summary>
        /// action to insert or update user
        /// </summary>
        /// <param name="personalizationrequset"></param>       
        /// <returns>returns user along with is associated devices and backends</returns>        
        [Route("api/personalizationapi/users/{userID}")]
        public HttpResponseMessage PutUsers(PersonalizationRequsetDTO personalizationrequset)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                InsightLogger.TrackEvent("PersonalizationAPIController ::endpoint : api/personalizationapi/users/{userID}, action: start put user");
                //BL object instances created
                Personalization personalization = new Personalization();
                UserBackend userbackend = new UserBackend();
                UserDevice userdevice = new UserDevice();
                //if userID is available in requset then check user exist or not.
                if (personalizationrequset!=null && !string.IsNullOrEmpty(personalizationrequset.user.UserID))
                {                    
                    Boolean isUserExists = personalization.CheckUser(personalizationrequset.user.UserID);
                    Boolean isDevicesProvided, isBackendsProvided;

                    //retrackting individual objects from request
                    IEnumerable<UserDeviceDTO> userdevicesdto = personalizationrequset.user.userdevices;
                    IEnumerable<UserBackendDTO> userbackendsdto = personalizationrequset.user.userbackends;
                    UserDTO user = personalizationrequset.user;
                    UserEntity userentity = personalization.UserEntityGenerator(user);
                    IEnumerable<UserDeviceEntity> userprovideddevices=null;
                    IEnumerable<UserBackendEntity> userprovidedbackends = null;

                    //to check if requset has userdevices or not
                    if (userdevicesdto != null)
                    {
                        isDevicesProvided = true;
                        userprovideddevices = userdevice.UserDeviceEntityGenerator(userdevicesdto);
                    }
                    else { isDevicesProvided = false; }

                    //to check if requset has userbackends or not
                    if (userbackendsdto != null)
                    {
                        isBackendsProvided = true;
                        userprovidedbackends = userbackend.UserBackendEntityGenerator(userbackendsdto);
                    }
                    else { isBackendsProvided = false; }
                    UserDTO updateduser;
                    //create if user not exists else update
                    if (!isUserExists)
                    {
                        InsightLogger.TrackEvent("PersonalizationAPIController :: endpoint : api/personalizationapi/users/{userID}, action: check user, response: false,userID: " + personalizationrequset.user.UserID);
                        personalization.CreateUser(userentity);
                        InsightLogger.TrackEvent("PersonalizationAPIController :: endpoint : api/personalizationapi/users/{userID}, action: create new user, response: success,userID: " + personalizationrequset.user.UserID);
                        //add user devices if provided in request
                        if (isDevicesProvided)
                        {
                            InsightLogger.TrackEvent("PersonalizationAPIController :: endpoint : api/personalizationapi/users/{userID}, action: devices provided, response: true");                            
                            userdevice.AddDevices(userprovideddevices.ToList());
                            InsightLogger.TrackEvent("PersonalizationAPIController :: endpoint : api/personalizationapi/users/{userID}, action: create and/or associate devices to user, response: success");
                        }
                        //associate user backends if provided in request other wise associate all backends in system
                        if (isBackendsProvided)
                        {
                            InsightLogger.TrackEvent("PersonalizationAPIController :: endpoint : api/personalizationapi/users/{userID}, action: backends provided, response: true");
                            userbackend.AddBackends(userprovidedbackends.ToList());
                            InsightLogger.TrackEvent("PersonalizationAPIController :: endpoint : api/personalizationapi/users/{userID}, action: create and/or associate backends to user, response: success");
                        }
                        else
                        {
                            InsightLogger.TrackEvent("PersonalizationAPIController :: endpoint : api/personalizationapi/users/{userID}, action: backends provided, response: false");
                            userbackend.AddAllBackends(user.UserID);
                            InsightLogger.TrackEvent("PersonalizationAPIController :: endpoint : api/personalizationapi/users/{userID}, action: create and/or associate all backends to user, response: success");
                        }
                        updateduser = personalization.GetUser(personalizationrequset.user.UserID);                        
                        personalization.TriggerUserRequests(personalizationrequset.user.UserID,updateduser.userbackends);
                        InsightLogger.TrackEvent("PersonalizationAPIController :: endpoint : api/personalizationapi/users/{userID}, action: trigger a user requests update, response: success");                        
                        int SynchTime=personalization.CalcSynchTime(userprovidedbackends);
                        InsightLogger.TrackEvent("PersonalizationAPIController :: endpoint : api/personalizationapi/users/{userID}, action: calculate synch waiting time, response: success, synchWaitingtime:"+ SynchTime);
                    }
                    else
                    {
                        InsightLogger.TrackEvent("PersonalizationAPIController :: endpoint : api/personalizationapi/users/{userID}, action: check user, response: true");
                        personalization.UpdateUserProp(userentity);
                        //remove existing devices to user and add the provided userdevices in requset
                        if (isDevicesProvided)
                        {
                            InsightLogger.TrackEvent("PersonalizationAPIController ::  endpoint : api/personalizationapi/users/{userID}, action: devices provided, response: true");
                            userdevice.RemoveDevices(personalizationrequset.user.UserID);
                            InsightLogger.TrackEvent("PersonalizationAPIController ::  endpoint : api/personalizationapi/users/{userID}, action: remove all existing associated devices , response: success");                            
                            userdevice.AddDevices(userprovideddevices.ToList());
                            InsightLogger.TrackEvent("PersonalizationAPIController ::  endpoint : api/personalizationapi/users/{userID}, action: create associated devices to user, response: success");
                        }
                        //remove existing backends to user and add the provided userbackends in requset
                        if (isBackendsProvided)
                        {
                            InsightLogger.TrackEvent("PersonalizationAPIController ::  endpoint : api/personalizationapi/users/{userID}, action: backends provided, response: true");
                            userbackend.RemoveBackends(personalizationrequset.user.UserID);
                            InsightLogger.TrackEvent("PersonalizationAPIController ::  endpoint : api/personalizationapi/users/{userID}, action: remove all existing associated backends , response: success");
                            userbackend.AddBackends(userprovidedbackends.ToList());
                            InsightLogger.TrackEvent("PersonalizationAPIController ::  endpoint : api/personalizationapi/users/{userID}, action: create associated backends to user, response: success");
                        }
                        else
                        {
                            userbackend.RemoveBackends(personalizationrequset.user.UserID);
                        }
                        updateduser = personalization.GetUser(personalizationrequset.user.UserID);
                    }                    
                    var ResponseUser = new PersonalizationResponseDTO<UserDTO>();
                    ResponseUser.result = updateduser;
                    ResponseUser.query = personalizationrequset;
                    InsightLogger.TrackEvent("PersonalizationAPIController :: endpoint : api/personalizationapi/users/{userID}, action: populate user object to response, response: success, userid: "+ personalizationrequset.user.UserID);
                    return Request.CreateResponse(HttpStatusCode.OK, ResponseUser);
                }
                else
                {
                    InsightLogger.TrackEvent("PersonalizationAPIController ::endpoint : api/personalizationapi/users/{userID} , action : put user, response : userid is null");
                    return Request.CreateResponse(HttpStatusCode.BadRequest, DataProvider.PersonalizationResponseError<UserBackendDTO>("400", "Error in updating user", ""));
                }
            }
            catch (DataAccessException dalexception)
            {
                InsightLogger.Exception(dalexception.Message, dalexception, callerMethodName);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserDTO>("400", dalexception.Message, dalexception.Message));
            }
            catch (BusinessLogicException blexception)
            {
                InsightLogger.Exception(blexception.Message, blexception, callerMethodName);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserDTO>("400", blexception.Message, blexception.Message));
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
               //LoggerHelper.WriteToLog(exception + " - exception in controller action while inserting/updating user : "
               //       + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserDTO>("400", exception.Message, exception.StackTrace));
            }
        }

        /// <summary>
        /// action to get user entity
        /// </summary>
        /// <param name="userID">takes userid as input</param>
        /// <returns>returns user along with is associated devices and backends</returns>
        [Route("api/personalizationapi/users/{userID}")]
        public HttpResponseMessage GetUsers(string userID)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                InsightLogger.TrackEvent("PersonalizationAPIController :: endpoint : api/personalizationapi/users/{userID}, action : start get method");
                //check if userid is null
                if (!string.IsNullOrEmpty(userID))
                {
                    Personalization personalization = new Personalization();                    
                    UserDTO user = personalization.GetUser(userID);                    
                    //if user exists returns response otherwise not found
                    if (user != null)
                    {
                        InsightLogger.TrackEvent("PersonalizationAPIController :: endpoint : api/personalizationapi/users/{userID}, action : getting user with associated backends and devices, response: success, userID: " + userID);
                        var ResponseUsers = new PersonalizationResponseDTO<UserDTO>();
                        ResponseUsers.result = user;
                        return Request.CreateResponse(HttpStatusCode.OK, ResponseUsers);
                    }
                    else
                    {
                        InsightLogger.TrackEvent("PersonalizationAPIController :: endpoint : api/personalizationapi/users/{userID}, action : getting user with associated backends and devices, response: user not available, userID: "+userID);
                        return Request.CreateResponse(HttpStatusCode.OK, DataProvider.PersonalizationResponseError<UserBackendDTO>("400", "user does not exist", ""));
                    }
                }
                else                   
                {
                    InsightLogger.TrackEvent("PersonalizationAPIController :: endpoint : api/personalizationapi/users/{userID} , action: get user, response:userid is null");
                    return Request.CreateResponse(HttpStatusCode.BadRequest, DataProvider.PersonalizationResponseError<UserBackendDTO>("400", "user does not exist", ""));
                }
            }
            catch (DataAccessException dalexception)
            {
                InsightLogger.Exception(dalexception.Message, dalexception, callerMethodName);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserDTO>("400", dalexception.Message, dalexception.Message));
            }
            catch (BusinessLogicException blexception)
            {
                InsightLogger.Exception(blexception.Message, blexception, callerMethodName);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserDTO>("400", blexception.Message, blexception.Message));
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                //LoggerHelper.WriteToLog(exception + " - exception in controller action while retreiving user : "
                //      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserDTO>("400", exception.Message, exception.StackTrace));
            }
        }

        /// <summary>
        /// action to delete user
        /// </summary>
        /// <param name="userID">takes userid as input</param>
        /// <returns>returns succes or failure status code for user deletion</returns>
        [Route("api/personalizationapi/users/{userID}")]
        public HttpResponseMessage DeleteUsers(string userID)
        {
            string callerMethodName = string.Empty;
            try
            {
                InsightLogger.TrackEvent("PersonalizationAPIController :: Delete method, endpoint - api / personalizationapi / users /"+ userID);                
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                //check if userid is null
                if (!string.IsNullOrEmpty(userID))
                {
                    Personalization personalization = new Personalization();
                    InsightLogger.TrackEvent("PersonalizationAPIController :: Deleting user");
                    UserEntity deleteUserEntity = personalization.DeleteUser(userID);                    
                    //if user deleted returns ok otherwise not found
                    if (deleteUserEntity != null)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK);
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound);
                    }
                }
                else
                {
                    InsightLogger.TrackEvent("PersonalizationAPIController ::user does not exist to delete");
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
                }
            }
            catch (DataAccessException dalexception)
            {
                InsightLogger.Exception(dalexception.Message, dalexception, callerMethodName);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserDTO>("400", dalexception.Message, dalexception.Message));
            }
            catch (BusinessLogicException blexception)
            {
                InsightLogger.Exception(blexception.Message, blexception, callerMethodName);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserDTO>("400", blexception.Message, blexception.Message));
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                //LoggerHelper.WriteToLog(exception + " - exception in controller action while deleting user : "
                //      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserDTO>("400", exception.Message, exception.StackTrace));
            }

        }

        /// <summary>
        /// action to get userdevices
        /// </summary>
        /// <param name="userID">take userid as input</param>
        /// <returns>returns list of devices associated to user</returns>
        [Route("api/personalizationapi/users/{userID}/devices")]
        public HttpResponseMessage GetUserAllDevices(string userID)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                InsightLogger.TrackEvent("PersonalizationAPIController :: Get method endpoint - api/personalizationapi/users/{userID}/devices");

                //check if userid is null
                if (!string.IsNullOrEmpty(userID))
                {
                    UserDevice userdevices = new UserDevice();
                    InsightLogger.TrackEvent("PersonalizationAPIController :: Get all Associated devices  to user");
                    IEnumerable<UserDeviceDTO> alluserdevices = userdevices.GetUserAllDevices(userID);                   
                    //if user as devices associated returns response otherwise not found
                    if (alluserdevices != null)
                    {
                        //adding userdevice dto to responsedto
                        var ResponseUserDevices = new PersonalizationResponseListDTO<UserDeviceDTO>();
                        ResponseUserDevices.result = alluserdevices;
                        return Request.CreateResponse(HttpStatusCode.OK, ResponseUserDevices);
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, DataProvider.PersonalizationResponseError<UserBackendDTO>("400", "user does not have associated devices", ""));
                    }

                }
                else
                {
                    InsightLogger.TrackEvent("PersonalizationAPIController ::userid is null or empty");

                    return Request.CreateResponse(HttpStatusCode.BadRequest, DataProvider.PersonalizationResponseError<UserBackendDTO>("400", "user does not have associated devices", ""));
                }
            }
            catch (DataAccessException dalexception)
            {
                InsightLogger.Exception(dalexception.Message, dalexception, callerMethodName);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserDeviceDTO>("400", dalexception.Message, dalexception.Message));
            }
            catch (BusinessLogicException blexception)
            {
                InsightLogger.Exception(blexception.Message, blexception, callerMethodName);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserDeviceDTO>("400", blexception.Message, blexception.Message));
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                //LoggerHelper.WriteToLog(exception + " - exception in controller action while retreiving all userdevcies for user : "
                //      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserDeviceDTO>("400", exception.Message, exception.StackTrace));
            }
        }

        /// <summary>
        /// action to insert userdevices
        /// </summary>
        /// <param name="userdeviceentity">takes list of devcies to associate to user</param>
        /// <returns>returns status code as sucsses or failure for association of devices</returns>
        [Route("api/personalizationapi/users/{userID}/devices")]
        public HttpResponseMessage PostDevices(PersonalizationRequsetDTO personalizationrequset)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                InsightLogger.TrackEvent("PersonalizationAPIController :: post method endpoint - api/personalizationapi/users/{userID}/devices");
                UserDevice userdevice = new UserDevice();
                //if user devcies provided in requset
                if (personalizationrequset.userdevices != null)
                {
                    IEnumerable<UserDeviceDTO> userdevicesdto = personalizationrequset.userdevices;              
                    IEnumerable<UserDeviceEntity> userprovideddevices = userdevice.UserDeviceEntityGenerator(userdevicesdto);
                    InsightLogger.TrackEvent("PersonalizationAPIController :: Associate devices  to user");
                    userdevice.AddDevices(userprovideddevices.ToList());                    
                    return Request.CreateResponse(HttpStatusCode.OK);
                }
                else
                {
                    InsightLogger.TrackEvent("PersonalizationAPIController ::userdevices not provided to associate");

                    return Request.CreateResponse(HttpStatusCode.BadRequest);
                }
            }
            catch (DataAccessException dalexception)
            {
                InsightLogger.Exception(dalexception.Message, dalexception, callerMethodName);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserDeviceDTO>("400", dalexception.Message, dalexception.Message));
            }
            catch (BusinessLogicException blexception)
            {
                InsightLogger.Exception(blexception.Message, blexception, callerMethodName);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserDeviceDTO>("400", blexception.Message, blexception.Message));
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                //LoggerHelper.WriteToLog(exception + " - exception in controller action while inserting userdevcie : "
                //      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserDeviceDTO>("400", exception.Message, exception.StackTrace));
            }
        }

        /// <summary>
        /// action to get single userdevice
        /// </summary>
        /// <param name="userID">takes userid as input</param>
        /// <param name="userDeviceID">takes user device id as input</param>
        /// <returns> returns device with given id associate to user</returns>
        [Route("api/personalizationapi/users/{userID}/devices/{userDeviceID}")]
        public HttpResponseMessage GetUserDevice(string userID, string userDeviceID)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                InsightLogger.TrackEvent("PersonalizationAPIController :: Get method, endpoint - api/personalizationapi/users/{userID}/devices/{userDeviceID}");
                //checks userid and userdeviceis for null
                if (!(string.IsNullOrEmpty(userID) || string.IsNullOrEmpty(userDeviceID)))
                {
                    UserDevice userdevice = new UserDevice();
                    InsightLogger.TrackEvent("PersonalizationAPIController :: Get specific Associated device  to user");
                    PersonalizationResponseDTO<UserDeviceDTO> ResponseUserDevice = userdevice.GetUserDevice(userID, userDeviceID);
                    //if user has associated device return response otherwise not found                   
                    if (ResponseUserDevice.result != null)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, ResponseUserDevice);
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, DataProvider.PersonalizationResponseError<UserDeviceDTO>("400", "user does not have associated device", ""));
                    }

                }
                else
                {
                    InsightLogger.TrackEvent("PersonalizationAPIController ::userid or userdeviceid is null");
                    return Request.CreateResponse(HttpStatusCode.BadRequest, DataProvider.PersonalizationResponseError<UserDeviceDTO>("400", "userid or deviceid can't be empty or null ", ""));
                }
            }
            catch (DataAccessException dalexception)
            {
                InsightLogger.Exception(dalexception.Message, dalexception, callerMethodName);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserDeviceDTO>("400", dalexception.Message, dalexception.Message));
            }
            catch (BusinessLogicException blexception)
            {
                InsightLogger.Exception(blexception.Message, blexception, callerMethodName);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserDeviceDTO>("400", blexception.Message, blexception.Message));
            }
            catch (Exception exception)
            {
                //LoggerHelper.WriteToLog(exception + " - exception in controller action while retreving single userdevcie with deviceID : "
                //      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserDeviceDTO>("400", exception.Message, exception.StackTrace));
            }
        }

        /// <summary>
        /// action to delete single userdevice
        /// </summary>
        /// <param name="userID">takes userid as input</param>
        /// <param name="userDeviceID">takes user device id as input</param>
        /// <returns>returns status code for deletion of user device</returns>
        [Route("api/personalizationapi/users/{userID}/devices/{userDeviceID}")]
        public HttpResponseMessage DeleteUserDevice(string userID, string userDeviceID)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                InsightLogger.TrackEvent("PersonalizationAPIController :: delete method, endpoint - api/personalizationapi/users/{userID}/devices/{userDeviceID}");
                //checks userid and userdeviceis for null
                if (!(string.IsNullOrEmpty(userID) || string.IsNullOrEmpty(userDeviceID)))
                {
                    UserDevice userdevice = new UserDevice();
                    InsightLogger.TrackEvent("PersonalizationAPIController :: delete specific Associated device to user");
                    UserDeviceEntity deleteUserDeviceEntity = userdevice.DeleteUserDevice(userID, userDeviceID);
                    //if user has associated device deleted return ok otherwise not found                    
                    if (deleteUserDeviceEntity != null)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK);
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound);
                    }

                }
                else
                {
                    InsightLogger.TrackEvent("PersonalizationAPIController ::userid or userdeviceid is null");
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
                }
            }
            catch (DataAccessException dalexception)
            {
                InsightLogger.Exception(dalexception.Message, dalexception, callerMethodName);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserDeviceDTO>("400", dalexception.Message, dalexception.Message));
            }
            catch (BusinessLogicException blexception)
            {
                InsightLogger.Exception(blexception.Message, blexception, callerMethodName);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserDeviceDTO>("400", blexception.Message, blexception.Message));
            }
            catch (Exception exception)
            {
                //LoggerHelper.WriteToLog(exception + " - exception in controller action while deleting single userdevcie with deviceID : "
                //      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserDeviceDTO>("400", exception.Message, exception.StackTrace));
            }
        }

        /// <summary>
        /// action to get userbackends
        /// </summary>
        /// <param name="userID">takes userid as input</param>
        /// <returns>returns list of backends</returns>
        [Route("api/personalizationapi/users/{userID}/backends")]
        public HttpResponseMessage GetUserAllBackends(string userID)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                InsightLogger.TrackEvent("PersonalizationAPIController ::Get method, endpoint - api/personalizationapi/users/{userID}/backends");
                //check userid for null
                if (!string.IsNullOrEmpty(userID))
                {
                    UserBackend userdevices = new UserBackend();
                    InsightLogger.TrackEvent("PersonalizationAPIController :: Get all Associated backends  to user");
                    IEnumerable<UserBackendDTO> alluserbackends = userdevices.GetUserAllBackends(userID);
                    //if user has associated backends return response else not found                   
                    if (alluserbackends != null)
                    {
                        //converting userbackendsentity to Responsedto
                        var ResponseUserBackends = new PersonalizationResponseListDTO<UserBackendDTO>();
                        ResponseUserBackends.result = alluserbackends;
                        return Request.CreateResponse(HttpStatusCode.OK, ResponseUserBackends);
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, DataProvider.PersonalizationResponseError<UserBackendDTO>("400", "user does not have associated backends", ""));
                    }
                }
                else
                {
                    InsightLogger.TrackEvent("PersonalizationAPIController :: userid is null");
                    return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserBackendDTO>("400", "user does not exists", ""));
                }
            }
            catch (DataAccessException dalexception)
            {
                InsightLogger.Exception(dalexception.Message, dalexception, callerMethodName);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserBackendDTO>("400", dalexception.Message, dalexception.Message));
            }
            catch (BusinessLogicException blexception)
            {
                InsightLogger.Exception(blexception.Message, blexception, callerMethodName);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserBackendDTO>("400", blexception.Message, blexception.Message));
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                //LoggerHelper.WriteToLog(exception + " - exception in controller action while retreiving all userbackends : "
                //      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserBackendDTO>("400", exception.Message, exception.StackTrace));
            }
        }

        /// <summary>
        /// action to insert userbackends
        /// </summary>
        /// <param name="userBackendentity">takes list of backends to associate to user</param>
        /// <returns>returns status code for backend association to user</returns>
        [Route("api/personalizationapi/users/{userID}/backends")]
        public HttpResponseMessage PostBackends(PersonalizationRequsetDTO personalizationrequset)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                InsightLogger.TrackEvent("PersonalizationAPIController :: post backends method, endpoint - api/personalizationapi/users/{userID}/backends");
                //check request for userbackend deatils
                if (personalizationrequset.userbackends != null)
                {
                    UserBackend userbackend = new UserBackend();
                    
                    IEnumerable<UserBackendDTO> userbackendsdto = personalizationrequset.userbackends;
                    IEnumerable<UserBackendEntity> userprovidedbackends = userbackend.UserBackendEntityGenerator(userbackendsdto);
                    InsightLogger.TrackEvent("PersonalizationAPIController :: associate backends to user");
                    userbackend.AddBackends(userprovidedbackends.ToList());                   
                    return Request.CreateResponse(HttpStatusCode.OK);
                }
                else
                {
                    InsightLogger.TrackEvent("PersonalizationAPIController :: backends not provided");
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
                }
            }
            catch (DataAccessException dalexception)
            {
                InsightLogger.Exception(dalexception.Message, dalexception, callerMethodName);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserBackendDTO>("400", dalexception.Message, dalexception.Message));
            }
            catch (BusinessLogicException blexception)
            {
                InsightLogger.Exception(blexception.Message, blexception, callerMethodName);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserBackendDTO>("400", blexception.Message, blexception.Message));
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                //LoggerHelper.WriteToLog(exception + " - exception in controller action while inserting single userbackend : "
                //      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserBackendDTO>("400", exception.Message, exception.StackTrace));
            }
        }

        /// <summary>
        /// method to get single userbackend
        /// </summary>
        /// <param name="userID">takes userid as input</param>
        /// <param name="userBackendID">takes user backend id as input</param>
        /// <returns>returns user backend with given backend id associated to user</returns>
        [Route("api/personalizationapi/users/{userID}/backends/{userBackendID}")]
        public HttpResponseMessage GetUserBackend(string userID, string userBackendID)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                InsightLogger.TrackEvent("PersonalizationAPIController :: Get backend method, api/personalizationapi/users/{userID}/backends/{userBackendID}");
                //check userid and userbackendid for null
                if (!(string.IsNullOrEmpty(userID) || string.IsNullOrEmpty(userBackendID)))
                {
                    UserBackend userbackend = new UserBackend();
                    PersonalizationResponseDTO<UserBackendDTO> ResponseUserBackend = userbackend.GetUserBackend(userID, userBackendID);                   
                    //if user backend avialable return response else not found
                    if (ResponseUserBackend.result != null)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, ResponseUserBackend);
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, DataProvider.PersonalizationResponseError<UserBackendDTO>("400", "user does not have associated backends", ""));
                }
                else
                {
                    InsightLogger.TrackEvent("PersonalizationAPIController :: userid or userbackendid is null");

                    return Request.CreateResponse(HttpStatusCode.BadRequest, DataProvider.PersonalizationResponseError<UserBackendDTO>("400", "userid or associated deviceid empty or null", ""));
                }
            }
            catch (DataAccessException dalexception)
            {
                InsightLogger.Exception(dalexception.Message, dalexception, callerMethodName);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserBackendDTO>("400", dalexception.Message, dalexception.Message));
            }
            catch (BusinessLogicException blexception)
            {
                InsightLogger.Exception(blexception.Message, blexception, callerMethodName);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserBackendDTO>("400", blexception.Message, blexception.Message));
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                //LoggerHelper.WriteToLog(exception + " - exception in controller action while retreving single userbackend : "
                //      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserBackendDTO>("400", exception.Message, exception.StackTrace));
            }
        }

        /// <summary>
        /// method to delete single userbackend
        /// </summary>
        /// <param name="userID">takes userid as input</param>
        /// <param name="userBackendID">takes user backend id as input</param>
        /// <returns>returns status code for user backend deletion</returns>        
        [Route("api/personalizationapi/users/{userID}/backends/{userBackendID}")]
        public HttpResponseMessage DeleteUserBackend(string userID, string userBackendID)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                InsightLogger.TrackEvent("PersonalizationAPIController :: delete backend method, endpoint - api/personalizationapi/users/{userID}/backends/{userBackendID}");
                //check userid and userbackendid for null
                if (!(string.IsNullOrEmpty(userID) || string.IsNullOrEmpty(userBackendID)))
                {
                    UserBackend userbackend = new UserBackend();
                    InsightLogger.TrackEvent("PersonalizationAPIController :: delete specific backend  to user");
                    UserBackendEntity deleteUserBackendEntity = userbackend.DeleteUserBackend(userID, userBackendID);                   
                    // if user deleted return ok otherwise not found
                    if (deleteUserBackendEntity != null)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK);
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound);
                    }

                }
                else
                {
                    InsightLogger.TrackEvent("PersonalizationAPIController :: userid or userbackendid is null");
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
                }
            }
            catch (DataAccessException dalexception)
            {
                InsightLogger.Exception(dalexception.Message, dalexception, callerMethodName);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserBackendDTO>("400", dalexception.Message, dalexception.Message));
            }
            catch (BusinessLogicException blexception)
            {
                InsightLogger.Exception(blexception.Message, blexception, callerMethodName);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserBackendDTO>("400", blexception.Message, blexception.Message));
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                //LoggerHelper.WriteToLog(exception + " - exception in controller action while inserting single userbackend : "
                //      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserBackendDTO>("400", exception.Message, exception.StackTrace));
            }

        }

    }
}
