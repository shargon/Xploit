using System;
using XPloit.Core;

namespace XPloit.Modules.Encoders.String
{
    public class Base64Encoder : Encoder
    {
        public override string Author { get { return "Fernando Díaz Toledano"; } }
        public override string Description { get { return "Encode byte[] to base64 string"; } }
        public override string Name { get { return "Base64Decoder"; } }
        public override string Path { get { return "Encoders/String"; } }

        /// <summary>
        /// Constructor
        /// </summary>
        public Base64Encoder() { }

        /// <summary>
        /// Encode
        /// </summary>
        /// <param name="input">Input</param>
        /// <returns>Return object</returns>
        public override byte[] Run(Payload payload)
        {
            if (payload == null) return null;
            return payload.Encoding.GetBytes(Convert.ToBase64String(payload.Value));
        }
    }
}