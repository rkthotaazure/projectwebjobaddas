//-----------------------------------------------------------
// <copyright file="BusinessLogicException.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using System;
using System.Runtime.Serialization;

namespace adidas.clb.job.RequestsUpdate.Exceptions
{
    /// <summary>
    /// Custom exception class for handling business logic exceptions
    /// </summary>
    [Serializable()]
    public class BusinessLogicException : CoreException, ISerializable
    {
        #region "Constructors"

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <remarks></remarks>
        public BusinessLogicException()
            : base()
        {
        }

        /// <summary>
        /// Overloaded constructor with message as parameter
        /// </summary>
        /// <param name="message">message</param>
        /// <remarks></remarks>
        public BusinessLogicException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Overloaded constructor with message and exception source as parameter
        /// </summary>
        /// <param name="message">message</param>
        /// <remarks></remarks>
        public BusinessLogicException(string message, string source)
            : base(message, source)
        {
        }

        /// <summary>
        /// Overloaded constructor with message and inner exception as parameters
        /// </summary>
        /// <param name="message">message</param>
        /// <param name="InnerException">inner Exception</param>
        /// <remarks></remarks>
        public BusinessLogicException(string message, Exception InnerException)
            : base(message, InnerException)
        {
        }

        /// <summary>
        /// Overloaded constructor with message, inner exception and exception source as parameters
        /// </summary>
        /// <param name="message">message</param>
        /// <param name="InnerException">inner Exception</param>
        /// <param name="source">source string</param>
        public BusinessLogicException(string message, Exception InnerException, string source)
            : base(message, InnerException, source)
        {
        }

        /// <summary>
        /// Overloaded constructor with serialization info and streaming context as parameters
        /// </summary>
        /// <param name="info">serialization info</param>
        /// <param name="context">context for streaming</param>
        /// <remarks></remarks>
        public BusinessLogicException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion
    }
}