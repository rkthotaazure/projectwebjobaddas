using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;
using adidas.clb.job.UpdateTriggering.Exceptions;
using adidas.clb.job.UpdateTriggering.Utility;
namespace adidas.clb.job.UpdateTriggering.App_Data.DAL
{
    public static class BackgroundTaskRunner
    {
        static IAppInsight InsightLogger { get { return AppInsightLogger.Instance; } }
        public static void FireAndForgetTask(Action action)
        {
            HostingEnvironment.QueueBackgroundWorkItem(cancellationToken => // .Net 4.5.2 required
            {
                try
                {
                    action();
                }
                catch (Exception exception)
                {
                    InsightLogger.Exception(exception.Message, exception, "FireAndForgetTask");
                }
            });
        }
    }
}
