using System;
using Enterspeed.Source.SitecoreCms.V8.Services.Contracts;
using log4net;
using Sitecore.Diagnostics;

namespace Enterspeed.Source.SitecoreCms.V8.Services
{
    public class EnterspeedSitecoreLoggingService : IEnterspeedSitecoreLoggingService
    {
        private readonly ILog _log;

        public EnterspeedSitecoreLoggingService()
        {
            _log = LoggerFactory.GetLogger("Enterspeed.Logger");
        }

        public void Debug(string message)
        {
            _log.Debug(message);
        }

        public void Debug(string message, Exception exception)
        {
            _log.Debug(message, exception);
        }

        public void Info(string message)
        {
            _log.Info(message);
        }

        public void Info(string message, Exception exception)
        {
            _log.Info(message, exception);
        }

        public void Warn(string message)
        {
            _log.Warn(message);
        }

        public void Warn(string message, Exception exception)
        {
            _log.Warn(message, exception);
        }

        public void Error(string message)
        {
            _log.Error(message);
        }

        public void Error(string message, Exception exception)
        {
            _log.Error(message, exception);
        }
    }
}