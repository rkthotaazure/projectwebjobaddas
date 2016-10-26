using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using adidas.clb.MobileApproval.Models;
using adidas.clb.MobileApproval.Utility;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using adidas.clb.MobileApproval.App_Code.BL.Personalization;

namespace adidas.clb.MobileApproval.Controllers
{
    public class PersonalizationAPIController : ApiController
    {
        
        //GET: api/personalizationapi/backends
        [Route("api/personalizationapi/backends")]
        public IEnumerable<BackendEntity> GetBackends()
        {           
            UserBackend userbackend = new UserBackend();
            return userbackend.GetBackends();
        }

        //Put: api/personalizationapi/users/{userID}
        [Route("api/personalizationapi/users/{userID}")]
        public IHttpActionResult PutUsers(UserEntity user, List<UserDeviceEntity> userprovideddevices, List<UserBackendEntity> userprovidedbackends, int maxsyncreplysize)
        {
            Personalization personalization = new Personalization();
            UserBackend userbackend = new UserBackend();
            Boolean isUserExists = personalization.CheckUser(user.RowKey);
            Boolean isBackendsProvided = true;
            Boolean isDevicesProvided = true;
            if (isUserExists)
            {
                personalization.CreateUser(user);                
                if (isDevicesProvided)
                {
                    userbackend.AddDevices(userprovideddevices);
                }
                if (isBackendsProvided)
                {
                    userbackend.AddBackends(userprovidedbackends);
                }
                personalization.TriggerUserRequests(user.RowKey);
                personalization.CalcSynchTime(userprovidedbackends, maxsyncreplysize);
            }
            else
            {
                userbackend.UpdateUserProp(user);
                userbackend.RemoveBackends(user.RowKey, userprovidedbackends);
                userbackend.RemoveDevices(user.RowKey, userprovideddevices);
                if (isDevicesProvided)
                {                    
                    List<UserDeviceEntity> associateddevices = userbackend.GetAssociatedDevices(userprovideddevices);
                    List<UserDeviceEntity> nonassociateddevices = userbackend.GetNonAssociatedDevices(userprovideddevices,associateddevices);
                    userbackend.AddDevices(nonassociateddevices);
                    userbackend.UpdateDevices(associateddevices);

                }
                if (isBackendsProvided)
                {
                    List<UserBackendEntity> associatedbackends = userbackend.GetAssociatedBackends(userprovidedbackends);
                    List<UserBackendEntity> nonassociatedbackends= userbackend.GetNonAssociatedBackends(userprovidedbackends, associatedbackends);
                    userbackend.AddBackends(nonassociatedbackends);
                    userbackend.UpdateBackends(associatedbackends);
                }
            }
            UserEntity updateduser = personalization.GetUser(user.RowKey);
            return Ok(updateduser);
        }

        //GET: personalization/users/{userID}
        [Route("api/personalizationapi/users/{userID}")]
        public IHttpActionResult GetUser(String userID)
        {
            Personalization personalization = new Personalization();
            personalization.GetUser(userID);
            
        }

        //Delete: api/personalizationapi/users/{userID}
        [Route("api/personalizationapi/users/{userID}")]
        public IHttpActionResult DeleteUser(String userID)
        {
            Personalization personalization = new Personalization();
            CloudTable ReferenceDataTable = personalization.GetAzureTableInstance(CoreConstants.AzureTables.ReferenceData);
            TableOperation RetrieveUser = TableOperation.Retrieve<UserEntity>(CoreConstants.AzureTables.User, userID);            
            TableResult RetrievedResultUser = ReferenceDataTable.Execute(RetrieveUser);
            UserEntity deleteUserEntity = (UserEntity)RetrievedResultUser.Result;
            if (deleteUserEntity != null)
            {
                TableOperation deleteOperation = TableOperation.Delete(deleteUserEntity);
                ReferenceDataTable.Execute(deleteOperation);
                return Ok();
            }
            else
            {
                return NotFound();
            }

        }

        //Get: api/personalizationapi/users/{userID}/devices
        [Route("api/personalizationapi/users/{userID}/devices")]
        public IEnumerable<UserDeviceEntity> GetDevices(String userID)
        {
            Personalization personalization = new Personalization();
            CloudTable table = personalization.GetAzureTableInstance(CoreConstants.AzureTables.UserDeviceConfiguration);
            TableQuery<UserDeviceEntity> query = new TableQuery<UserDeviceEntity>().Where(TableQuery.GenerateFilterCondition(CoreConstants.AzureTables.PartitionKey, QueryComparisons.Equal, "BCK"));
            List<UserDeviceEntity> userDevices = (List<UserDeviceEntity>)table.ExecuteQuery(query);
            return userDevices;
        }

