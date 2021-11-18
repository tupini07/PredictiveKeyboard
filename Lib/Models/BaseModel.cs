using Lib.Serialization;
using Lib.Utils;
using Newtonsoft.Json;

namespace Lib.Models
{
    public abstract class BaseModel<T>
        where T : BaseModel<T>
    {
        public byte[] ToCompressedData()
        {
            var serialized = JsonConvert.SerializeObject(this, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                ContractResolver = new ModelJsonContractResolver()
            });

            return Compressor.Zip(serialized);
        }

        public static T FromCompressedData(byte[] bytes)
        {
            var json = Compressor.Unzip(bytes);
            var deserialized = JsonConvert.DeserializeObject<T>(json, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                ContractResolver = new ModelJsonContractResolver()
            });

            if (deserialized == null)
                throw new Exception("Could not create model from compressed data");

            return deserialized;
        }

    }
}
