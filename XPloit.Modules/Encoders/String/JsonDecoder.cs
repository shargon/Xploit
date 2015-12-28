using System;
using XPloit.Core;
using XPloit.Core.Helpers;

namespace XPloit.Modules.Encoders.String
{
    public class JsonDecoder : XPloit.Core.Encoder
    {
        public override string Author { get { return "Fernando Díaz Toledano"; } }
        public override string Description { get { return "Parse json string to Object"; } }
        public override string Name { get { return "JsonDecoder"; } }
        public override string Path { get { return "Encoders/String"; } }

        /// <summary>
        /// Type
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public JsonDecoder() { }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type">Type</param>
        public JsonDecoder(Type type) { Type = type; }

        /// <summary>
        /// Encode
        /// </summary>
        /// <param name="input">Input</param>
        /// <returns>Return object</returns>
        public override object Run(Payload payload)
        {
            return SerializationJsonHelper.Deserialize(payload.StringValue, Type);
        }
    }
}