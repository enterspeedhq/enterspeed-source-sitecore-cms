using Enterspeed.Source.SitecoreCms.V8.Services.Contracts;
using Sitecore.Data.Fields;

namespace Enterspeed.Source.SitecoreCms.V8.Services
{
    public class EnterspeedSitecoreFieldService : IEnterspeedSitecoreFieldService
    {
        public string GetFieldName(Field field)
        {
            if (field == null)
            {
                return null;
            }

            string fieldName = field.Name;
            if (string.IsNullOrEmpty(fieldName))
            {
                // If, for unexplainable reasons this should happen, we return null.
                return null;
            }

            fieldName = fieldName.Replace(" ", string.Empty); // i.e. "Link Text" becomes "LinkText"

            fieldName = fieldName.Trim(); // i.e. "LinkText " becomes "LinkText"

            fieldName = fieldName.ToLower(); // i.e. "LinkText" becomes "linktext"

            string sectionName = field.Section
                .Replace(" ", string.Empty)
                .ToLower();
            return $"{sectionName}_{fieldName}"; // i.e. becomes "content_linktext"
        }
    }
}