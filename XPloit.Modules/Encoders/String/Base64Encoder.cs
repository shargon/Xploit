using System;
using System.Text;
using XPloit.Core;

namespace Encoders.String
{
    public class Base64Encoder : XPloit.Core.Encoder
    {
        public override string Author { get { return "Fernando Díaz Toledano"; } }
        public override string Description { get { return "Encode byte[] to base64 string"; } }

        /// <summary>
        /// Encode
        /// </summary>
        /// <param name="target">Target</param>
        /// <param name="payload">Payload</param>
        public override byte[] Run(Target target, Payload payload)
        {
            if (payload == null) return null;

            return Encoding.ASCII.GetBytes(Convert.ToBase64String(payload.GetValue(target)));
        }
    }
}