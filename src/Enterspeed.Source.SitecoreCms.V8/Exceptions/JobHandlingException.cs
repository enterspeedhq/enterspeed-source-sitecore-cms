using System;

namespace Enterspeed.Source.SitecoreCms.V8.Exceptions
{
    public class JobHandlingException : Exception
    {
        public JobHandlingException(string message) : base(message)
        {
        }
    }
}