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
    public class PersonalizationAPIController : ApiController
    {
        /// <summary>
        /// action method to get all backends
        /// </summary>
        /// <returns></returns>        
        [Route("api/personalizationapi/backends")]
        public HttpResponseMessage GetBackends()
        {
            try
            {
                UserBackend userBackend = new UserBackend();
                PersonalizationResponseListDTO<BackendDTO> allBackends = userBackend.GetBackends();
                if (allBackends.result != null)
                {                    
                    return Request.CreateResponse(HttpStatusCode.OK, allBackends);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserBackendDTO>("400", "User does not have backends", ""));
                }
            }
            catch (DataAccessException DALexception)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserBackendDTO>("400", DALexception.Message, DALexception.Message));
            }
            catch (BusinessLogicException BLexception)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserBackendDTO>("400", BLexception.Message, BLexception.Message));
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - exception in controller action while retrieving all backends : "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserBackendDTO>("400", exception.Message, exception.StackTrace));
            }
        }

        /// <summary>
        /// action to insert or update user
        /// </summary>
        /// <param name="user"></param>
        /// <param name="userprovideddevices"></param>
        /// <param name="userprovidedbackends"></param>
        /// <param name="maxsyncreplysize"></param>
        /// <returns></returns>        
        [Route("api/personalizationapi/users/{userID}")]
        public HttpResponseMessage PutUsers(PersonalizationRequsetDTO personalizationrequset)
        {
            try
            {
                //BL object instances created
                Personalization personalization = new Personalization();
                UserBackend userbackend = new UserBackend();
                UserDevice userdevice = new UserDevice();
                if (!string.IsNullOrEmpty(personalizationrequset.user.UserID))
                {
                    Boolean isUserExists = personalization.CheckUser(personalizationrequset.user.UserID);
                    Boolean isDevicesProvided, isBackendsProvided;

                    //retracking individual objects from request
                    IEnumerable<UserDeviceDTO> userdevicesdto = personalizationrequset.user.userdevices;
                    IEnumerable<UserBackendDTO> userbackendsdto = personalizationrequset.user.userbackends;
                    UserDTO user = personalizationrequset.user;
                    UserEntity userentity = personalization.UserEntityGenerator(user);
                    IEnumerable<UserDeviceEntity> userprovideddevices = userdevice.UserDeviceEntityGenerator(userdevicesdto);
                    IEnumerable<UserBackendEntity> userprovidedbackends = userbackend.UserBackendEntityGenerator(userbackendsdto);

                    //to check if requset has userdevices or not
                    if (userprovideddevices != null)
                    {
                        isDevicesProvided = true;
                    }
                    else { isDevicesProvided = true; }

                    //to check if requset has userbackends or not
                    if (userprovidedbackends != null)
                    {
                        isBackendsProvided = true;
                    }
                    else { isBackendsProvided = true; }

                    //create if user not exists else update
                    if (!isUserExists)
                    {
                        personalization.CreateUser(userentity);

                        //add user devices if provided in request
                        if (isDevicesProvided)
                        {
                            userdevice.AddDevices(userprovideddevices.ToList());
                        }

                        //add user backends if provided in request
                        if (isBackendsProvided)
                        {
                            userbackend.AddBackends(userprovidedbackends.ToList());
                        }

                        personalization.TriggerUserRequests(personalizationrequset.user.UserID);
                        personalization.CalcSynchTime(userprovidedbackends);
                    }
                    else
                    {
                        personalization.UpdateUserProp(userentity);
                        //remove existing devices to user and add the provided userdevices in requset
                        if (isDevicesProvided)
                        {
                            userdevice.RemoveDevices(personalizationrequset.user.UserID);
                            userdevice.AddDevices(userprovideddevices.ToList());
                        }
                        //remove existing backends to user and add the provided userbackends in requset
                        if (isBackendsProvided)
                        {
                            userbackend.RemoveBackends(personalizationrequset.user.UserID);
                            userbackend.AddBackends(userprovidedbackends.ToList());
                        }
                    }

                    UserDTO updateduser = personalization.GetUser(personalizationrequset.user.UserID);
                    var ResponseUser = new PersonalizationResponseDTO<UserDTO>();
                    ResponseUser.result = updateduser;
                    ResponseUser.query = personalizationrequset;
                    return Request.CreateResponse(HttpStatusCode.OK, ResponseUser);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, DataProvider.PersonalizationResponseError<UserBackendDTO>("400", "Error in updating user", ""));
                }
            }
            catch (DataAccessException DALexception)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserDTO>("400", DALexception.Message, DALexception.Message));
            }
            catch (BusinessLogicException BLexception)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserDTO>("400", BLexception.Message, BLexception.Message));
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - exception in controller action while inserting/updating user : "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserDTO>("400", exception.Message, exception.StackTrace));
            }
        }

        /// <summary>
        /// action to get user entity
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        [Route("api/personalizationapi/users/{userID}")]
        public HttpResponseMessage GetUsers(String userID)
        {
            try
            {
                if (!string.IsNullOrEmpty(userID))
                {
                    Personalization personalization = new Personalization();
                    UserDTO user = personalization.GetUser(userID);                  
                    
                    if (user != null)
                    {
                        var ResponseUsers = new PersonalizationResponseDTO<UserDTO>();
                        ResponseUsers.result = user;
                        return Request.CreateResponse(HttpStatusCode.OK, ResponseUsers);
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserBackendDTO>("400", "user does not exist", ""));
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, DataProvider.PersonalizationResponseError<UserBackendDTO>("400", "user does not exist", ""));
                }
            }
            catch (DataAccessException DALexception)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserDTO>("400", DALexception.Message, DALexception.Message));
            }
            catch (BusinessLogicException BLexception)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserDTO>("400", BLexception.Message, BLexception.Message));
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - exception in controller action while retreiving user : "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserDTO>("400", exception.Message, exception.StackTrace));
            }
        }

        /// <summary>
        /// action to delete user
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        [Route("api/personalizationapi/users/{userID}")]
        public HttpResponseMessage DeleteUsers(String userID)
        {
            try
            {
                if (!string.IsNullOrEmpty(userID))
                {
                    Personalization personalization = new Personalization();
                    UserEntity deleteUserEntity = personalization.DeleteUser(userID);

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
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
                }
            }
            catch (DataAccessException DALexception)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserDTO>("400", DALexception.Message, DALexception.Message));
            }
            catch (BusinessLogicException BLexception)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserDTO>("400", BLexception.Message, BLexception.Message));
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - exception in controller action while deleting user : "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserDTO>("400", exception.Message, exception.StackTrace));
            }

        }

        /// <summary>
        /// action to get userdevices
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        [Route("api/personalizationapi/users/{userID}/devices")]
        public HttpResponseMessage GetUserAllDevices(String userID)
        {
            try
            {
                if (!string.IsNullOrEmpty(userID))
                {
                    UserDevice userdevices = new UserDevice();
                    IEnumerable<UserDeviceDTO> alluserdevices = userdevices.GetUserAllDevices(userID);
                    if (alluserdevices != null)
                    {
                        //adding userdevice dto to responsedto
                        var ResponseUserDevices = new PersonalizationResponseListDTO<UserDeviceDTO>();
                        ResponseUserDevices.result = alluserdevices;
                        return Request.CreateResponse(HttpStatusCode.OK, ResponseUserDevices);
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserBackendDTO>("400", "user does not have associated devices", ""));
                    }
                    
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, DataProvider.PersonalizationResponseError<UserBackendDTO>("400", "user does not have associated devices", ""));
                }
            }
            catch (DataAccessException DALexception)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserDeviceDTO>("400", DALexception.Message, DALexception.Message));
            }
            catch (BusinessLogicException BLexception)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserDeviceDTO>("400", BLexception.Message, BLexception.Message));
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - exception in controller action while retreiving all userdevcies for user : "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserDeviceDTO>("400", exception.Message, exception.StackTrace));
            }
        }

        /// <summary>
        /// action to insert userdevices
        /// </summary>
        /// <param name="userdeviceentity"></param>
        /// <returns></returns>
        [Route("api/personalizationapi/users/{userID}/devices")]
        public HttpResponseMessage PostDevices(PersonalizationRequsetDTO personalizationrequset)
        {
            try
            {
                
                UserDevice userdevice = new UserDevice();
                if(personalizationrequset.userdevices!=null)
                {
                    userdevice.PostDevices(personalizationrequset);
                    return Request.CreateResponse(HttpStatusCode.OK);
                }
               else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
                }
            }
            catch (DataAccessException DALexception)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserDeviceDTO>("400", DALexception.Message, DALexception.Message));
            }
            catch (BusinessLogicException BLexception)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserDeviceDTO>("400", BLexception.Message, BLexception.Message));
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - exception in controller action while inserting userdevcie : "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserDeviceDTO>("400", exception.Message, exception.StackTrace));
            }
        }

        /// <summary>
        /// action to get single userdevice
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="userDeviceID"></param>
        /// <returns></returns>
        [Route("api/personalizationapi/users/{userID}/devices/{userDeviceID}")]
        public HttpResponseMessage GetUserDevice(String userID, String userDeviceID)
        {
            try
            {
                if (!(string.IsNullOrEmpty(userID) || string.IsNullOrEmpty(userDeviceID)))
                {
                    UserDevice userdevice = new UserDevice();
                    PersonalizationResponseDTO<UserDeviceDTO> ResponseUserDevice = userdevice.GetUserDevice(userID, userDeviceID);
                    if (ResponseUserDevice.result != null)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, ResponseUserDevice);
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserDeviceDTO>("400", "user does not have associated device", ""));
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, DataProvider.PersonalizationResponseError<UserDeviceDTO>("400", "userid or deviceid can't be empty or null ", ""));
                }
            }
            catch (DataAccessException DALexception)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserDeviceDTO>("400", DALexception.Message, DALexception.Message));
            }
            catch (BusinessLogicException BLexception)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserDeviceDTO>("400", BLexception.Message, BLexception.Message));
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - exception in controller action while retreving single userdevcie with deviceID : "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserDeviceDTO>("400", exception.Message, exception.StackTrace));
            }
        }

        /// <summary>
        /// action to delete single userdevice
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="userDeviceID"></param>
        /// <returns></returns>
        [Route("api/personalizationapi/users/{userID}/devices/{userDeviceID}")]
        public HttpResponseMessage DeleteUserDevice(String userID, String userDeviceID)
        {
            try
            {
                if (!(string.IsNullOrEmpty(userID) || string.IsNullOrEmpty(userDeviceID)))
                {
                    UserDevice userdevice = new UserDevice();
                    UserDeviceEntity deleteUserDeviceEntity = userdevice.DeleteUserDevice(userID, userDeviceID);

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
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
                }
            }
            catch (DataAccessException DALexception)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserDeviceDTO>("400", DALexception.Message, DALexception.Message));
            }
            catch (BusinessLogicException BLexception)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserDeviceDTO>("400", BLexception.Message, BLexception.Message));
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - exception in controller action while deleting single userdevcie with deviceID : "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserDeviceDTO>("400", exception.Message, exception.StackTrace));
            }
        }

        /// <summary>
        /// action to get userbackends
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        [Route("api/personalizationapi/users/{userID}/backends")]
        public HttpResponseMessage GetUserAllBackends(String userID)
        {
            try
            {
                if (!string.IsNullOrEmpty(userID))
                {
                    UserBackend userdevices = new UserBackend();
                    IEnumerable<UserBackendDTO> alluserbackends = userdevices.GetUserAllBackends(userID);
                    if (alluserbackends != null)
                    {
                        //converting userbackendsentity to Responsedto
                        var ResponseUserBackends = new PersonalizationResponseListDTO<UserBackendDTO>();
                        ResponseUserBackends.result = alluserbackends;
                        return Request.CreateResponse(HttpStatusCode.OK, ResponseUserBackends);
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserBackendDTO>("400", "user does not have associated backends", ""));
                    }                    
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserBackendDTO>("400", "user does not exists", ""));
                }
            }
            catch (DataAccessException DALexception)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserBackendDTO>("400", DALexception.Message, DALexception.Message));
            }
            catch (BusinessLogicException BLexception)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserBackendDTO>("400", BLexception.Message, BLexception.Message));
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - exception in controller action while retreiving all userbackends : "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserBackendDTO>("400", exception.Message, exception.StackTrace));
            }
        }

        /// <summary>
        /// action to insert userbackends
        /// </summary>
        /// <param name="userBackendentity"></param>
        /// <returns></returns>
        [Route("api/personalizationapi/users/{userID}/backends")]
        public HttpResponseMessage PostBackends(PersonalizationRequsetDTO personalizationrequset)
        {
            try
            {
                if (personalizationrequset.userbackends != null)
                {
                    UserBackend userbackend = new UserBackend();
                    userbackend.PostBackends(personalizationrequset);
                    return Request.CreateResponse(HttpStatusCode.OK);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
                }                
            }
            catch (DataAccessException DALexception)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserBackendDTO>("400", DALexception.Message, DALexception.Message));
            }
            catch (BusinessLogicException BLexception)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserBackendDTO>("400", BLexception.Message, BLexception.Message));
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - exception in controller action while inserting single userbackend : "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserBackendDTO>("400", exception.Message, exception.StackTrace));
            }
        }

        /// <summary>
        /// method to get single userbackend
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="userBackendID"></param>
        /// <returns></returns>
        [Route("api/personalizationapi/users/{userID}/backends/{userBackendID}")]
        public HttpResponseMessage GetUserBackend(String userID, String userBackendID)
        {
            try
            {
                if (!(string.IsNullOrEmpty(userID) || string.IsNullOrEmpty(userBackendID)))
                {
                    UserBackend userbackend = new UserBackend();
                    PersonalizationResponseDTO<UserBackendDTO> ResponseUserBackend = userbackend.GetUserBackend(userID, userBackendID);                    
                    
                    if (ResponseUserBackend.result != null)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, ResponseUserBackend);
                    }
                    return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserBackendDTO>("400", "user does not have associated backends", ""));
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, DataProvider.PersonalizationResponseError<UserBackendDTO>("400", "userid or associated deviceid empty or null", ""));
                }
            }
            catch (DataAccessException DALexception)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserBackendDTO>("400", DALexception.Message, DALexception.Message));
            }
            catch (BusinessLogicException BLexception)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserBackendDTO>("400", BLexception.Message, BLexception.Message));
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - exception in controller action while retreving single userbackend : "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserBackendDTO>("400", exception.Message, exception.StackTrace));
            }
        }

        /// <summary>
        /// method to delete single userbackend
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="userBackendID"></param>
        /// <returns></returns>        
        [Route("api/personalizationapi/users/{userID}/backends/{userBackendID}")]
        public HttpResponseMessage DeleteUserBackend(String userID, String userBackendID)
        {
            try
            {
                if (!(string.IsNullOrEmpty(userID) || string.IsNullOrEmpty(userBackendID)))
                {
                    UserBackend userbackend = new UserBackend();
                    UserBackendEntity deleteUserBackendEntity = userbackend.DeleteUserBackend(userID, userBackendID);

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
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
                }
            }
            catch (DataAccessException DALexception)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserBackendDTO>("400", DALexception.Message, DALexception.Message));
            }
            catch (BusinessLogicException BLexception)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserBackendDTO>("400", BLexception.Message, BLexception.Message));
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - exception in controller action while inserting single userbackend : "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserBackendDTO>("400", exception.Message, exception.StackTrace));
            }

        }

    }
}
