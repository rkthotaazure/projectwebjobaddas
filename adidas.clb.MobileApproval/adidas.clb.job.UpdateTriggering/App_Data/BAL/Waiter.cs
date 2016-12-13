//-----------------------------------------------------------
// <copyright file="Waiter.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
namespace adidas.clb.job.UpdateTriggering.App_Data.BAL
{

    class Waiter : IDisposable
    {

        private readonly System.Timers.Timer timer;
        private readonly EventWaitHandle waitHandle;      
        private bool disposed = false;


        public Waiter(TimeSpan? interval = null)
        {
            waitHandle = new AutoResetEvent(false);
            timer = new System.Timers.Timer();
            timer.Elapsed += (sender, args) => waitHandle.Set();
            SetInterval(interval);
        }

        public TimeSpan Interval
        {
            set { timer.Interval = value.TotalMilliseconds; }
        }

        public void Wait(TimeSpan? newInterval = null)
        {
            SetInterval(newInterval);
            timer.Start();
            waitHandle.WaitOne();
            timer.Close();
            waitHandle.Reset();
        }

        private void SetInterval(TimeSpan? newInterval)
        {
            if (newInterval.HasValue)
            {
                Interval = newInterval.Value;
            }
        }

        #region TimeSpan Calulation

        ////<summary>
        ////Calculating the wait time based on the current time and run schedule defined in config 
        ////</summary>

        public static TimeSpan CalculateTimeSpan(string[] interval)
        {            

            DateTime dt = DateTime.Now;
            TimeSpan returnTS = new TimeSpan();
            List<DateTime> dates = new List<DateTime>();
            int withinRange = 0;

            for (int index = 0; index < interval.Length; index++)
            {
                DateTime dtime = DateTime.Parse(interval[index]);
                dates.Add(dtime);
            }

            dates.Sort();

            for (int index = 0; index < dates.Count; index++)
            {
                withinRange = dates[index].CompareTo(dt);
                if (withinRange == 1)
                {
                    returnTS = dates[index].Subtract(dt);
                    break;
                }
            }

            if (withinRange == -1)
                returnTS = dates[0].AddDays(1).Subtract(dt);

           

            return returnTS.Duration();

        }

        #endregion

        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    if (this.waitHandle != null)
                    {
                        this.waitHandle.Dispose();
                    }

                    if (this.timer != null)
                    {
                        this.timer.Dispose();
                    }
                }

                // Note disposing has been done.
                this.disposed = true;
            }
        }

        public void Dispose()
        {
            this.Dispose(true);

            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }
    }
}
