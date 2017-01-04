using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
namespace adidas.clb.job.GeneratePDF.Utility
{
    /// <summary>
    /// Implements Callinformation class
    /// </summary>
    class CallerInformation
    {
        /// <summary>
        /// This method gives information about the caller of any function
        /// </summary>
        /// <param name="message"></param>
        /// <param name="File_name"></param>
        /// <param name="Line"></param>
        /// <param name="member_name"></param>
        /// <returns></returns>
        /// These attributes must be added as an optional parameter in the method for which there is a need to get the Caller Information
        public static string TrackCallerInformation(string message="", [CallerFilePath] string File_name = "",
           [CallerLineNumber] int Line = 0,
           [CallerMemberName] string member_name = "")
        {
            try
            {
                //These three types of attributes that are used in tracking information.
                //CallerFilePath: Sets the information about caller's source code file.
                //CallerLineNumber: Sets the information about caller's line number.
                //CallerMemberName: Sets the information about caller member name.
                //message: Information related to function.
                string logInfo = "Method Name: " + member_name + "\n File Name:" + File_name + "\n Line No: " + Line + "\n Caller Message: " + message;
                return logInfo;
            }
            catch (Exception exception)
            {
                throw exception;
            }

        }
        /// <summary>
        /// This method retruns caller Method Name
        /// </summary>
        /// <param name="member_name"></param>
        /// <returns></returns>
        public static string TrackCallerMethodName([CallerMemberName] string member_name = "")
        {
            try
            {
               
                //CallerMemberName: Sets the information about caller member name.               
                return member_name;
            }
            catch (Exception exception)
            {
                throw exception;
            }

        }
    }
}
