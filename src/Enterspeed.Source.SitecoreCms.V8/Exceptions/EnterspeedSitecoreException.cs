using System;

namespace Enterspeed.Source.SitecoreCms.V8.Exceptions
{
    public class EnterspeedSitecoreException : Exception
    {
        public EnterspeedSitecoreException(string message)
            : base(message)
        {
        }

        public EnterspeedSitecoreException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}