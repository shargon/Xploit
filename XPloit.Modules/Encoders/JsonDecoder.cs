using System;
using System.Text;
using XPloit.Core;
using XPloit.Core.Helpers;

namespace XPloit.Modules.Encoders
{
    public class JsonDecoder : XPloit.Core.Encoder
    {
        public override string Author { get { return "Fernando Díaz Toledano"; } }
        public override string Description { get { return "Parse json to Object"; } }
        public override string Name { get { return "JsonDecoder"; } }
        public override string Path { get { return "Encoders/String"; } }

        /// <summary>
        /// Type
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// Encode
        /// </summary>
        /// <param name="input">Input</param>
        /// <returns>Return object</returns>
        public override object Run(Payload payload)
        {
            return SerializationHelper.Deserialize(payload.StringValue, Type);
        }
    }
}