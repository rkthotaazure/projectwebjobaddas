using adidas.clb.job.GeneratePDF.Models;
using adidas.clb.job.GeneratePDF.Exceptions;
using adidas.clb.job.GeneratePDF.Utility;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace adidas.clb.job.GeneratePDF.App_Data.DAL
{
    public class FieldsMapping
    {
        //Application insights interface reference for logging the error details into Application Insight azure service.
        static IAppInsight InsightLogger { get { return AppInsightLogger.Instance; } }
        /// <summary>
        /// This method returns Data Fields mapping json strings from web.config for given backendID
        /// </summary>
        /// <param name="backendID"></param>
        /// <returns></returns>
        public MappingTypes GetFiledsMappingJsonString(string backendID)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                //Object initialization for C calss
                MappingTypes Objtype = null;
                string requestFieldsMappingJson = string.Empty;
                string matrixFieldsMappingJson = string.Empty;
                //checking is backend is null or empty
                if (!string.IsNullOrEmpty(backendID))
                {

                    switch (backendID.ToLower())
                    {
                        //based on the backendid reading the corresponding mapping json string from web.config
                        case CoreConstants.Backends.CAR:
                            requestFieldsMappingJson = Convert.ToString(ConfigurationManager.AppSettings["Backend.CAR.RequestFeildsMapping"]);
                            matrixFieldsMappingJson = Convert.ToString(ConfigurationManager.AppSettings["Backend.CAR.MatrixFeildsMapping"]);
                            break;
                        case CoreConstants.Backends.BPMOnline:
                            requestFieldsMappingJson = Convert.ToString(ConfigurationManager.AppSettings["Backend.BPMOnline.RequestFeildsMapping"]);
                            matrixFieldsMappingJson = Convert.ToString(ConfigurationManager.AppSettings["Backend.BPMOnline.MatrixFeildsMapping"]);
                            break;
                        default:
                            break;
                    }

                    //Assign values to  MappingTypes class properties
                    Objtype = new MappingTypes()
                    {
                        RequestFieldsMappingJson = requestFieldsMappingJson,
                        MatrixFieldsMappingJson = matrixFieldsMappingJson
                    };

                }
                return Objtype;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                throw new DataAccessException(exception.Message, exception.InnerException);
            }
        }
        /// <summary>
        /// This method returns RequestDetailsMapping object based on request fields mapping json string
        /// </summary>
        /// <param name="row"></param>
        /// <param name="jsonstring"></param>
        /// <returns></returns>
        public T MapDtFieldstoBackendRequest<T>(DataRow row, string jsonstring)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class               
                callerMethodName = CallerInformation.TrackCallerMethodName();
                string reqJson = string.Empty;
                //verifying given datarow is null or not
                if (row != null)
                {
                    //dictionary initialization
                    Dictionary<string, object> dicRequestDetails = new Dictionary<string, object>();
                    //Deserialize given json string into JObject class
                    JObject mapJObject = JsonConvert.DeserializeObject<JObject>(jsonstring);

                    //foreach column in given datarow
                    foreach (DataColumn column in row.Table.Columns)
                    {
                        //foreach Property in mapJObject
                        foreach (KeyValuePair<string, JToken> mapProperty in mapJObject)
                        {
                            //verifying the column name with jobject class property name                            
                            if (column.ColumnName.ToLower() == mapProperty.Value.ToString().ToLower())
                            {
                                //if it match then add  jobject class property name and Datarow column value to  Dictionary<string, object> dicRequestDetails
                                dicRequestDetails.Add(mapProperty.Key.ToString(), row[column.ColumnName]);
                            }
                        }
                    }
                    //Serialize the dictionary into json string
                    reqJson = JsonConvert.SerializeObject(dicRequestDetails);

                }
                //parse the json string into RequestDetailsMapping class
                T ObjRequestDetails = JsonConvert.DeserializeObject<T>(reqJson);
                return ObjRequestDetails;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                throw new DataAccessException(exception.Message, exception.InnerException);

            }
        }
        public Dictionary<string, object> MapDBFieldstoBackendRequest(DataRow row, string jsonstring)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class               
                callerMethodName = CallerInformation.TrackCallerMethodName();
                //dictionary initialization
                Dictionary<string, object> dicRequestDetails = null;
                string reqJson = string.Empty;
                //verifying given datarow is null or not
                if (row != null)
                {

                    dicRequestDetails = new Dictionary<string, object>();
                    //Deserialize given json string into JObject class
                    JObject mapJObject = JsonConvert.DeserializeObject<JObject>(jsonstring);

                    //foreach column in given datarow
                    foreach (DataColumn column in row.Table.Columns)
                    {
                        //foreach Property in mapJObject
                        foreach (KeyValuePair<string, JToken> mapProperty in mapJObject)
                        {
                            //verifying the column name with jobject class property name                            
                            if (column.ColumnName.ToLower() == mapProperty.Value.ToString().ToLower())
                            {
                                //if it match then add  jobject class property name and Datarow column value to  Dictionary<string, object> dicRequestDetails
                                dicRequestDetails.Add(mapProperty.Key.ToString(), row[column.ColumnName]);
                            }
                        }
                    }
                    //Serialize the dictionary into json string
                    // reqJson = JsonConvert.SerializeObject(dicRequestDetails);

                }
                //parse the json string into RequestDetailsMapping class

                return dicRequestDetails;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                throw new DataAccessException(exception.Message, exception.InnerException);

            }
        }
        /// <summary>
        /// This method return list of Approver details based on approver fields mapping json string
        /// </summary>
        /// <param name="dtApprovers"></param>
        /// <param name="jsonstring"></param>
        /// <returns></returns>
        public List<T> MapApproverFieldstoBackendRequest<T>(DataTable dtApprovers, string jsonstring)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class                
                callerMethodName = CallerInformation.TrackCallerMethodName();
                //initialization of list of type ApproverFeildsMapping 
                List<T> lstApproverFeilds = null;
                //verifying given Approvers datatable is null or not
                if (dtApprovers != null && dtApprovers.Rows.Count > 0)
                {
                    lstApproverFeilds = new List<T>();
                    //dictionary initialization
                    Dictionary<string, object> dicApproverFeilds = null;
                    //Deserialize given json string into JObject class
                    JObject approverFieldsmapJObject = JsonConvert.DeserializeObject<JObject>(jsonstring);
                    //foreach row in datatable approvers
                    foreach (DataRow datarow in dtApprovers.Rows)
                    {
                        dicApproverFeilds = new Dictionary<string, object>();
                        //foreach column in given datarow                       
                        foreach (DataColumn dtcolumn in datarow.Table.Columns)
                        {
                            //foreach Property in Approver map fields Jobject
                            foreach (KeyValuePair<string, JToken> appmapProperty in approverFieldsmapJObject)
                            {
                                //verifying the column name with jobject class property name 
                                if (dtcolumn.ColumnName.ToLower() == appmapProperty.Value.ToString().ToLower())
                                {
                                    //if it match then add  jobject class property name and Datarow column value to  Dictionary<string, object> dicApproverFeilds
                                    dicApproverFeilds.Add(appmapProperty.Key.ToString(), datarow[dtcolumn.ColumnName]);
                                }
                            }

                        }
                        //Serialize the dictionary into json string
                        string reqJson = JsonConvert.SerializeObject(dicApproverFeilds);
                        //parse the json string into ApproverFeildsMapping class
                        T objApproverFeilds = JsonConvert.DeserializeObject<T>(reqJson);
                        //add ApproverFeildsMapping object to list
                        lstApproverFeilds.Add(objApproverFeilds);
                    }
                }


                return lstApproverFeilds;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                throw new DataAccessException(exception.Message, exception.InnerException);

            }
        }
    }
}
