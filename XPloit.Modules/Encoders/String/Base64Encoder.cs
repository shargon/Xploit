using System;
using System.Text;
using XPloit.Core;
using XPloit.Core.Attributes;

namespace Encoders.String
{
    [ModuleInfo(Author = "Fernando Díaz Toledano", Description = "Encode byte[] to base64 string")]
    public class Base64Encoder : XPloit.Core.Encoder
    {
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