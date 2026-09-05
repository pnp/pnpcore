using PnP.Core.Provisioning.Utilities;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Providers.Xml.Resolvers;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace PnP.Core.Provisioning.Providers.Xml.Serializers
{
    /// <summary>
    /// Class to serialize/deserialize the Content Types
    /// </summary>
    [TemplateSchemaSerializer(SerializationSequence = 1000, DeserializationSequence = 1000,
        MinimalSupportedSchemaVersion = XMLPnPSchemaVersion.V201605,
        Scope = SerializerScope.ProvisioningTemplate)]
    internal class ContentTypesSerializer : PnPBaseSchemaSerializer<ContentType>
    {
        public override void Deserialize(object persistence, ProvisioningTemplate template)
        {
            var contentTypes = persistence.GetPublicInstancePropertyValue("ContentTypes");

            if (contentTypes != null)
            {
                var expressions = new Dictionary<Expression<Func<ContentType, Object>>, IResolver>
                {

                    { c => c.FieldRefs[0].Id, new FromStringToGuidValueResolver() },
                    { c => c.DocumentTemplate, new ExpressionValueResolver((s, v) => v.GetPublicInstancePropertyValue("TargetName")) },
                    { c => c.DocumentSetTemplate, new PropertyObjectTypeResolver<ContentType>(ct => ct.DocumentSetTemplate) },


                    { c => c.DocumentSetTemplate.AllowedContentTypes, new ExpressionCollectionValueResolver<ContentTypeReference>(
                        (s) => new ContentTypeReference {
                            ContentTypeId = s.GetPublicInstancePropertyValue("ContentTypeID").ToString(),
                            Name = s.GetPublicInstancePropertyValue("Name") != null ? s.GetPublicInstancePropertyValue("Name").ToString() : null,
                            Remove = s.GetPublicInstancePropertyValue("Remove") != null ? bool.Parse(s.GetPublicInstancePropertyValue("Remove").ToString()) : false
                        }) },

                    { c => c.DocumentSetTemplate.SharedFields[0].Id, new FromStringToGuidValueResolver() },
                };

                template.ContentTypes.AddRange(
                    PnPObjectsMapper.MapObjects<ContentType>(contentTypes,
                            new CollectionFromSchemaToModelTypeResolver(typeof(ContentType)),
                            expressions,
                            recursive: true)
                            as IEnumerable<ContentType>);
            }
        }

        public override void Serialize(ProvisioningTemplate template, object persistence)
        {
            if (template.ContentTypes != null && template.ContentTypes.Count > 0)
            {
                var baseNamespace = PnPSerializationScope.Current?.BaseSchemaNamespace;
                var contentTypeTypeName = $"{baseNamespace}.ContentType, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}";
                var contentTypeType = Type.GetType(contentTypeTypeName, true);
                var documentSetTemplateTypeName = $"{baseNamespace}.DocumentSetTemplate, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}";
                var documentSetTemplateType = Type.GetType(documentSetTemplateTypeName, true);
                var documentTemplateTypeName = $"{baseNamespace}.ContentTypeDocumentTemplate, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}";
                var documentTemplateType = Type.GetType(documentTemplateTypeName, true);

                var expressions = new Dictionary<string, IResolver>
                {

                    { $"{contentTypeType.FullName}.DocumentSetTemplate", new PropertyObjectTypeResolver(documentSetTemplateType, "DocumentSetTemplate") },
                    { $"{contentTypeType.FullName}.DocumentTemplate", new DocumentTemplateFromModelToSchemaTypeResolver(documentTemplateType) }
                };

                persistence.GetPublicInstanceProperty("ContentTypes")
                    .SetValue(
                        persistence,
                        PnPObjectsMapper.MapObjects(template.ContentTypes,
                            new CollectionFromModelToSchemaTypeResolver(contentTypeType), expressions, true));
            }
        }
    }
}
