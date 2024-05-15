using System;
using System.Globalization;

namespace Enterspeed.Source.SitecoreCms.V9.Extensions
{
    public static class DateTimeExtensions
    {
        public static string ToSqlDateTime(this DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        }
    }
}