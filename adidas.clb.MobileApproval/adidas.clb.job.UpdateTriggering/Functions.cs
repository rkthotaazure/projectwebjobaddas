using adidas.clb.job.UpdateTriggering.App_Data.BAL;
using adidas.clb.job.UpdateTriggering.App_Data.DAL;
using adidas.clb.job.UpdateTriggering.Exceptions;
using adidas.clb.job.UpdateTriggering.Helpers;
using adidas.clb.job.UpdateTriggering.Models;
using adidas.clb.job.UpdateTriggering.Utility;
using Microsoft.Azure;
using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions;
using Microsoft.Azure.WebJobs.Extensions.Timers;
using System.Runtime.CompilerServices;
namespace adidas.clb.job.UpdateTriggering
{

    public class Functions
    {
        //Application insights interface reference for logging the error details into Application Insight azure service.
        static IAppInsight InsightLogger { get { return AppInsightLogger.Instance; } }
        public static string azureTableUserDeviceConfiguration = ConfigurationManager.AppSettings["AzureTables.UserDeviceConfiguration"];
        // This function will get triggered/executed when a new message is written 
        // on an Azure Queue called queue.
        /// <summary>
        /// This method automatically triggered when ever a new message inserted into queue.        
        /// </summary>
        public static void ProcessQueueMessage([QueueTrigger("%utQueue%")] string message, TextWriter log)
        {
            //Get Caller Method name
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                string queueName = Convert.ToString(ConfigurationManager.AppSettings["UpdateTriggerInputQueue"]);
                // InsightLogger.TrackStartEvent(callerMethodName);
                //checking update triggering queue message null or empty
                if (!string.IsNullOrEmpty(message))
                {
                    //log.WriteLine("adidas.clb.job.UpdateTriggering :: Processing update triggering queue message :: start()" + message);
                    //write message into application insights
                    InsightLogger.TrackEvent(queueName + " , Action :: UpdateTriggering input queue triggered : Start(), \n Response :: Message:" + message);
                    //Deserializ input queue message into UpdateTriggeringMsg object
                    UpdateTriggeringMsg objUTMsg = JsonConvert.DeserializeObject<UpdateTriggeringMsg>(message);
                    //checking UpdateTriggeringMsg is null or not
                    if (objUTMsg != null)
                    {
                        //get userupdateMsg list from UpdateTriggeringMsg
                        List<UserUpdateMsg> lstUsers = null;
                        //checking users list in UpdateTriggeringMsg null or not
                        if (objUTMsg.Users != null)
                        {
                            lstUsers = objUTMsg.Users.ToList();
                            InsightLogger.TrackEvent(queueName + " , Action :: Users Provided? , Response :: true ");
                        }
                        //get RequestUpdateMsg list from UpdateTriggeringMsg
                        List<RequestUpdateMsg> lstRequests = null;
                        //checking request list in UpdateTriggeringMsg null or not
                        if (objUTMsg.Requests != null)
                        {
                            lstRequests = objUTMsg.Requests.ToList();
                            InsightLogger.TrackEvent(queueName + " , Action :: Requests Provided? , Response :: true ");
                        }
                        //Declare a CancellationToken object, which indicates whether cancellation is requested
                        var ctsut = new CancellationTokenSource();
                        //create object for userbackend class for calling the Updateusers & updateRequest methods
                        UserBackendDAL objUserbackendDAL = new UserBackendDAL();
                        //Parallely update the user update and request update messages from UserUpdateMsg.
                        Task[] tasksProcessQueueMsg = new Task[2];
                        //Process users update messages
                        tasksProcessQueueMsg[0] = Task.Factory.StartNew(() => objUserbackendDAL.UpdateUserBackends(lstUsers, objUTMsg.VIP, objUTMsg.GetPDFs, objUTMsg.ChangeAfter, queueName));
                        //Process request update Messages
                        tasksProcessQueueMsg[1] = Task.Factory.StartNew(() => objUserbackendDAL.UpdateRequests(lstRequests, objUTMsg.VIP, objUTMsg.GetPDFs, objUTMsg.ChangeAfter, queueName));
                        //create processTimeoutperiod variable and assign the value from app.config file  
                        int processTimeoutperiod = Convert.ToInt32(CloudConfigurationManager.GetSetting("timeoutperiod"));
                        //if all the tasks which are not completed with in the timeout period then those task automatically canceled by CancellationToken object cancel method.
                        if (!Task.WaitAll(tasksProcessQueueMsg, processTimeoutperiod, ctsut.Token))
                        {
                            //Communicates a request for cancellation
                            ctsut.Cancel();
                        }
                        //log.WriteLine("adidas.clb.job.UpdateTriggering :: Processing update triggering queue message :: End()" + message);
                        //write message into application insights
                        InsightLogger.TrackEvent(queueName + " , Action :: UpdateTriggering input queue triggered : End()");
                    }
                    else
                    {
                        InsightLogger.TrackEvent(queueName + " , Action :: update triggering message is null");
                    }
                }
            }
            catch (DataAccessException dalexception)
            {

                //write exception message to web job dashboard logs
                //log.WriteLine(dalexception.Message);
                //write exception into application insights
                InsightLogger.Exception(dalexception.Message, dalexception, callerMethodName);
            }
            catch (Exception exception)
            {
                //write exception message to web job dashboard logs
                // log.WriteLine(exception.Message);
                //write exception into application insights
                InsightLogger.Exception(exception.Message, exception, callerMethodName);

            }
        }
        public static void ProcessMissingUpdates([QueueTrigger("%utMissingUpdatesQueue%")] string message, TextWriter log)
        {
            //Get Caller Method name
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                string queueName = Convert.ToString(ConfigurationManager.AppSettings["UpdateTriggerMissedUpdatesInputQueue"]);
                // InsightLogger.TrackStartEvent(callerMethodName);
                //checking update triggering queue message null or empty
                if (!string.IsNullOrEmpty(message))
                {
                    //log.WriteLine("adidas.clb.job.UpdateTriggering :: Processing update triggering queue message :: start()" + message);
                    //write message into application insights
                    InsightLogger.TrackEvent(queueName + " , Action :: UpdateTriggering input queue triggered : Start(), \n Response :: Message:" + message);
                    //Deserializ input queue message into UpdateTriggeringMsg object
                    UpdateTriggeringMsg objUTMsg = JsonConvert.DeserializeObject<UpdateTriggeringMsg>(message);
                    //checking UpdateTriggeringMsg is null or not
                    if (objUTMsg != null)
                    {
                        //get userupdateMsg list from UpdateTriggeringMsg
                        List<UserUpdateMsg> lstUsers = null;
                        //checking users list in UpdateTriggeringMsg null or not
                        if (objUTMsg.Users != null)
                        {
                            lstUsers = objUTMsg.Users.ToList();
                            InsightLogger.TrackEvent(queueName + " , Action :: Users Provided? , Response :: true ");
                        }
                        //get RequestUpdateMsg list from UpdateTriggeringMsg
                        List<RequestUpdateMsg> lstRequests = null;
                        //checking request list in UpdateTriggeringMsg null or not
                        if (objUTMsg.Requests != null)
                        {
                            lstRequests = objUTMsg.Requests.ToList();
                            InsightLogger.TrackEvent(queueName + " , Action :: Requests Provided? , Response :: true ");
                        }
                        //Declare a CancellationToken object, which indicates whether cancellation is requested
                        var ctsut = new CancellationTokenSource();
                        //create object for userbackend class for calling the Updateusers & updateRequest methods
                        UserBackendDAL objUserbackendDAL = new UserBackendDAL();
                        //Parallely update the user update and request update messages from UserUpdateMsg.
                        Task[] tasksProcessQueueMsg = new Task[2];
                        //Process users update messages
                        tasksProcessQueueMsg[0] = Task.Factory.StartNew(() => objUserbackendDAL.UpdateUserBackends(lstUsers, objUTMsg.VIP, objUTMsg.GetPDFs, objUTMsg.ChangeAfter, queueName));
                        //Process request update Messages
                        tasksProcessQueueMsg[1] = Task.Factory.StartNew(() => objUserbackendDAL.UpdateRequests(lstRequests, objUTMsg.VIP, objUTMsg.GetPDFs, objUTMsg.ChangeAfter, queueName));
                        //create processTimeoutperiod variable and assign the value from app.config file  
                        int processTimeoutperiod = Convert.ToInt32(CloudConfigurationManager.GetSetting("timeoutperiod"));
                        //if all the tasks which are not completed with in the timeout period then those task automatically canceled by CancellationToken object cancel method.
                        if (!Task.WaitAll(tasksProcessQueueMsg, processTimeoutperiod, ctsut.Token))
                        {
                            //Communicates a request for cancellation
                            ctsut.Cancel();
                        }
                        //log.WriteLine("adidas.clb.job.UpdateTriggering :: Processing update triggering queue message :: End()" + message);
                        //write message into application insights
                        InsightLogger.TrackEvent(queueName + " , Action :: UpdateTriggering input queue triggered : End()");
                    }
                    else
                    {
                        InsightLogger.TrackEvent(queueName + " , Action :: update triggering message is null");
                    }
                }
            }
            catch (DataAccessException dalexception)
            {

                //write exception message to web job dashboard logs
                //log.WriteLine(dalexception.Message);
                //write exception into application insights
                InsightLogger.Exception(dalexception.Message, dalexception, callerMethodName);
            }
            catch (Exception exception)
            {
                //write exception message to web job dashboard logs
                // log.WriteLine(exception.Message);
                //write exception into application insights
                InsightLogger.Exception(exception.Message, exception, callerMethodName);

            }
        }
        /// <summary>
        /// This method automatically triggered when ever a new message inserted into vipmessagesqueue.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="log"></param>
        public static void TriggerVIPMessagesQueue([QueueTrigger("%vipQueue%")] string message, TextWriter log)
        {
            //Get Caller Method name
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                string queueName = Convert.ToString(ConfigurationManager.AppSettings["VIPMessagesQueue"]);
                // InsightLogger.TrackStartEvent(callerMethodName);
                //checking update triggering message null or empty
                if (!string.IsNullOrEmpty(message))
                {
                    //write message into application insights
                    InsightLogger.TrackEvent(queueName + " , Action :: VIP Queue triggered : Start(), \n Response :: Message:" + message);
                    //Deserializ input queue message into UpdateTriggeringMsg object
                    UpdateTriggeringMsg objUTMsg = JsonConvert.DeserializeObject<UpdateTriggeringMsg>(message);
                    //checking UpdateTriggeringMsg is null or not
                    if (objUTMsg != null)
                    {
                        //get userupdateMsg list from UpdateTriggeringMsg
                        List<UserUpdateMsg> lstUsers = null;
                        //checking users list in UpdateTriggeringMsg null or not
                        if (objUTMsg.Users != null)
                        {
                            lstUsers = objUTMsg.Users.ToList();
                            InsightLogger.TrackEvent(queueName + " , Action :: Users Provided? , Response :: true ");
                        }

                        //get RequestUpdateMsg list from UpdateTriggeringMsg
                        List<RequestUpdateMsg> lstRequests = null;
                        //checking request list in UpdateTriggeringMsg null or not
                        if (objUTMsg.Requests != null)
                        {
                            lstRequests = objUTMsg.Requests.ToList();
                            InsightLogger.TrackEvent(queueName + " , Action :: Requests Provided? , Response :: true ");
                        }
                        //Declare a CancellationToken object, which indicates whether cancellation is requested
                        var ctsut = new CancellationTokenSource();
                        //create object for userbackend class for calling the Updateusers & updateRequest methods
                        UserBackendDAL objUserbackendDAL = new UserBackendDAL();
                        //Parallely update the user update and request update messages from UserUpdateMsg.
                        Task[] tasksProcessQueueMsg = new Task[2];
                        //Process users update messages
                        tasksProcessQueueMsg[0] = Task.Factory.StartNew(() => objUserbackendDAL.UpdateUserBackends(lstUsers, objUTMsg.VIP, objUTMsg.GetPDFs, objUTMsg.ChangeAfter, queueName));
                        //Process request update Messages
                        tasksProcessQueueMsg[1] = Task.Factory.StartNew(() => objUserbackendDAL.UpdateRequests(lstRequests, objUTMsg.VIP, objUTMsg.GetPDFs, objUTMsg.ChangeAfter, queueName));
                        //create processTimeoutperiod variable and assign the value from app.config file  
                        int processTimeoutperiod = Convert.ToInt32(CloudConfigurationManager.GetSetting("timeoutperiod"));
                        //if all the tasks which are not completed with in the timeout period then those task automatically canceled by CancellationToken object cancel method.
                        if (!Task.WaitAll(tasksProcessQueueMsg, processTimeoutperiod, ctsut.Token))
                        {
                            //Communicates a request for cancellation
                            ctsut.Cancel();
                        }
                        //log.WriteLine("adidas.clb.job.UpdateTriggering :: Processing update triggering queue message :: End()" + message);
                        //write message into application insights
                        InsightLogger.TrackEvent(queueName + " , Action :: VIP Queue triggered : End()");
                    }
                    else
                    {
                        InsightLogger.TrackEvent(queueName + " , Action :: VIP Queue message is null");
                    }
                }
            }
            catch (DataAccessException dalexception)
            {

                //write exception message to web job dashboard logs
                //log.WriteLine(dalexception.Message);
                //write exception into application insights
                InsightLogger.Exception(dalexception.Message, dalexception, callerMethodName);
            }
            catch (Exception exception)
            {
                //write exception message to web job dashboard logs
                // log.WriteLine(exception.Message);
                //write exception into application insights
                InsightLogger.Exception(exception.Message, exception, callerMethodName);

            }
        }

        /// <summary>        
        /// This method updates all the backends next collecting time in Azure Referencedata table
        /// This method triggered based on given time interval(i.e MyDailyScheduleForUpdateNextCollectingTime)
        /// it is scheduled by every hour
        /// </summary>
        public static void ForEachBackendSetNextCollectingTime([TimerTrigger(typeof(MyDailyScheduleForUpdateNextCollectingTime))] TimerInfo timerInfo, TextWriter log)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class                
                callerMethodName = CallerInformation.TrackCallerMethodName();
                bool IsFirstTime = false;
                ////Create object for NextUserCollectingTime class
                NextUserCollectingTimeDAL objdal = new NextUserCollectingTimeDAL();
                //call the UpdateNextCollectingTime method which will update the Next Collecting Time of the each backend
                objdal.UpdateNextCollectingTime(IsFirstTime);
                // InsightLogger.TrackEndEvent(callerMethodName);
            }
            catch (DataAccessException dalexception)
            {
                //write exception message to web job dashboard logs
                //log.WriteLine(dalexception.Message);
                //write data layer exception into application insights
                InsightLogger.Exception(dalexception.Message, dalexception, callerMethodName);
            }
            catch (Exception exception)
            {
                //log.WriteLine("Error in adidas.clb.job.UpdateTriggering :: Functions :: UpdateNextCollectingTimeForAllBackends() :: Exception Message=" + exception.Message);
                //write exception into application insights
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
            }


        }
        /// <summary>
        /// This method triggered based on given time interval(i.e MyDailyScheduleForRegularUpdates)
        /// This method will verifies whether the userbackends needs update or not based on UT RUle ,if it requires update then it will update the userbackend
        /// if any user needs update then  keep the messages into update trigger input queue in UpdateTriggerMsg Format.
        /// </summary>
        public static void CollectUsersNeedsUpdate([TimerTrigger(typeof(MyDailyScheduleForRegularUpdates))] TimerInfo timerInfo, TextWriter log)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();

                NextUserCollectingTimeDAL objnextcollentingTime = new NextUserCollectingTimeDAL();
                //get all the userbackends needs to update
                List<NextUserCollectingTimeEntity> lstbackends = objnextcollentingTime.GetBackendsNeedsUpdate();
                //foreach backend  
                DateTime currentTimestamp = DateTime.Now;
                Parallel.ForEach<NextUserCollectingTimeEntity>(lstbackends, backend =>
                {
                    InsightLogger.TrackEvent("UpdateTriggering, Action :: for each backend :: collect users needing update : start(), Response :: Backend Name : " + backend.BackendID);
                    //getting minutes difference between currenttime and Regular Update Next CollectingTime
                    TimeSpan span = backend.RegularUpdateNextCollectingTime.Subtract(currentTimestamp);
                    int minutes = span.Minutes;
                    //if minutes difference is with in RegularChecksWaitingTimeInMinutes(>=-5 and <=0) then invoke CollectUsersNeedUpdateByBackend method()
                    if (minutes >= -(Convert.ToInt32(ConfigurationManager.AppSettings["RegularChecksWaitingTimeInMinutes"])) && minutes <= 0)
                    {
                        UserBackendDAL objdal = new UserBackendDAL();
                        //collect the users needing update and keep the messages in update trigger input queue
                        objdal.CollectUsersNeedUpdateByBackend(backend.BackendID, currentTimestamp);
                        //update the backend entity with new collecting time[i.e LastCollectingTime= NextCollectingTime and NextCollectingTime=NextCollectingTime+(Backend MinimumusersUpdateFrequency)/2 ]
                        objnextcollentingTime.UpdateBackendRegularNextCollectingTime(backend.BackendID, backend.MinimumUpdateFrequency, backend.RegularUpdateNextCollectingTime);
                        InsightLogger.TrackEvent("UpdateTriggering, Action :: for each backend :: collect users needing update : End(), Response :: Success,  Backend Name : " + backend.BackendID);
                    }
                    else
                    {
                        InsightLogger.TrackEvent("UpdateTriggering, Action :: for each backend :: collect users needing update : End(), Response :: next collecting time of the backend [ " + backend.BackendID + " ] is greater-than to current time.");

                    }

                });

            }
            catch (BusinessLogicException balexception)
            {
                //write exception message to web job dashboard logs
                // log.WriteLine(balexception.Message);
                //write (Business Logic Exception into application insights
                InsightLogger.Exception(balexception.Message, balexception, callerMethodName);
            }
            catch (DataAccessException dalexception)
            {
                //write exception message to web job dashboard logs
                //log.WriteLine(dalexception.Message);
                //write Data Access Exception into application insights
                InsightLogger.Exception(dalexception.Message, dalexception, callerMethodName);
            }
            catch (Exception exception)
            {
                //  log.WriteLine("Error in adidas.clb.job.UpdateTriggering :: Functions :: RegularChecksforBackendNeedsUpdate() :: Exception Message=" + exception.Message);
                //write exception into application insights
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
            }
        }
        /// <summary>
        /// This method triggered based on given time interval(i.e MyDailyScheduleForMissingUpdates)
        /// This method will verifies whether any userbackend(s)/Request(s) missed update or not
        /// if any user(s)/Request(s) misses update then keep the messages into update trigger input queue in UpdateTriggerMsg Format. 
        /// </summary>
        public static void CollectMissingUpdates([TimerTrigger(typeof(MyDailyScheduleForMissingUpdates))] TimerInfo timerInfo, TextWriter log)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                // InsightLogger.TrackStartEvent(callerMethodName);
                //foreach backend               
                NextUserCollectingTimeDAL objnextcollectingTime = new NextUserCollectingTimeDAL();
                //get all the userbackends needs to update
                List<NextUserCollectingTimeEntity> lstbackends = objnextcollectingTime.GetBackendsNeedsUpdate();

                DateTime currentTimestampForMissedUpdates = DateTime.Now;
                Parallel.ForEach<NextUserCollectingTimeEntity>(lstbackends, backend =>
                {
                    InsightLogger.TrackEvent("UpdateTriggering, Action :: for each backend :: collect missing updates : start() , Response :: Backend Name : " + backend.BackendID);
                    //getting minutes difference between currenttime and Missing Update Next CollectingTime
                    TimeSpan tspan = backend.MissingUpdateNextCollectingTime.Subtract(currentTimestampForMissedUpdates);
                    int waitingMinutes = tspan.Minutes;
                    //if minutes difference is with in RegularChecksWaitingTimeInMinutes(>=-8 and <=0) then invoke MissedUpdatesWaitingTimeInMinutes method()
                    if (waitingMinutes >= -(Convert.ToInt32(ConfigurationManager.AppSettings["MissedUpdatesWaitingTimeInMinutes"])) && waitingMinutes <= 0)
                    {
                        UserBackendDAL objUserBackendDAL = new UserBackendDAL();
                        //collects the missed update userbackends,Requests and convert into update trigger message format and put into UT input queue
                        objUserBackendDAL.CollectUsersMissedUpdatesByBackend(backend.BackendID, currentTimestampForMissedUpdates);
                        //update the backend entity with new missing update collecting time[i.e MissingUpdateLastCollectingTime= MissingUpdateNextCollectingTime and MissingUpdateNextCollectingTime=Max(]
                        objnextcollectingTime.UpdateMisseduserBackendNextCollectingTime(backend.BackendID, backend.MissingUpdateNextCollectingTime, DateTime.Now);
                        InsightLogger.TrackEvent("UpdateTriggering, Action :: for each backend :: collect missing updates : End() , Response :: Success, Backend Name : " + backend.BackendID);
                    }
                    else
                    {
                        InsightLogger.TrackEvent("UpdateTriggering, Action :: for each backend :: collect missing updates : End(), Response ::  next missing update collecting time of the backend [ " + backend.BackendID + " ] is greater-than to current time.");

                    }
                });

            }
            catch (BusinessLogicException balexception)
            {
                //write exception message to web job dashboard logs
                //log.WriteLine(balexception.Message);
                //write Business Logic Exception into application insights
                InsightLogger.Exception(balexception.Message, balexception, callerMethodName);
            }
            catch (DataAccessException dalexception)
            {
                //write exception message to web job dashboard logs
                //log.WriteLine(dalexception.Message);
                //write Data layer logic Exception into application insights
                InsightLogger.Exception(dalexception.Message, dalexception, callerMethodName);
            }
            catch (Exception exception)
            {
                // log.WriteLine("Error in adidas.clb.job.UpdateTriggering :: Functions :: RegularChecksforUserbackendLostsUpdate() :: Exception Message=" + exception.Message);
                //write  Exception into application insights
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
            }

        }


        /// <summary>
        /// This method generates time intervals based on given minutes time span
        /// </summary>
        /// <param name="minutesTimespan"></param>
        /// <returns></returns>
        public static string[] GetTimeIntervals(int minutesTimespan)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                //InsightLogger.TrackStartEvent(callerMethodName);
                List<string> lsttimeIntervals = new List<string>();
                int j = 0;
                for (int i = 0; i <= CoreConstants.TimeIntervals.TotalHours; i++) //hours
                {
                    while (j <= CoreConstants.TimeIntervals.TotalMinutes)//minutes
                    {
                        lsttimeIntervals.Add(getTimeformat(i, j));
                        j = j + minutesTimespan;

                    }
                    j = 0;

                }
                //InsightLogger.TrackEndEvent(callerMethodName);
                return lsttimeIntervals.ToArray();
            }
            catch (BusinessLogicException balexception)
            {
                //write  Data Access Exception into application insights
                InsightLogger.Exception(balexception.Message, balexception, callerMethodName);
                throw balexception;
            }

        }
        /// <summary>
        /// This method format the string in HH:mm:SS format
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <returns></returns>
        public static string getTimeformat(int i, int j)
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                int singledigit = CoreConstants.TimeIntervals.singledigit;
                string strseconds = CoreConstants.TimeIntervals.TotalSeconds;
                string timeformat = string.Empty;
                if (i <= singledigit && j <= singledigit)
                {
                    timeformat = "0" + i + ":" + "0" + j + ":" + strseconds;
                }
                else if (i <= singledigit && j > singledigit)
                {
                    timeformat = "0" + i + ":" + j + ":" + strseconds;
                }
                else if (i > singledigit && j <= singledigit)
                {
                    timeformat = i + ":" + "0" + j + ":" + strseconds;
                }
                else if (i > singledigit && j > singledigit)
                {
                    timeformat = i + ":" + j + ":" + strseconds;
                }

                return timeformat;
            }
            catch (BusinessLogicException balexception)
            {
                //write  Data Access Exception into application insights
                InsightLogger.Exception(balexception.Message, balexception, callerMethodName);
                throw balexception;
            }
        }
        /// <summary>
        /// This class generates the time schedule,based on this timeschedule  userbackends regular update code will be triggered.
        /// </summary>
        public class MyDailyScheduleForRegularUpdates : DailySchedule
        {
            public MyDailyScheduleForRegularUpdates() : base(GetTimeIntervals(int.Parse(ConfigurationManager.AppSettings["RegularChecksWaitingTimeInMinutes"])))
            {

            }
        }
        /// <summary>
        /// This class generates the time schedule,based on this timeschedule  userbackends/requests missing updates code will be triggered.
        /// </summary>
        public class MyDailyScheduleForMissingUpdates : DailySchedule
        {
            public MyDailyScheduleForMissingUpdates() : base(GetTimeIntervals(int.Parse(ConfigurationManager.AppSettings["MissedUpdatesWaitingTimeInMinutes"])))
            {

            }
        }
        /// <summary>
        /// This class generates the time schedule,based on this timeschedule  UpdateNextCollectingTime of the backend code will be triggered.
        /// </summary>
        public class MyDailyScheduleForUpdateNextCollectingTime : DailySchedule
        {
            //public MyDailyScheduleForUpdateNextCollectingTime() : base(Convert.ToString(ConfigurationManager.AppSettings["DailyScheduleForUpdateNextCollectingTime"]).Split(','))
            //{

            //}
            public MyDailyScheduleForUpdateNextCollectingTime() : base(GetTimeIntervals(int.Parse(ConfigurationManager.AppSettings["DailyScheduleForUpdateNextCollectingTime"])))
            {

            }
        }
        /// <summary>
        /// This class generates the time schedule for collecting the requests which are not updated from azure layer based on application configuration setting value
        /// </summary>
        //public class MyDailyScheduleForRequestsUpdateNextCollectingTime : DailySchedule
        //{
        //    public MyDailyScheduleForRequestsUpdateNextCollectingTime() : base(GetTimeIntervals(int.Parse(ConfigurationManager.AppSettings["DailyScheduleForRequestsUpdateNextCollectingTime"])))
        //    {

        //    }
        //}

    }

}
