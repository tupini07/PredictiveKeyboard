﻿using Lib.Entities;
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
                ContractResolver = new ModelJsonContractResolver()
            });

            return Compressor.Zip(serialized);
        }

        public static T FromCompressedData(byte[] bytes)
        {
            var json = Compressor.Unzip(bytes);
            var deserialized = JsonConvert.DeserializeObject<T>(json, new JsonSerializerSettings
            {
                ContractResolver = new ModelJsonContractResolver()
            });

            if (deserialized == null)
                throw new Exception("Could not create model from compressed data");

            return deserialized;
        }

        public abstract void Hydrate(string corpus);
        public abstract List<Prediction> PredictNextOptions(string currentText);
        public abstract void Clear();
    }
}