        //Post: api/personalizationapi/users/{userID}/devices
        [Route("api/personalizationapi/users/{userID}/devices")]
        public IHttpActionResult PostDevices(UserDeviceEntity userDevice)
        {
            Personalization personalization = new Personalization();
            CloudTable table = personalization.GetAzureTableInstance(CoreConstants.AzureTables.UserDeviceConfiguration);
            TableOperation insertOperation = TableOperation.Insert(userDevice);            
            table.Execute(insertOperation);
            return Ok();
        }

        //Get: api/personalizationapi/users/{userID}/devices/{userDeviceID}
        [Route("api/personalizationapi/users/{userID}/devices/{userDeviceID}")]
        public IHttpActionResult GetUserDevice(String userID, String userDeviceID)
        {
            Personalization personalization = new Personalization();
            CloudTable table = personalization.GetAzureTableInstance(CoreConstants.AzureTables.UserDeviceConfiguration);
            TableOperation retrieveUser = TableOperation.Retrieve<UserEntity>("User", userDeviceID);            
            TableResult retrievedResult = table.Execute(retrieveUser);
            if (retrievedResult.Result == null)
            {
                return NotFound();
            }
            return Ok(retrievedResult.Result);
        }

        //Delete: api/personalizationapi/users/{userID}/devices/{userDeviceID}
        [Route("api/personalizationapi/users/{userID}/devices/{userDeviceID}")]
        public IHttpActionResult DeleteUserDevice(String userID, String userDeviceID)
        {
            Personalization personalization = new Personalization();
            CloudTable table = personalization.GetAzureTableInstance(CoreConstants.AzureTables.UserDeviceConfiguration);
            TableOperation retrieveUserDevice = TableOperation.Retrieve<UserEntity>("User", userDeviceID);            
            TableResult retrievedUser = table.Execute(retrieveUserDevice);
            UserEntity deleteUserDeviceEntity = (UserEntity)retrievedUser.Result;
            if (deleteUserDeviceEntity != null)
            {
                TableOperation deleteOperation = TableOperation.Delete(deleteUserDeviceEntity);                
                table.Execute(deleteOperation);
                return Ok();
            }
            else
            {
                return NotFound();
            }
        }
        
        //Get: api/personalizationapi/users/{userID}/backends
        [Route("api/personalizationapi/users/{userID}/backends")]
        public IEnumerable<UserDeviceEntity> GetBackends(String userID)
        {
            Personalization personalization = new Personalization();
            CloudTable table = personalization.GetAzureTableInstance(CoreConstants.AzureTables.UserDeviceConfiguration);
            TableQuery<UserDeviceEntity> query = new TableQuery<UserDeviceEntity>().Where(TableQuery.GenerateFilterCondition(CoreConstants.AzureTables.PartitionKey, QueryComparisons.Equal, "BCK"));
            List<UserDeviceEntity> userBackends = (List<UserDeviceEntity>)table.ExecuteQuery(query);
            return userBackends;
        }

        //Post: api/personalizationapi/users/{userID}/backends
        [Route("api/personalizationapi/users/{userID}/backends")]
        public IHttpActionResult PostBackends(UserBackendEntity userBackend)
        {
            Personalization personalization = new Personalization();
            CloudTable table = personalization.GetAzureTableInstance(CoreConstants.AzureTables.UserDeviceConfiguration);
            TableOperation insertOperation = TableOperation.Insert(userBackend);            
            table.Execute(insertOperation);
            return Ok();
        }

        //Get: api/personalizationapi/users/{userID}/backends/{userBackendID}
        [Route("api/personalizationapi/users/{userID}/backends/{userBackendID}")]
        public IHttpActionResult GetUserBackend(String userID, String userBackendID)
        {
            Personalization personalization = new Personalization();
            CloudTable table = personalization.GetAzureTableInstance(CoreConstants.AzureTables.UserDeviceConfiguration);
            TableOperation retrieveUserBackend = TableOperation.Retrieve<UserEntity>("User", userBackendID);            
            TableResult retrievedResult = table.Execute(retrieveUserBackend);
            if (retrievedResult.Result == null)
            {
                return NotFound();
            }
            return Ok(retrievedResult.Result);
        }

        //Delete: api/personalizationapi/users/{userID}/devices/{userBackendID}
        [Route("api/personalizationapi/users/{userID}/devices/{userBackendID}")]
        public IHttpActionResult DeleteUserBackend(String userID, String userBackendID)
        {
            Personalization personalization = new Personalization();
            CloudTable table = personalization.GetAzureTableInstance(CoreConstants.AzureTables.UserDeviceConfiguration);
            TableOperation retrieveUserBackend = TableOperation.Retrieve<UserEntity>("User", userBackendID);            
            TableResult retrievedUserBackend = table.Execute(retrieveUserBackend);
            UserEntity deleteUserBackendEntity = (UserEntity)retrievedUserBackend.Result;
            if (deleteUserBackendEntity != null)
            {
                TableOperation deleteOperation = TableOperation.Delete(deleteUserBackendEntity);                
                table.Execute(deleteOperation);
                return Ok();
            }
            else
            {
                return NotFound();
            }

        }

    }
}
