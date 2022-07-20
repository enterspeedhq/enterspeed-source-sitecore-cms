using System;

namespace Enterspeed.Source.SitecoreCms.V8.Exceptions
{
    public static class EnumExtensions
    {
        public static int ToInt<T>(this T source) where T : IConvertible
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException("T must be an enumerated type");

            return (int)(IConvertible)source;
        }
    }
}