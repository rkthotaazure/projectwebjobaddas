//-----------------------------------------------------------
// <copyright file="CoreException.cs" company="adidas AG">
// Copyright (C) 2016 adidas AG.
// </copyright>
//-----------------------------------------------------------
using System;
using System.Runtime.Serialization;

namespace adidas.clb.MobileApprovalUI.Exceptions
{
    /// <summary>
    /// Custom exception class for handling core exceptions
    /// </summary>
    [Serializable()]
    public class CoreException : Exception, ISerializable
    {
        #region "Constructors"
        /// <summary>
        /// Default constructor
        /// </summary>
        public CoreException()
            : base()
        {
        }

        /// <summary>
        /// Overloaded constructor having message as parameter
        /// </summary>
        /// <param name="message">message for Exception</param>
        public CoreException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Overloaded constructor having message, source as parameter
        /// </summary>
        /// <param name="message">message for Exception</param>
        /// <param name="source">source string</param>
        public CoreException(string message, string source)
            : base(message)
        {
        }

        /// <summary>
        /// Overloaded constructor having message, inner exception as parameter
        /// </summary>
        /// <param name="message">message</param>
        /// <param name="innerException">inner Exception</param>
        public CoreException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Overloaded constructor having message, source string and inner Exception as parameter
        /// </summary>
        /// <param name="message">message</param>
        /// <param name="innerException">inner Exception</param>
        /// <param name="source">source string</param>
        public CoreException(string message, Exception innerException, string source)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Overloaded constructor having serialization Info and streaming context as parameter
        /// </summary>
        /// <param name="info">serialization info</param>
        /// <param name="context">context for streaming</param>
        public CoreException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
        #endregion
    }
}