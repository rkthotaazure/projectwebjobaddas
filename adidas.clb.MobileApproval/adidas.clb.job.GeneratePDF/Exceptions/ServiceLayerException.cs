using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
namespace adidas.clb.job.GeneratePDF.Exceptions
{
    [Serializable]
    public class ServiceLayerException : CoreException, ISerializable
    {
        #region "Constructors"

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <remarks></remarks>
        public ServiceLayerException()
            : base()
        {
        }

        /// <summary>
        /// Overloaded constructor with message as parameter
        /// </summary>
        /// <param name="message"></param>
        /// <remarks></remarks>
        public ServiceLayerException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Overloaded constructor with message and exception source as parameter
        /// </summary>
        /// <param name="message"></param>
        /// <remarks></remarks>
        public ServiceLayerException(string message, string source)
            : base(message, source)
        {
        }

        /// <summary>
        /// Overloaded constructor with message and inner exception as parameters
        /// </summary>
        /// <param name="message"></param>
        /// <param name="InnerException"></param>
        /// <remarks></remarks>
        public ServiceLayerException(string message, Exception InnerException)
            : base(message, InnerException)
        {
        }

        /// <summary>
        /// Overloaded constructor with message, inner exception and exception code as parameters
        /// </summary>
        /// <param name="message"></param>
        /// <param name="InnerException"></param>
        /// <remarks></remarks>
        public ServiceLayerException(string message, Exception InnerException, string source)
            : base(message, InnerException, source)
        {
        }

        /// <summary>
        /// Overloaded constructor with serialization info and streaming context as parameters
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        /// <remarks></remarks>
        public ServiceLayerException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion
    }
}