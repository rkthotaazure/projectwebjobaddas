//-----------------------------------------------------------
// <copyright file="HomeController.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using adidas.clb.MobileApprovalUI.Exceptions;
using adidas.clb.MobileApprovalUI.Models.JSONHelper;
using adidas.clb.MobileApprovalUI.Models;
using adidas.clb.MobileApprovalUI.Utility;
using Newtonsoft.Json;

namespace adidas.clb.MobileApprovalUI.Controllers
{
    /// <summary>
    /// The controller that will handle requests for the home page.
    /// </summary>
    //[Authorize]
    public class HomeController : Controller
    {
        /// <summary>
        /// This methods will be executed to display mobileHome view on
        /// </summary>
        /// <returns>returns mobileHome view if authenticated or navigate to create new user </returns>

        // return CreateNewUser view 
        string userid = SettingsHelper.UserId;
        public ActionResult CreateUser()
        {
            return View();
        }
        // return UpdateUser view 
        public ActionResult UpdateUser()
        {
            return View();
        }
        // return Index view 
        public ActionResult Index()
        {
            return View();

        }
        //Check user exisits or not
        [HttpGet]
        public async Task<ActionResult> CheckUserExisits()
        {
            //creates list Backend model object
            List<UserBackendDTO> backends = new List<UserBackendDTO>();
            //creates list Backend model object
            List<UserBackendDTO> userBackend = new List<UserBackendDTO>();
            //creates list Device model object
            List<UserDeviceDTO> UserDevicedetails = new List<UserDeviceDTO>();
            //NewUser model object initialization
            UserDTO userdetails = new UserDTO();
            try
            {
                //Api Controller object initialization
                APIController apiControllerObj = new APIController();
                NewuserJsonData newuserjsonResponse = new NewuserJsonData();
                //Gets the user details returned by Personalization API
                string strUserExisits = await apiControllerObj.Getuserinfo(userid);
                if (!string.IsNullOrEmpty(strUserExisits))
                {
                    //Deseralize the result returned by the API
                    newuserjsonResponse = JsonConvert.DeserializeObject<NewuserJsonData>(strUserExisits);
                    if (newuserjsonResponse.userResults != null)
                    {
                        // Return Json Formate object and pass to UI
                        return Json(newuserjsonResponse, JsonRequestBehavior.AllowGet);
                    }
                }
                // Return Json Formate object and pass to UI
                return Json(newuserjsonResponse, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error while creating client context : "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                return View("Error");
            }
        }

        //Pass the list of avaliable backends as Json object to Angular JS
        [HttpGet]
        public async Task<JsonResult> GetBackends()
        {
            //creates list Backend model object
            List<UserBackendDTO> backends = new List<UserBackendDTO>();
            backends = await BackEndInfo();
            return Json(backends, JsonRequestBehavior.AllowGet);
        }
        //Pass the User details as Json object to Angular JS
        [HttpGet]
        public async Task<JsonResult> GetUserInfo()
        {
            //NewUser model object initialization
            UserDTO userdetails = new UserDTO();
            // Get user details as list
            userdetails = await Getuserdetails();
            // Return Json Formate object and pass to UI
            return Json(userdetails, JsonRequestBehavior.AllowGet);
        }

        //Gets the Avaliable backend deatils returned by Personalization API
        public async Task<List<UserBackendDTO>> BackEndInfo()
        {
            //creates list Backend model object
            List<UserBackendDTO> backends = new List<UserBackendDTO>();
            try
            {
                //Api Controller object initialization
                APIController apiControllerObj = new APIController();
                //Gets the List of backends returned by the Personalization API
                string strbackEndApp = await apiControllerObj.Getbackendapplications();

                if (!string.IsNullOrEmpty(strbackEndApp))
                {
                    //Deseralize the result returned by the API
                    BackendJsonData backendjsonResponse = JsonConvert.DeserializeObject<BackendJsonData>(strbackEndApp);
                    //Checks whether the JSON response is not null
                    if (backendjsonResponse != null)
                    {
                        //Iterate backend json format result and bind to Model
                        foreach (BackendJsonResult backendid in backendjsonResponse.Results)
                        {
                            //Create  UserBackendDTO Model object
                            UserBackendDTO BackendObj = new UserBackendDTO();
                            BackendObj.backend = new BackendDTO();
                            //Get Backend id
                            BackendObj.backend.BackendID = backendid.BackendID;
                            BackendObj.backend.BackendName = backendid.BackendName;
                            //Adding the Model object to the list
                            backends.Add(BackendObj);
                        }
                    }
                }
                //Return list of backends
                return backends;
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error while creating client context : "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new DataAccessException("Error in BackEndInfo");
            }
        }
        //Gets the user backend deatils returned by Personalization API
        public async Task<UserDTO> Getuserdetails()
        {
            //creates list Backend model object
            List<UserBackendDTO> userBackend = new List<UserBackendDTO>();
            //creates list Device model object
            List<UserDeviceDTO> UserDevicedetails = new List<UserDeviceDTO>();
            try
            {
                //Api Controller object initialization
                APIController apiControllerObj = new APIController();
                //NewUser model object initialization
                UserDTO userdetails = new UserDTO();
                //Gets the user details returned by Personalization API
                string strUserExisits = await apiControllerObj.Getuserinfo(userid);
                if (!string.IsNullOrEmpty(strUserExisits))
                {
                    //Deseralize the result returned by the API
                    NewuserJsonData newuserjsonResponse = JsonConvert.DeserializeObject<NewuserJsonData>(strUserExisits);
                    //Checks whether the JSON response is not null
                    if (newuserjsonResponse.userResults != null)
                    {
                        UserBackendJsonData userBackendjsonResponse = JsonConvert.DeserializeObject<UserBackendJsonData>(strUserExisits);
                        UserDeviceJsonData userDevicejsonResponse = JsonConvert.DeserializeObject<UserDeviceJsonData>(strUserExisits);
                        //Checks whether the JSON response is not null
                        if (newuserjsonResponse != null)
                        {
                            //Getting All updated user info
                            userdetails.UserID = newuserjsonResponse.userResults.UserID;
                            userdetails.FirstName = newuserjsonResponse.userResults.FirstName;
                            userdetails.LastName = newuserjsonResponse.userResults.LastName;
                            userdetails.Fullname = newuserjsonResponse.userResults.Fullname;
                            userdetails.Email = newuserjsonResponse.userResults.Email;
                            userdetails.Domain = newuserjsonResponse.userResults.Domain;
                            userdetails.DeviceName = newuserjsonResponse.userResults.DeviceName;
                            userdetails.DeviceOS = newuserjsonResponse.userResults.DeviceOS;
                        }
                        //Checks whether the JSON response is not null
                        if (userBackendjsonResponse != null)
                        {
                            //Iterate user backend json format result and bind to Model
                            foreach (userBackenddetails UserbackendInfo in userBackendjsonResponse.userBackendResults.userBackenddetails)
                            {
                               //Create Model object
                               UserBackendDTO BackendObj = new UserBackendDTO();
                                BackendObj.UserID = UserbackendInfo.UserID;
                                BackendObj.backend = new BackendDTO();
                                //setting the properties of Model
                                BackendObj.backend.BackendID = UserbackendInfo.userBackend.BackendID;
                                BackendObj.backend.BackendName = UserbackendInfo.userBackend.BackendName;
                                BackendObj.backend.DefaultUpdateFrequency = UserbackendInfo.userBackend.DefaultUpdateFrequency;
                                BackendObj.backend.AverageAllRequestsLatency = UserbackendInfo.userBackend.AverageAllRequestsLatency;
                                BackendObj.backend.AverageAllRequestsSize = UserbackendInfo.userBackend.AverageAllRequestsSize;
                                BackendObj.backend.AverageRequestLatency = UserbackendInfo.userBackend.AverageRequestLatency;
                                BackendObj.backend.AverageRequestSize = UserbackendInfo.userBackend.AverageRequestSize;
                                BackendObj.backend.ExpectedLatency = UserbackendInfo.userBackend.ExpectedLatency;
                                DateTime? expdate= UserbackendInfo.userBackend.ExpectedUpdate;
                                if (expdate != null)
                                {
                                    BackendObj.backend.ExpectedUpdate = expdate.Value;
                                }
                                BackendObj.backend.LastAllRequestsLatency = UserbackendInfo.userBackend.LastAllRequestsLatency;
                                BackendObj.backend.LastAllRequestsSize = UserbackendInfo.userBackend.LastAllRequestsSize;
                                BackendObj.backend.LastRequestLatency = UserbackendInfo.userBackend.LastRequestLatency;
                                BackendObj.backend.LastRequestSize = UserbackendInfo.userBackend.LastRequestSize;
                                DateTime? lstdate = UserbackendInfo.userBackend.LastUpdate;
                                if (expdate != null)
                                {
                                    BackendObj.backend.LastUpdate = lstdate.Value;
                                }                                 
                                BackendObj.backend.OpenApprovals = UserbackendInfo.userBackend.OpenApprovals;
                                BackendObj.backend.OpenRequests = UserbackendInfo.userBackend.OpenRequests;
                                BackendObj.backend.TotalBatchRequestsCount = UserbackendInfo.userBackend.TotalBatchRequestsCount;
                                BackendObj.backend.TotalRequestsCount = UserbackendInfo.userBackend.TotalRequestsCount;
                                BackendObj.backend.UpdateTriggered = UserbackendInfo.userBackend.UpdateTriggered;
                                BackendObj.backend.UrgentApprovals = UserbackendInfo.userBackend.UrgentApprovals;
                               
                                // BackendObj.backend.MissingConfirmationsLimit = UserbackendInfo.userBackend.MissingConfirmationsLimit;
                                //Adding the Model object to the list
                                userBackend.Add(BackendObj);
                                userdetails.userbackends = userBackend;
                            }
                        }
                        //Checks whether the JSON response is not null
                        if (userDevicejsonResponse != null)
                        {
                            //Iterate user devices json format result and bind to Model
                            foreach (userDevicedetails userDeviceInfo in userDevicejsonResponse.userDevicesResults.userDevicedetails)
                            {
                                //Create Model object
                                UserDeviceDTO deviceObj = new UserDeviceDTO();
                                // Get user device details
                                deviceObj.UserID = userDeviceInfo.UserID;
                                deviceObj.device = new DeviceDTO();
                                deviceObj.device.DeviceID = userDeviceInfo.userDevices.DeviceID;
                                deviceObj.device.DeviceName = userDeviceInfo.userDevices.DeviceName;
                                deviceObj.device.DeviceBrand = userDeviceInfo.userDevices.DeviceBrand;
                                deviceObj.device.DeviceModel = userDeviceInfo.userDevices.DeviceModel;
                                deviceObj.device.maxSynchReplySize = userDeviceInfo.userDevices.maxSynchReplySize;
                                // Add details to new model
                                UserDevicedetails.Add(deviceObj);
                                userdetails.userdevices = UserDevicedetails;
                            }
                        }
                    }
                }
                //Return user details as list object
                return userdetails;
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error while creating client context : "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                throw new DataAccessException("Error in Getuserdetails");
            }
        }
        //Get the submitted data from angular js and Post user details to Personalization API
        [HttpPost]
        public async Task<ActionResult> CreateNew(UserDTO userinfo)
        {
            try
            {
                //Api Controller object initialization
                PersonalizationRequsetDTO Personalization = new PersonalizationRequsetDTO();
                //creates list user backend dto object
                List<UserBackendDTO> lstuserbackend = new List<UserBackendDTO>(userinfo.userbackends);
                List<UserBackendDTO> lstupdateduserbackend = new List<Models.UserBackendDTO>();
                //Iterate user backend json format result and add UserID 
                foreach (UserBackendDTO Objuserbackend in lstuserbackend)
                {
                    Objuserbackend.UserID = userid;
                    lstupdateduserbackend.Add(Objuserbackend);
                }
                //creates list user Devices dto object
                List<UserDeviceDTO> lstuserdevices = new List<UserDeviceDTO>(userinfo.userdevices);
                List<UserDeviceDTO> lstupdateduserdevices = new List<Models.UserDeviceDTO>();
                //Iterate user device json format result and add UserID 
                foreach (UserDeviceDTO Objuserdevices in lstuserdevices)
                {
                    Objuserdevices.UserID = userid;
                    lstupdateduserdevices.Add(Objuserdevices);
                }
                //Initialing variables and get user info details
                userinfo.UserID = userid;
                userinfo.userbackends = lstupdateduserbackend;
                userinfo.userdevices = lstupdateduserdevices;
                Personalization.user = userinfo;
                Personalization.userbackends = null;
                Personalization.userdevices = null;
                //Api Controller object initialization
                APIController apiControllerObj = new APIController();
                //Seralize the  User details result into json
                //string ModelSerialize = JsonConvert.SerializeObject(Personalization);                
                var createResponse = await apiControllerObj.SaveUserinfo(Personalization, userid);
                if (createResponse != "OK")
                {
                    LoggerHelper.WriteToLog(" Error while creating client context in UpdateUser method");
                    return View("Error");
                }
                return RedirectToAction("ApprovalLanding", "Landing", new { userid = userid });
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error while creating client context : "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                return View("Error");
            }
        }
        //Get the updated data from angular js and Post user details to Personalization API
        [HttpPost]
        public async Task<ActionResult> UpdateUser(UserDTO userinfo)
        {
            try
            {
                //Api Controller object initialization
                PersonalizationRequsetDTO Personalization = new PersonalizationRequsetDTO();
                userinfo.UserID = userid;
                //creates list user backend dto object
                List<UserBackendDTO> lstuserbackend = new List<UserBackendDTO>(userinfo.userbackends);
                List<UserBackendDTO> lstupdateduserbackend = new List<Models.UserBackendDTO>();
                //Iterate user Backend json format result and add UserID
                foreach (UserBackendDTO Objuserbackend in lstuserbackend)
                {
                    Objuserbackend.UserID = userid;
                    lstupdateduserbackend.Add(Objuserbackend);
                }
                //creates list user Devices dto object
                List<UserDeviceDTO> lstuserdevices = new List<UserDeviceDTO>(userinfo.userdevices);
                List<UserDeviceDTO> lstupdateduserdevices = new List<Models.UserDeviceDTO>();
                //Iterate user Backend json format result and add UserID
                foreach (UserDeviceDTO Objuserdevices in lstuserdevices)
                {
                    Objuserdevices.UserID = userid;
                    lstupdateduserdevices.Add(Objuserdevices);
                }
                //Get user info details
                userinfo.userbackends = lstupdateduserbackend;
                userinfo.userdevices = lstupdateduserdevices;
                Personalization.user = userinfo;
                Personalization.userbackends = null;
                Personalization.userdevices = null;
                //Api Controller object initialization
                APIController apiControllerObj = new APIController();
                //Seralize the  User details result into json
                //string ModelSerialize = JsonConvert.SerializeObject(userinfo);
                var updateResponse = await apiControllerObj.SaveUserinfo(Personalization, userid);
                if (updateResponse != "OK")
                { LoggerHelper.WriteToLog(" Error while creating client context in UpdateUser method");
                    return View("Error");
                }
                // Return Json Formate object and pass to UI
                return Json(updateResponse, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exception)
            {
                LoggerHelper.WriteToLog(exception + " - Error while creating client context : "
                      + exception.ToString(), CoreConstants.Priority.High, CoreConstants.Category.Error);
                return View("Error");
            }  
        }
    }
}