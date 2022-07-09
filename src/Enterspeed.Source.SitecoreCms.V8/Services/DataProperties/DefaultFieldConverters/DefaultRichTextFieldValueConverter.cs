using System;
using System.Collections.Generic;
using System.Linq;
using Enterspeed.Source.Sdk.Api.Models.Properties;
using Enterspeed.Source.SitecoreCms.V8.Models.Configuration;
using Enterspeed.Source.SitecoreCms.V8.Services.Contracts;
using HtmlAgilityPack;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Web.UI.WebControls;

namespace Enterspeed.Source.SitecoreCms.V8.Services.DataProperties.DefaultFieldConverters
{
    public class DefaultRichTextFieldValueConverter : IEnterspeedFieldValueConverter
    {
        private readonly IEnterspeedSitecoreFieldService _fieldService;

        public DefaultRichTextFieldValueConverter(
            IEnterspeedSitecoreFieldService fieldService)
        {
            _fieldService = fieldService;
        }

        public bool CanConvert(Field field)
        {
            return field != null && field.TypeKey.Equals("rich text", StringComparison.OrdinalIgnoreCase);
        }

        public IEnterspeedProperty Convert(Item item, Field field, EnterspeedSiteInfo siteInfo, List<IEnterspeedFieldValueConverter> fieldValueConverters, EnterspeedSitecoreConfiguration configuration)
        {
            string value = FieldRenderer.Render(item, field.Name);

            if (siteInfo != null)
            {
                value = PrefixRelativeImagesWithDomain(value, siteInfo.BaseUrl);

                value = PrefixRelativeLinksWithDomain(value, siteInfo.BaseUrl);
            }

            return new StringEnterspeedProperty(_fieldService.GetFieldName(field), value);
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
            if (imageNodes == null || !imageNodes.Any())
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
            if (anchorNodes == null || !anchorNodes.Any())
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

                if (!Uri.TryCreate(href, UriKind.RelativeOrAbsolute, out var uri))
                {
                    continue;
                }

                if (!uri.IsAbsoluteUri || href.StartsWith("/"))
                {
                    anchorNode.SetAttributeValue("href", new Uri(baseUri, href).ToString());
                }
            }

            return htmlDocument.DocumentNode.InnerHtml;
        }
    }
}