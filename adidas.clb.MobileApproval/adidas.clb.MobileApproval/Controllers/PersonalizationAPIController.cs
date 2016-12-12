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
    //[Authorize]
    public class PersonalizationAPIController : ApiController
    {
        /// <summary>
        /// action method to get all backends
        /// </summary>
        /// <returns>Returns list of backends</returns>        
        [Route("api/personalizationapi/backends")]
        public HttpResponseMessage GetBackends()
        {
            try
            {
                UserBackend userBackend = new UserBackend();
                PersonalizationResponseListDTO<BackendDTO> allBackends = userBackend.GetBackends();
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
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserBackendDTO>("400", dalexception.Message, dalexception.Message));
            }
            catch (BusinessLogicException blexception)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserBackendDTO>("400", blexception.Message, blexception.Message));
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
        /// <param name="personalizationrequset"></param>       
        /// <returns>returns user along with is associated devices and backends</returns>        
        [Route("api/personalizationapi/users/{userID}")]
        public HttpResponseMessage PutUsers(PersonalizationRequsetDTO personalizationrequset)
        {
            try
            {
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
                        personalization.CreateUser(userentity);

                        //add user devices if provided in request
                        if (isDevicesProvided)
                        {
                            userdevice.AddDevices(userprovideddevices.ToList());
                        }

                        //associate user backends if provided in request other wise associate all backends in system
                        if (isBackendsProvided)
                        {
                            userbackend.AddBackends(userprovidedbackends.ToList());
                        }
                        else
                        {
                            userbackend.AddAllBackends(user.UserID);
                        }
                        updateduser = personalization.GetUser(personalizationrequset.user.UserID);
                        personalization.TriggerUserRequests(personalizationrequset.user.UserID,updateduser.userbackends);
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
                        else
                        {
                            userbackend.RemoveBackends(personalizationrequset.user.UserID);
                        }
                        updateduser = personalization.GetUser(personalizationrequset.user.UserID);
                    }                    
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
            catch (DataAccessException dalexception)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserDTO>("400", dalexception.Message, dalexception.Message));
            }
            catch (BusinessLogicException blexception)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserDTO>("400", blexception.Message, blexception.Message));
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
        /// <param name="userID">takes userid as input</param>
        /// <returns>returns user along with is associated devices and backends</returns>
        [Route("api/personalizationapi/users/{userID}")]
        public HttpResponseMessage GetUsers(string userID)
        {
            try
            {
                //check if userid is null
                if (!string.IsNullOrEmpty(userID))
                {
                    Personalization personalization = new Personalization();
                    UserDTO user = personalization.GetUser(userID);
                    //if user exists returns response otherwise not found
                    if (user != null)
                    {
                        var ResponseUsers = new PersonalizationResponseDTO<UserDTO>();
                        ResponseUsers.result = user;
                        return Request.CreateResponse(HttpStatusCode.OK, ResponseUsers);
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, DataProvider.PersonalizationResponseError<UserBackendDTO>("400", "user does not exist", ""));
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, DataProvider.PersonalizationResponseError<UserBackendDTO>("400", "user does not exist", ""));
                }
            }
            catch (DataAccessException dalexception)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserDTO>("400", dalexception.Message, dalexception.Message));
            }
            catch (BusinessLogicException blexception)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserDTO>("400", blexception.Message, blexception.Message));
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
        /// <param name="userID">takes userid as input</param>
        /// <returns>returns succes or failure status code for user deletion</returns>
        [Route("api/personalizationapi/users/{userID}")]
        public HttpResponseMessage DeleteUsers(string userID)
        {
            try
            {
                //check if userid is null
                if (!string.IsNullOrEmpty(userID))
                {
                    Personalization personalization = new Personalization();
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
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
                }
            }
            catch (DataAccessException dalexception)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserDTO>("400", dalexception.Message, dalexception.Message));
            }
            catch (BusinessLogicException blexception)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserDTO>("400", blexception.Message, blexception.Message));
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
        /// <param name="userID">take userid as input</param>
        /// <returns>returns list of devices associated to user</returns>
        [Route("api/personalizationapi/users/{userID}/devices")]
        public HttpResponseMessage GetUserAllDevices(string userID)
        {
            try
            {
                //check if userid is null
                if (!string.IsNullOrEmpty(userID))
                {
                    UserDevice userdevices = new UserDevice();
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
                    return Request.CreateResponse(HttpStatusCode.BadRequest, DataProvider.PersonalizationResponseError<UserBackendDTO>("400", "user does not have associated devices", ""));
                }
            }
            catch (DataAccessException dalexception)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserDeviceDTO>("400", dalexception.Message, dalexception.Message));
            }
            catch (BusinessLogicException blexception)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserDeviceDTO>("400", blexception.Message, blexception.Message));
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
        /// <param name="userdeviceentity">takes list of devcies to associate to user</param>
        /// <returns>returns status code as sucsses or failure for association of devices</returns>
        [Route("api/personalizationapi/users/{userID}/devices")]
        public HttpResponseMessage PostDevices(PersonalizationRequsetDTO personalizationrequset)
        {
            try
            {

                UserDevice userdevice = new UserDevice();
                //if user devcies provided in requset
                if (personalizationrequset.userdevices != null)
                {
                    IEnumerable<UserDeviceDTO> userdevicesdto = personalizationrequset.userdevices;              
                    IEnumerable<UserDeviceEntity> userprovideddevices = userdevice.UserDeviceEntityGenerator(userdevicesdto);                  
                    userdevice.AddDevices(userprovideddevices.ToList());
                    return Request.CreateResponse(HttpStatusCode.OK);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
                }
            }
            catch (DataAccessException dalexception)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserDeviceDTO>("400", dalexception.Message, dalexception.Message));
            }
            catch (BusinessLogicException blexception)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserDeviceDTO>("400", blexception.Message, blexception.Message));
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
        /// <param name="userID">takes userid as input</param>
        /// <param name="userDeviceID">takes user device id as input</param>
        /// <returns> returns device with given id associate to user</returns>
        [Route("api/personalizationapi/users/{userID}/devices/{userDeviceID}")]
        public HttpResponseMessage GetUserDevice(string userID, string userDeviceID)
        {
            try
            {
                //checks userid and userdeviceis for null
                if (!(string.IsNullOrEmpty(userID) || string.IsNullOrEmpty(userDeviceID)))
                {
                    UserDevice userdevice = new UserDevice();
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
                    return Request.CreateResponse(HttpStatusCode.BadRequest, DataProvider.PersonalizationResponseError<UserDeviceDTO>("400", "userid or deviceid can't be empty or null ", ""));
                }
            }
            catch (DataAccessException dalexception)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserDeviceDTO>("400", dalexception.Message, dalexception.Message));
            }
            catch (BusinessLogicException blexception)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserDeviceDTO>("400", blexception.Message, blexception.Message));
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
        /// <param name="userID">takes userid as input</param>
        /// <param name="userDeviceID">takes user device id as input</param>
        /// <returns>returns status code for deletion of user device</returns>
        [Route("api/personalizationapi/users/{userID}/devices/{userDeviceID}")]
        public HttpResponseMessage DeleteUserDevice(string userID, string userDeviceID)
        {
            try
            {
                //checks userid and userdeviceis for null
                if (!(string.IsNullOrEmpty(userID) || string.IsNullOrEmpty(userDeviceID)))
                {
                    UserDevice userdevice = new UserDevice();
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
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
                }
            }
            catch (DataAccessException dalexception)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserDeviceDTO>("400", dalexception.Message, dalexception.Message));
            }
            catch (BusinessLogicException blexception)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserDeviceDTO>("400", blexception.Message, blexception.Message));
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
        /// <param name="userID">takes userid as input</param>
        /// <returns>returns list of backends</returns>
        [Route("api/personalizationapi/users/{userID}/backends")]
        public HttpResponseMessage GetUserAllBackends(string userID)
        {
            try
            {
                //check userid for null
                if (!string.IsNullOrEmpty(userID))
                {
                    UserBackend userdevices = new UserBackend();
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
                    return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserBackendDTO>("400", "user does not exists", ""));
                }
            }
            catch (DataAccessException dalexception)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserBackendDTO>("400", dalexception.Message, dalexception.Message));
            }
            catch (BusinessLogicException blexception)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserBackendDTO>("400", blexception.Message, blexception.Message));
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
        /// <param name="userBackendentity">takes list of backends to associate to user</param>
        /// <returns>returns status code for backend association to user</returns>
        [Route("api/personalizationapi/users/{userID}/backends")]
        public HttpResponseMessage PostBackends(PersonalizationRequsetDTO personalizationrequset)
        {
            try
            {
                //check request for userbackend deatils
                if (personalizationrequset.userbackends != null)
                {
                    UserBackend userbackend = new UserBackend();
                    
                    IEnumerable<UserBackendDTO> userbackendsdto = personalizationrequset.userbackends;
                    IEnumerable<UserBackendEntity> userprovidedbackends = userbackend.UserBackendEntityGenerator(userbackendsdto);
                    userbackend.AddBackends(userprovidedbackends.ToList());
                    return Request.CreateResponse(HttpStatusCode.OK);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
                }
            }
            catch (DataAccessException dalexception)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserBackendDTO>("400", dalexception.Message, dalexception.Message));
            }
            catch (BusinessLogicException blexception)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserBackendDTO>("400", blexception.Message, blexception.Message));
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
        /// <param name="userID">takes userid as input</param>
        /// <param name="userBackendID">takes user backend id as input</param>
        /// <returns>returns user backend with given backend id associated to user</returns>
        [Route("api/personalizationapi/users/{userID}/backends/{userBackendID}")]
        public HttpResponseMessage GetUserBackend(string userID, string userBackendID)
        {
            try
            {
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
                    return Request.CreateResponse(HttpStatusCode.BadRequest, DataProvider.PersonalizationResponseError<UserBackendDTO>("400", "userid or associated deviceid empty or null", ""));
                }
            }
            catch (DataAccessException dalexception)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserBackendDTO>("400", dalexception.Message, dalexception.Message));
            }
            catch (BusinessLogicException blexception)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserBackendDTO>("400", blexception.Message, blexception.Message));
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
        /// <param name="userID">takes userid as input</param>
        /// <param name="userBackendID">takes user backend id as input</param>
        /// <returns>returns status code for user backend deletion</returns>        
        [Route("api/personalizationapi/users/{userID}/backends/{userBackendID}")]
        public HttpResponseMessage DeleteUserBackend(string userID, string userBackendID)
        {
            try
            {
                //check userid and userbackendid for null
                if (!(string.IsNullOrEmpty(userID) || string.IsNullOrEmpty(userBackendID)))
                {
                    UserBackend userbackend = new UserBackend();
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
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
                }
            }
            catch (DataAccessException dalexception)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserBackendDTO>("400", dalexception.Message, dalexception.Message));
            }
            catch (BusinessLogicException blexception)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, DataProvider.PersonalizationResponseError<UserBackendDTO>("400", blexception.Message, blexception.Message));
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
