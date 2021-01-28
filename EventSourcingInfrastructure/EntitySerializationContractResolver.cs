using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using DomainBase;

namespace EventSourcingInfrastructure
{
    public class EntitySerializationContractResolver : DefaultContractResolver
    {
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            if (type.BaseType == typeof(Entity) || type.BaseType == typeof(AggregateRoot))
            {
                var props = type
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Select(p => base.CreateProperty(p, memberSerialization))
                    .ToList();

                props.ForEach(p => { p.Writable = true; p.Readable = true; });
                return props;
            }
            else 
            {
                return base.CreateProperties(type, memberSerialization);
            }
        }
    }
}
