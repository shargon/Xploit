using System;
using XPloit.Core;

namespace XPloit.Modules.Encoders.String
{
    public class Base64Decoder : Encoder
    {
        public override string Author { get { return "Fernando Díaz Toledano"; } }
        public override string Description { get { return "Decode base64 string to byte[]"; } }
        public override string Name { get { return "Base64Decoder"; } }
        public override string Path { get { return "Encoders/String"; } }

        /// <summary>
        /// Constructor
        /// </summary>
        public Base64Decoder() { }

        /// <summary>
        /// Encode
        /// </summary>
        /// <param name="input">Input</param>
        /// <returns>Return object</returns>
        public override object Run(Payload payload)
        {
            if (payload == null) return null;
            return Convert.FromBase64String(payload.StringValue);
        }
    }
}