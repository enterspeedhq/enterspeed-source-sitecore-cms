using System;

namespace Enterspeed.Source.SitecoreCms.V8.Services.Contracts
{
    public interface IEnterspeedSitecoreLoggingService
    {
        void Debug(string message);
        void Debug(string message, Exception exception);
        void Info(string message);
        void Info(string message, Exception exception);
        void Warn(string message);
        void Warn(string message, Exception exception);
        void Error(string message);
        void Error(string message, Exception exception);
    }
}