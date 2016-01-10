using System;
using System.Text;
using XPloit.Core;

namespace XPloit.Modules.Encoders.String
{
    public class Base64Encoder : XPloit.Core.Encoder
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
        /// <param name="target">Target</param>
        /// <param name="payload">Payload</param>
        public override byte[] Run(Target target, Payload payload)
        {
            if (payload == null) return null;

            return Encoding.Default.GetBytes(Convert.ToBase64String(payload.GetValue(target)));
        }
    }
}