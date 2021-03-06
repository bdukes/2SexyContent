﻿using System.Collections.Generic;
using System.Linq;
using System.Web;
using ToSic.Eav;

namespace ToSic.SexyContent.DataImportExport.Extensions
{
    internal static class EntityExtension
    {
        /// <summary>
        /// Get the value of an attribute in the language specified.
        /// </summary>
        public static EavValue GetAttributeValue(this Entity entity, Attribute attribute, string language)
        {
            var values = entity.Values.Where(value => value.Attribute.StaticName == attribute.StaticName);
            if (string.IsNullOrEmpty(language))
            {
                return values.FirstOrDefault(value => !value.ValuesDimensions.Any());
            }
            else
            {
                var rootValue = values.FirstOrDefault();
                if (rootValue != null && rootValue.ValuesDimensions.Count == 0)
                {   // When we enable languages in 2sxc, but have not saved the content yet!
                    return rootValue;
                }
                return values.FirstOrDefault(value => value.ValuesDimensions.Any(reference => reference.Dimension.ExternalKey == language));
            }
        }

        /// <summary>
        /// Get the value of an attribute in the language specified.
        /// </summary>
        public static EavValue GetAttributeValue(this Entity entity, Attribute attribute, string language, string languageFallback)
        {
            var attributeValue = entity.GetAttributeValue(attribute, language);
            if (attributeValue == null)
            {
                attributeValue = entity.GetAttributeValue(attribute, languageFallback);
            }
            return attributeValue;
        }
    }
}