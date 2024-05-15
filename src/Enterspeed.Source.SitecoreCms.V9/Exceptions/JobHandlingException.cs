using System;

namespace Enterspeed.Source.SitecoreCms.V9.Exceptions
{
    public class JobHandlingException : Exception
    {
        public JobHandlingException(string message)
            : base(message)
        {
        }
    }
}