using System;

namespace Enterspeed.Source.SitecoreCms.V8.Exceptions
{
    public static class DateTimeExtensions
    {
        public static string ToSqlDateTime(this DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}