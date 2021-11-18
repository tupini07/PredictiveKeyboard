using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Reflection;

namespace Lib.Serialization
{
    public class ModelJsonContractResolver : Newtonsoft.Json.Serialization.DefaultContractResolver
    {
        public ModelJsonContractResolver()
        {
            this.IgnoreIsSpecifiedMembers = true;
        }


        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                            .Select(p => base.CreateProperty(p, memberSerialization))
                        .Union(type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                                   .Select(f => base.CreateProperty(f, memberSerialization)))
                        .ToList();

            props.ForEach(p =>
            {
                p.Writable = true;
                p.Readable = true;
                p.ShouldDeserialize = instance => true;
                p.ShouldSerialize = instance => true;
            });

            return props;
        }
    }
}
