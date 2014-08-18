using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intro3DFramework.ResourceSystem
{
    /// <summary>
    /// Exception for any errors connected to resources.
    /// </summary>
    /// <see cref="IResource"/>
    /// <see cref="ResourceManager"/>
    public class ResourceException : Exception
    {
        /// <summary>
        /// Different types of resource failures.
        /// </summary>
        public enum Type
        {
            NOT_FOUND,      /// The resource was not found.
            LOAD_ERROR,     /// The resource was successfully found but an error occurred during the loading process.
            PROCESS_ERROR   /// The resource was successfully loaded but an error occurred during a postprocessing step.
        }

        /// <summary>
        /// Error type of this exception.
        /// </summary>
        public Type ErrorType { get; private set; }

        /// <summary>
        /// Creates a new resource exception.
        /// </summary>
        /// <param name="type">The error type.</param>
        /// <param name="description">A detailed description what caused the exception.</param>
        /// <param name="innerException">An optional inner exception that triggered the event or is connected to it.</param>
        public ResourceException(Type type, string description, Exception innerException = null) : 
            base(description, innerException)
        {
            ErrorType = type;
        }
    }
}
