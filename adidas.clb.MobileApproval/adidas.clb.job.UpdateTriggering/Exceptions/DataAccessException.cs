//-----------------------------------------------------------
// <copyright file="DataAccessException.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using System;
using System.Runtime.Serialization;

namespace adidas.clb.job.UpdateTriggering.Exceptions
{
    /// <summary>
    /// Custom exception class for handling controller level exceptions
    /// </summary>
    [Serializable()]
    public class DataAccessException : CoreException, ISerializable
    {
        #region "Constructors"

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <remarks></remarks>
        public DataAccessException()
            : base()
        {
        }

        /// <summary>
        /// Overloaded constructor with message as parameter
        /// </summary>
        /// <param name="message"></param>
        /// <remarks></remarks>
        public DataAccessException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Overloaded constructor with message and exception source as parameter
        /// </summary>
        /// <param name="message"></param>
        /// <remarks></remarks>
        public DataAccessException(string message, string source)
            : base(message, source)
        {
        }

        /// <summary>
        /// Overloaded constructor with message and inner exception as parameters
        /// </summary>
        /// <param name="message"></param>
        /// <param name="InnerException"></param>
        /// <remarks></remarks>
        public DataAccessException(string message, Exception InnerException)
            : base(message, InnerException)
        {
        }

        /// <summary>
        /// Overloaded constructor with message, inner exception and exception code as parameters
        /// </summary>
        /// <param name="message"></param>
        /// <param name="InnerException"></param>
        /// <remarks></remarks>
        public DataAccessException(string message, Exception InnerException, string source)
            : base(message, InnerException, source)
        {
        }

        /// <summary>
        /// Overloaded constructor with serialization info and streaming context as parameters
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        /// <remarks></remarks>
        public DataAccessException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion
    }
}