using System;
using System.Linq;
using Enterspeed.Source.Sdk.Api.Models.Properties;
using Enterspeed.Source.SitecoreCms.V9.Models.Configuration;
using HtmlAgilityPack;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Web.UI.WebControls;

namespace Enterspeed.Source.SitecoreCms.V9.Services.DataProperties.DefaultFieldConverters
{
    public class DefaultRichTextFieldValueConverter : IEnterspeedFieldValueConverter
    {
        public bool CanConvert(Field field)
        {
            return field != null && field.TypeKey.Equals("rich text", StringComparison.OrdinalIgnoreCase);
        }

        public IEnterspeedProperty Convert(Item item, Field field, EnterspeedSiteInfo siteInfo)
        {
            string value = FieldRenderer.Render(item, field.Name);

            value = PrefixRelativeImagesWithDomain(value, siteInfo.BaseUrl);

            value = PrefixRelativeLinksWithDomain(value, siteInfo.BaseUrl);

            return new StringEnterspeedProperty(field.Name, value);
        }

        private static string PrefixRelativeImagesWithDomain(string html, string baseUrl)
        {
            if (string.IsNullOrWhiteSpace(html) ||
                string.IsNullOrWhiteSpace(baseUrl))
            {
                return html;
            }

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);

            var imageNodes = htmlDocument.DocumentNode.SelectNodes("//img");
            if (imageNodes == null || imageNodes.Any() == false)
            {
                return html;
            }

            var baseUri = new Uri(baseUrl, UriKind.Absolute);
            foreach (var imageNode in imageNodes)
            {
                var src = imageNode.GetAttributeValue("src", string.Empty);
                if (src.StartsWith("-/media/") || src.StartsWith("~/media/"))
                {
                    imageNode.SetAttributeValue("src", new Uri(baseUri, src).ToString());
                }
            }

            return htmlDocument.DocumentNode.InnerHtml;
        }

        private static string PrefixRelativeLinksWithDomain(string html, string baseUrl)
        {
            if (string.IsNullOrWhiteSpace(html) ||
                string.IsNullOrWhiteSpace(baseUrl))
            {
                return html;
            }

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);

            var anchorNodes = htmlDocument.DocumentNode.SelectNodes("//a");
            if (anchorNodes == null || anchorNodes.Any() == false)
            {
                return html;
            }

            var baseUri = new Uri(baseUrl, UriKind.Absolute);
            foreach (var anchorNode in anchorNodes)
            {
                var href = anchorNode.GetAttributeValue("href", string.Empty);
                if (string.IsNullOrEmpty(href))
                {
                    continue;
                }

                if (Uri.TryCreate(href, UriKind.RelativeOrAbsolute, out var uri) == false)
                {
                    continue;
                }

                if (uri.IsAbsoluteUri == false || href.StartsWith("/"))
                {
                    anchorNode.SetAttributeValue("href", new Uri(baseUri, href).ToString());
                }
            }

            return htmlDocument.DocumentNode.InnerHtml;
        }
    }
}